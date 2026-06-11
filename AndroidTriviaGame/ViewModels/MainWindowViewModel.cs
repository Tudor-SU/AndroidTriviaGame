using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json;

namespace AndroidTriviaGame.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableObject _currentPage;
    
    private GameClient _gameClient;
    public GameClient GameClient => _gameClient;

    private void HandleLoginResponse(PacketReceivedEventArgs e)
    {
        var result = JsonSerializer.Deserialize<ResponseStatus>(e.Packet.Data);

        if (result is null) return;
                
        _gameClient.Log($"Login Response: {result}");
        if (CurrentPage is LoginViewModel lvm )
        {
            if (!result.IsSuccess)
            {
                lvm.ErrMsg = result.Message;
                return;
            }
            _gameClient.Username = lvm.Username;
            CurrentPage = new MenuViewModel(this);
        }
    }

    private void HandleRegisterResponse(PacketReceivedEventArgs e)
    {
        var result = JsonSerializer.Deserialize<ResponseStatus>(e.Packet.Data);

        if (result is null) return;
                
        _gameClient.Log($"Register Response: {result}");
        if (CurrentPage is RegisterViewModel rvm )
        {
            if (!result.IsSuccess)
            {
                rvm.ErrMsg = result.Message;
                return;
            }

            LoginViewModel lvm = new LoginViewModel(this);
            lvm.Username = rvm.Username;
            lvm.Password = rvm.Password;
            CurrentPage = lvm;
        }
    }

    private void HandleCreateLobbyResponse(PacketReceivedEventArgs e)
    {
        var result = JsonSerializer.Deserialize<ResponseStatus>(e.Packet.Data);
        _gameClient.Log($"Create Lobby Response: {result}");

        if (result is null || !result.IsSuccess) return;

        if (CurrentPage is CreateLobbyViewModel)
        {
            LobbyViewModel lvm = new LobbyViewModel(this);
            lvm.RoomCode = result.Message;
            lvm.IsHost = true;
            lvm.Players = [_gameClient.Username];
            CurrentPage = lvm;
        }
    }

    private void HandleJoinLobbyResponse(PacketReceivedEventArgs e)
    {
        var result = JsonSerializer.Deserialize<JoinLobbyResponse>(e.Packet.Data);
        _gameClient.Log($"Create Lobby Response: {result}");
        if(result is null) return;

        if (CurrentPage is JoinLobbyViewModel jvm)
        {
            if (!result.Status.IsSuccess)
            {
                jvm.ErrMsg = result.Status.Message;
                return;
            }

            LobbyViewModel lvm = new(this);
            lvm.Players = new System.Collections.ObjectModel.ObservableCollection<string>(
                result.PlayerNames ?? []
            );
            
            lvm.Players.Add(_gameClient.Username);
            lvm.RoomCode = result.LobbyCode ?? "";
            GameClient.Log($"Code:  {jvm.LobbyCode}");
            CurrentPage = lvm;
            
        }
    }

    private void HandlePlayerJoinedUpdate(PacketReceivedEventArgs e)
    {
        var name = JsonSerializer.Deserialize<string>(e.Packet.Data);
        if (name is null) return;
        
        _gameClient.Log($"Player joined update: {name}");
        if (CurrentPage is LobbyViewModel lvm)
        {
            lvm.Players.Add(name);
        }

    }
    
    private void HandlePlayerLeftUpdate(PacketReceivedEventArgs e)
    {
        var name = JsonSerializer.Deserialize<string>(e.Packet.Data);
        if (name is null) return;
        
        _gameClient.Log($"Player left update: {name}");
        if (CurrentPage is LobbyViewModel lvm)
        {
            lvm.Players.Remove(name);
        }
        
        

    }
    
    private void HandleHostLeftUpdate(PacketReceivedEventArgs e)
    {
        var name = JsonSerializer.Deserialize<string>(e.Packet.Data);
        if (name is null) return;
        
        _gameClient.Log($"Host left update: {name}");
        if (CurrentPage is LobbyViewModel lvm)
        {
            lvm.LobbyWasCanceled = true;
        }

    }

    private void HandleGameStateUpdate(PacketReceivedEventArgs e)
    {
        var data = JsonSerializer.Deserialize<GameStateUpdate>(e.Packet.Data);
        if (data is null) return;

        if (CurrentPage is LobbyViewModel)
        {
            CurrentPage = new GameViewModel(this);
        }

        // _gameClient.Log($"Game State Update: {data}");
        if (CurrentPage is GameViewModel gvm)
        {
            gvm.CurrentQuestionText = data.CurrentQuestion.QuestionText;
            gvm.CurrentAnswers = data.CurrentQuestion.Answers;
            gvm.QuestionProgress = $"Question {data.CurrentQuestionIndex+1}/{data.TotalQuestions}";
            gvm.HasSubmittedAnswer = false;
            
            List<string> playerList = data.StatusTable.Keys.ToList();
            
            gvm.Player1Text = data.StatusTable.Count > 0 ? 
                $"{playerList[0]}: {data.StatusTable[playerList[0]]}" : "";
            
            gvm.Player2Text = data.StatusTable.Count > 1 ? 
                $"{playerList[1]}: {data.StatusTable[playerList[1]]}" : "";
            
            gvm.Player3Text = data.StatusTable.Count > 2 ? 
                $"{playerList[2]}: {data.StatusTable[playerList[2]]}" : "";
            
            gvm.Player4Text = data.StatusTable.Count > 3 ? 
                $"{playerList[3]}: {data.StatusTable[playerList[3]]}" : "";
        }
        
    }

    private void HandleShowStatsUpdate(PacketReceivedEventArgs e)
    {
        var data = JsonSerializer.Deserialize<Dictionary<string,int>>(e.Packet.Data);
        if (data is null) return;

        CurrentPage = new StatsViewModel(this, data);

    }


    private void HandlePacket(object? sender, PacketReceivedEventArgs e)
    {
        // Console.WriteLine($"Handle Packet received: {e.Packet.Type}");
        
        switch (e.Packet.Type)
        {
            case PacketType.LoginResponse:
                HandleLoginResponse(e);
                break;
            
            case PacketType.RegisterResponse:
                HandleRegisterResponse(e);
                break;
            
            case PacketType.CreateLobbyResponse:
                HandleCreateLobbyResponse(e);
                break;
            
            case PacketType.JoinLobbyResponse:
                HandleJoinLobbyResponse(e);
                break;
            
            case PacketType.PlayerJoinedUpdate:
                HandlePlayerJoinedUpdate(e);
                break;
            
            case PacketType.PlayerLeftUpdate:
                HandlePlayerLeftUpdate(e);
                break;
            
            case PacketType.HostLeftUpdate:
                HandleHostLeftUpdate(e);
                break;
            
            case PacketType.GameStateUpdate:
                HandleGameStateUpdate(e);
                break;
            
            case PacketType.ShowStatsUpdate:
                HandleShowStatsUpdate(e);
                break;
        }
    }

    public MainWindowViewModel(GameClient gameClient)
    {
        _gameClient = gameClient;
        _currentPage = new LoginViewModel(this);
        // Dictionary<string, int> playerScores = new Dictionary<string, int>
        // {
        //     { "Alex", 50 },
        //     { "Steve", 20 },
        //     { "Maria", 80 }
        // };
        // _currentPage = new StatsViewModel(this, playerScores);
        _gameClient.PacketReceived +=  HandlePacket; 
    }
    
    public void LoadLoginPage()
    {
        CurrentPage = new LoginViewModel(this);
    }
    


    
}