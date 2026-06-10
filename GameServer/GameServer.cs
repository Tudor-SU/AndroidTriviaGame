using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace AndroidTriviaGame;

public static class DatabaseInitializer
{
    private const string ConnectionString = "Data Source=trivia_game.db";

    public static void InitializeDatabase()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        
        command.CommandText = @"
            DROP TABLE IF EXISTS Users; 
            
            CREATE TABLE Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL UNIQUE,
                PasswordHash TEXT NOT NULL
            );";

        command.ExecuteNonQuery();
        Console.WriteLine("Database initialized successfully! (File: trivia_game.db)");
    }
    
    public static SqliteConnection? EstablishDbConnection()
    {
        try
        {
            var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            return connection;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
    }
}

public record ClientData(string Name, NetworkStream Stream);

public class LobbyData
{
    public Dictionary<string, int> StatusTable { get; set; }
    public Dictionary<string, int> Answers { get; set; }
    public string Code { get; set; }
    public List<QuizQuestion> Questions { get; set; }
    public int QuestionIndex { get; set; }
    public string HostName { get; set; }
    public bool Started { get; set; }

    public int MaxPlayerCount { get; } = 4;

    public LobbyData(
        string code, 
        List<QuizQuestion> questions, 
        string hostName
        )
    {
        StatusTable = new ();
        Answers = new();
        Code = code;
        Questions = questions;
        QuestionIndex = 0;
        HostName = hostName;
        Started = false;
        StatusTable.Add(hostName, 0);
    }

    public GameStateUpdate GenerateGameStateUpdate()
    {
        return new GameStateUpdate(
            StatusTable, Questions.ElementAt(QuestionIndex),
            QuestionIndex, Questions.Count()
        );
    }

    public List<String> GetPlayerNames()
    {
        return StatusTable.Keys.ToList();
    }

    public int GetPlayerCount()
    {
        return GetPlayerNames().Count;
    }

    public override string ToString()
    {
        string result = $"\tCode: {Code}\n\tHost: {HostName}\n\tPlayers: ";
        foreach (var name in GetPlayerNames())
        {
            result += $"{name} | ";
        }

        return result;
    } 
}
public class GameServer
{
    
    private static SqliteConnection? _dbConnection;
    private static List<ClientData> _clients = new();
    private static List<LobbyData> _lobbies = new();

    private static bool IsConnected(string clientName)
    {
        foreach (var client in _clients)
        {
            if (client.Name == clientName) return  true;
        }
        return false;
    }

    private static LobbyData? SearchLobby(string lobbyCode)
    {
        foreach (var lobby in _lobbies)
        {
            if (lobby.Code == lobbyCode) return lobby;
        }
        return null;
    }

    private static LobbyData? SearchLobbyContainingPlayer(string playerName)
    {
        foreach (var lobby in _lobbies)
        {
            if (lobby.GetPlayerNames().Contains(playerName)) return  lobby;
        }
        return null;
    }

    private static NetworkStream? SearchClientStream(string clientName)
    {
        foreach (var clientData in _clients)
        {
            if (clientData.Name == clientName) return clientData.Stream;
        }

        return null;
    }
    
    private static void ProcessLoginRequest(NetworkStream stream, Packet packet)
    {
        Credentials? credentials = JsonSerializer.Deserialize<Credentials>(packet.Data);    
        Console.WriteLine(credentials);

        if (
            credentials is null ||
            string.IsNullOrWhiteSpace(credentials.Name) ||
            string.IsNullOrWhiteSpace(credentials.Password)
        )
        {
            NetworkingAPI.SendPacket(
                stream, PacketType.LoginResponse,
                new ResponseStatus(false, "Invalid credentials provided")
            );
            return;
        }
        
        using var command = new SqliteCommand("SELECT ...", _dbConnection);        
        command.CommandText = "SELECT PasswordHash FROM Users WHERE Username = $user";
        command.Parameters.AddWithValue("$user", credentials.Name);

        try
        {
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                string storedHash = reader.GetString(0);
                string inputHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(credentials.Password)));

                if (string.Equals(inputHash, storedHash, StringComparison.OrdinalIgnoreCase))
                {
                    if (IsConnected(credentials.Name))
                    {
                        NetworkingAPI.SendPacket(
                            stream, PacketType.LoginResponse,
                            new ResponseStatus(false, "User already connected")
                        );
                        return;
                    }
                    
                    NetworkingAPI.SendPacket(
                        stream, PacketType.LoginResponse,
                        new ResponseStatus(true, "Success")
                    );
                    Console.WriteLine($"\n[AUTH]: User '{credentials.Name}' successfully authenticated.");
                    _clients.Add(new  ClientData(credentials.Name, stream));
                    return;
                }
            }

            NetworkingAPI.SendPacket(
                stream, PacketType.LoginResponse,
                new ResponseStatus(false, "Invalid credentials provided")
            );
        }
        catch (Exception e)
        {
            NetworkingAPI.SendPacket(
                stream, PacketType.LoginResponse,
                new ResponseStatus(false, e.Message)
            );
        }

        
    }

    private static void ProcessRegisterRequest(NetworkStream stream, Packet packet)
    {
        Credentials? credentials = JsonSerializer.Deserialize<Credentials>(packet.Data);    
        
        if (
            credentials is null ||
            string.IsNullOrWhiteSpace(credentials.Name) ||
            string.IsNullOrWhiteSpace(credentials.Password)
        ) {
            NetworkingAPI.SendPacket(
                stream, PacketType.RegisterResponse,
                new ResponseStatus(false, "Invalid credentials provided")    
            );
            return;
        }
        
        string hashedPassword = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(credentials.Password))
        );
        
        var command = _dbConnection!.CreateCommand();
        command.CommandText = "INSERT INTO Users (Username, PasswordHash) VALUES ($user, $pass)";
        command.Parameters.AddWithValue("$user", credentials.Name);
        command.Parameters.AddWithValue("$pass", hashedPassword);

        try
        {
            command.ExecuteNonQuery();
            Console.WriteLine($"[DB]: New user registered successfully: {credentials.Name}");

            NetworkingAPI.SendPacket(
                stream, PacketType.RegisterResponse,
                new ResponseStatus(true, "Success")
            );

        }
        catch (SqliteException e)
        {
            if (e.SqliteErrorCode == 19)
            {
                NetworkingAPI.SendPacket(
                    stream, PacketType.RegisterResponse,
                    new ResponseStatus(false, "Username is already taken")
                );
                return;
            }

            NetworkingAPI.SendPacket(
                stream, PacketType.RegisterResponse,
                new ResponseStatus(false, e.Message)
            );
        }
        catch (Exception e)
        {
            NetworkingAPI.SendPacket(
                stream, PacketType.RegisterResponse,
                new ResponseStatus(false, e.Message)
            );
        }
        
    }

    private static string GenerateLobbyCode()
    {
        Random rnd = new Random();
        string code = "";

        while (code.Length < 6)
        {
            code += rnd.Next(0, 10).ToString();
        }

        foreach (var lobbyData in _lobbies)
        {
            if (lobbyData.Code == code)
            {
                return GenerateLobbyCode();
            }    
        }

        return code;
    }
    
    private static void ProcessCreateLobbyRequest(NetworkStream stream, Packet packet)
    {
        string code = GenerateLobbyCode();
        var questions = QuizManager.ExtractQuestions();
        
        Console.WriteLine($"[CREATE]: Successfully created lobby code: {code}");
        
        string? hostName = JsonSerializer.Deserialize<string>(packet.Data);

        if (hostName is null)
        {
            Console.WriteLine($"[CREATE]: Failed to create lobby code: {code}: hostName is null");
            return;
        }
        
        _lobbies.Add(new  LobbyData(
            code, questions, hostName    
        ));
        
        NetworkingAPI.SendPacket(
            stream, PacketType.CreateLobbyResponse,
            new ResponseStatus(true, code)
        );
    }

    private static void ProcessJoinLobbyRequest(NetworkStream stream, Packet packet)
    {
        var data =  JsonSerializer.Deserialize<JoinLobbyInfo>(packet.Data);
        if (data is null) return;
        
        LobbyData? lobbyData = SearchLobby(data.LobbyCode);

        if (lobbyData is null)
        {
            NetworkingAPI.SendPacket(
                stream, PacketType.JoinLobbyResponse,
                new JoinLobbyResponse(
                    new ResponseStatus(false, $"Lobby not found"),
                    null, null
                )
            );
            return;
        }

        if (lobbyData.Started)
        {
            NetworkingAPI.SendPacket(
                stream, PacketType.JoinLobbyResponse,
                new JoinLobbyResponse(
                    new ResponseStatus(false, $"Quiz already started"),
                    null, null
                )
            );
            return;
        }

        if (lobbyData.GetPlayerCount() == lobbyData.MaxPlayerCount)
        {
            NetworkingAPI.SendPacket(
                stream, PacketType.JoinLobbyResponse,
                new JoinLobbyResponse(
                    new ResponseStatus(false, $"Lobby is full"),
                    null, null
                )
            );
            return;
        }
        
        NetworkingAPI.SendPacket(
            stream, PacketType.JoinLobbyResponse,
            new JoinLobbyResponse(
                new ResponseStatus(true, $"Success"),
                lobbyData.GetPlayerNames(), lobbyData.Code
            )
        );
        
        Console.WriteLine($"\n[JOIN]: {data.PlayerName} joined lobby {data.LobbyCode}");
        
        foreach (var playerName in lobbyData.GetPlayerNames())
        {
            NetworkStream? playerStream = SearchClientStream(playerName);
            if (playerStream is null) continue;
                
            NetworkingAPI.SendPacket(
                playerStream, PacketType.PlayerJoinedUpdate,
                data.PlayerName
            );
        }
        
        lobbyData.StatusTable.Add(data.PlayerName, 0);
    }

    private static void ProcessLeaveLobbyUpdate(NetworkStream stream, Packet packet)
   
    {
        var name =  JsonSerializer.Deserialize<string>(packet.Data);
        if (name is null) return;
        
        LobbyData? lobbyData = SearchLobbyContainingPlayer(name);

        if (lobbyData is null) return;
        
        lobbyData.StatusTable.Remove(name);
        
        if (lobbyData.HostName == name && !lobbyData.Started)
        {
            Console.WriteLine($"\n[LEAVE]: Host {name} left the lobby\nLobby {lobbyData.Code} was canceled");
            
            foreach (var playerName in lobbyData.GetPlayerNames())
            {
                NetworkStream? playerStream = SearchClientStream(playerName);
                if (playerStream is null) continue;
                
                NetworkingAPI.SendPacket(
                    playerStream, PacketType.HostLeftUpdate,
                    lobbyData.HostName
                );
            }
            _lobbies.Remove(lobbyData);
        }

        else
        {
            Console.WriteLine($"\n[LEAVE]: Player {name} left the lobby");

            if (!lobbyData.Started)
            {
                foreach (var playerName in lobbyData.GetPlayerNames())
                {
                    NetworkStream? playerStream = SearchClientStream(playerName);
                    if (playerStream is null) continue;

                    NetworkingAPI.SendPacket(
                        playerStream, PacketType.PlayerLeftUpdate,
                        name
                    );
                }

                return;
            }

            if (lobbyData.GetPlayerCount() == 0)
            {
                Console.WriteLine($"\n[Lobby {lobbyData.Code}] All players left. Game is canceled");
                _lobbies.Remove(lobbyData);
            }
            
            if (lobbyData.Answers.Count == lobbyData.GetPlayerCount())
            {
                lobbyData.QuestionIndex++;
                if (lobbyData.QuestionIndex == lobbyData.Questions.Count)
                {
                    Console.WriteLine($"\n[Lobby {lobbyData.Code}] Game finished");
                    SendShowStatsUpdate(lobbyData);
                    _lobbies.Remove(lobbyData);
                    
                    return;
                }

                lobbyData.Answers = new();
            }
            SendGameStateUpdate(lobbyData);
            
        }
     
    }

    private static void SendGameStateUpdate(LobbyData lobbyData)
    {
        GameStateUpdate gameStateUpdate = lobbyData.GenerateGameStateUpdate();
        
        foreach (var playerName in lobbyData.GetPlayerNames())
        {
            NetworkStream? playerStream = SearchClientStream(playerName);
            if (playerStream is null) continue;
            
            Console.WriteLine($"Sending update to {playerName}");
            NetworkingAPI.SendPacket(
                playerStream, PacketType.GameStateUpdate,
                gameStateUpdate
            );
        }
    }

    private static void SendShowStatsUpdate(LobbyData lobbyData)
    {
        foreach (var playerName in lobbyData.GetPlayerNames())
        {
            NetworkStream? playerStream = SearchClientStream(playerName);
            if (playerStream is null) continue;
            
            NetworkingAPI.SendPacket(
                playerStream, PacketType.ShowStatsUpdate,
                lobbyData.StatusTable
            );
            
            
        }
    }

    private static void ProcessStartGameRequest(NetworkStream stream, Packet packet)
    {
        var code =  JsonSerializer.Deserialize<string>(packet.Data);
        if (code is null) return;
        
        LobbyData?  lobbyData = SearchLobby(code);
        if (lobbyData is null) return;
        
        lobbyData.Started = true;
        SendGameStateUpdate(lobbyData);
    }

    private static void ProcessSubmitAnswerUpdate(NetworkStream stream, Packet packet)
    {
        var data = JsonSerializer.Deserialize<AnswerUpdate>(packet.Data);
        if(data is null) return;
        LobbyData? lobbyData = SearchLobbyContainingPlayer(data.PlayerName);
        if (lobbyData is null) return;
        
        lobbyData.Answers[data.PlayerName] = data.AnswerIndex;
        Console.WriteLine($"\n[Lobby {lobbyData.Code}] : {data.PlayerName} answered {data.AnswerIndex}");

        if (data.AnswerIndex == lobbyData.Questions[lobbyData.QuestionIndex].CorrectIndex)
        {
            lobbyData.StatusTable[data.PlayerName]++;
        }
        
        if (lobbyData.Answers.Count == lobbyData.GetPlayerCount())
        {
            lobbyData.QuestionIndex++;
            if (lobbyData.QuestionIndex == lobbyData.Questions.Count)
            {
                Console.WriteLine($"\n[Lobby {lobbyData.Code}] Game finished");
                SendShowStatsUpdate(lobbyData);
                _lobbies.Remove(lobbyData);
                return;
            }

            lobbyData.Answers = new();
            SendGameStateUpdate(lobbyData);
        }
    }

    private static void ProcessPacket(NetworkStream stream, Packet packet)
    {
        switch (packet.Type)
        {
            case  PacketType.LoginRequest:
                ProcessLoginRequest(stream, packet);        
                break;
            
            case PacketType.RegisterRequest:
                ProcessRegisterRequest(stream, packet);
                break;
            
            case PacketType.CreateLobbyRequest:
                ProcessCreateLobbyRequest(stream, packet);
                break;
            
            case PacketType.JoinLobbyRequest:
                ProcessJoinLobbyRequest(stream, packet);
                break;
            
            case PacketType.LeaveLobbyUpdate:
                ProcessLeaveLobbyUpdate(stream, packet);
                break;
            
            case PacketType.StartGameRequest:
                ProcessStartGameRequest(stream, packet);
                break;
            
            case PacketType.SubmitAnswerUpdate:
                ProcessSubmitAnswerUpdate(stream, packet);
                break;
        }    
    }
    
    public static void ThreadedHandleClient(object? data)
    {
        if (data == null)
        {
            return;
        }
        
        using var client =  (TcpClient)data;
        Console.WriteLine($"Client connected: {client.Client.RemoteEndPoint}");
        NetworkStream stream = client.GetStream();
        
        try
        {
        
            while (client.Connected)
            {
                Packet? packet = NetworkingAPI.ReceivePacket(stream);
                
                if (packet == null)
                {
                    Console.WriteLine($"Client disconnected gracefully: {client.Client.RemoteEndPoint}");
                    _clients.RemoveAll(c => c.Stream == stream);
                    stream.Close();
                    break; 
                }
            
                Console.WriteLine($"Packet received: {packet.Type.ToString()}");
                ProcessPacket(stream, packet);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Client disconnected abruptly ({client.Client.RemoteEndPoint}): {e.Message}");
            _clients.RemoveAll(c => c.Stream == stream);
            stream.Close();
        }
        
    }
    

    public static void ThreadedAcceptConnections(object? data)
    {
        if (data == null)
        {
            return;
        }
        
        var tuple = (Tuple<IPAddress, int>)data;
        IPAddress ip = tuple.Item1;
        int port = tuple.Item2;
        
        TcpListener listener = new TcpListener(ip, port);
        listener.Start();
        
        
        Console.WriteLine($"\nStarting server on {ip}:{port}");
        Console.WriteLine("\nListening for connections:");

        while (true)
        {
            try
            {
                TcpClient client = listener.AcceptTcpClient() ;
                Thread handleClientThread = new Thread(ThreadedHandleClient);
                handleClientThread.IsBackground = true;
                handleClientThread.Start(client);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
    
    public static string GetLocalIPAddress()
    {
        string hostName = Dns.GetHostName();
    
        IPAddress[] addresses = Dns.GetHostAddresses(hostName);

        foreach (var ip in addresses)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }

        return "none";
    }

    public static int? SearchPort(string[] args)
    {
        string? portArg = args.FirstOrDefault(a => a.StartsWith("port=", StringComparison.OrdinalIgnoreCase));

        if (portArg != null)
        {
            string[] parts = portArg.Split('=');

            if (parts.Length == 2 && int.TryParse(parts[1], out int parsedPort))
            {
                return parsedPort;
            }
            
            Console.WriteLine("Invalid port format provided in args. Using default port.");
            
        }
        return null;
    }
    
    public static void Main(string[] args)
    {
        string localIP = GetLocalIPAddress();
        if (localIP == "none")
        {
            Console.WriteLine("No IPv4 address found for this system!");
            return;
        }
        
        IPAddress ip = IPAddress.Parse(localIP);
        int port = SearchPort(args) ?? 5000;

        if (args.Contains("init_db"))
        {
            DatabaseInitializer.InitializeDatabase();
        }

        _dbConnection = DatabaseInitializer.EstablishDbConnection();
        if (_dbConnection is null)
        {
            Console.WriteLine("\nNo DB connection established.");
            return;
        }
        Console.WriteLine("\nDB connection established.");

        try
        {
            int count = QuizManager.LoadQuestions("questions.json");
            if (count == 0)
            {
                Console.WriteLine("[ERROR] No quiz questions found at questions.json");
                return;
            }
            Console.WriteLine($"[QUIZ]: Successfully loaded {count} questions");

        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return;
        }
        
        Thread acceptConnectionsThread = new Thread(ThreadedAcceptConnections);
        acceptConnectionsThread.IsBackground = true;
        acceptConnectionsThread.Start(Tuple.Create(ip, port));
        
        string? command;
        while (true)
        {
            command = Console.ReadLine();
            
            if (command == "/q")
            {
                break;
            }

            if (command == "/c")
            {
                Console.WriteLine($"\nLogged clients: {_clients.Count}");
                foreach (var client in _clients)
                {
                    Console.WriteLine($"\t{client.Name}");
                }
                continue;
            }

            if (command == "/l")
            {
                Console.WriteLine($"\nCurrent lobbies: {_lobbies.Count}");
                foreach (var lobby in _lobbies)
                {
                    Console.WriteLine(lobby);
                }
                continue;
            }
            
            Console.WriteLine("\nUnknown command!");
        }
        
        _dbConnection.Close();
        Console.WriteLine("\nServer stopped.");
    }    
}