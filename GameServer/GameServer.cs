using System.Net;
using System.Net.Sockets;
using System.Text.Json;


namespace AndroidTriviaGame;

public class GameServer
{

    private static string ValidateCredentials(LoginCredentials? credentials)
    {
        // TODO: load from database
        if (credentials == null) return "";

        if (credentials.Name == "john" && credentials.Password == "123")
        {
            return "AJHGJ";
        }
        
        return "";
    }
    
    private static void ProcessPacket(NetworkStream stream, Packet packet)
    {
        switch (packet.Type)
        {
            case  PacketType.LoginCredentials:
                LoginCredentials? credentials = JsonSerializer.Deserialize<LoginCredentials>(packet.Data);    
                Console.WriteLine(credentials);
                string response = ValidateCredentials(credentials);
                
                NetworkingAPI.SendPacket(
                    stream, PacketType.LoginResponse,
                    new LoginResponse(response)
                );
                
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
        try
        {
            using NetworkStream stream = client.GetStream();
        
            // Loop continuously to keep reading packets over the same open stream
            while (client.Connected)
            {
                Packet? packet = NetworkingAPI.ReceivePacket(stream);

                // If ReceivePacket returns null, it almost always means 
                // the client gracefully closed the connection from their side.
                if (packet == null)
                {
                    Console.WriteLine($"Client disconnected gracefully: {client.Client.RemoteEndPoint}");
                    break; 
                }
            
                Console.WriteLine($"Packet received: {packet.Type.ToString()}");
                ProcessPacket(stream, packet);
            }
        }
        catch (Exception e)
        {
            // Catches unexpected disconnects, timeout errors, or broken network pipes
            Console.WriteLine($"Client disconnected abruptly ({client.Client.RemoteEndPoint}): {e.Message}");
        }
        // The 'using' keywords on 'client' and 'stream' will automatically clean up 
        // and close the sockets safely when this method finishes or throws an error!
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
        
        
        Console.WriteLine($"Starting server on {ip}:{port}");
        Console.WriteLine($"Listening for connections:");

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
                Console.WriteLine("raba bad");

                Console.WriteLine(e);
            }
        }
    }
    
    public static string GetLocalIPAddress()
    {
        // Get the host name of the current machine
        string hostName = Dns.GetHostName();
    
        // Get all IP addresses associated with this host
        IPAddress[] addresses = Dns.GetHostAddresses(hostName);

        foreach (var ip in addresses)
        {
            // Filter out IPv6 addresses so you only get IPv4
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }

        return "none";
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
        int port = 5000;
        
        if(args.Length > 0)
            try
            { 
                port = Int32.Parse(args[0]);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }

        string? command ;
        Thread acceptConnectionsThread = new Thread(ThreadedAcceptConnections);
        acceptConnectionsThread.IsBackground = true;
        acceptConnectionsThread.Start(Tuple.Create(ip, port));
        
        while (true)
        {
            command = Console.ReadLine();
            
            if (command == "q")
            {
                break;
            }
        }
        
        //TODO server stopping stuff
    }    
}