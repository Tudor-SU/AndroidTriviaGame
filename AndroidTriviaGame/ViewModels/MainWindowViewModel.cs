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

    private void HandlePacket(object? sender, PacketReceivedEventArgs e)
    {
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
        }
    }

    public MainWindowViewModel(GameClient gameClient)
    {
        _gameClient = gameClient;
        _currentPage = new LoginViewModel(this);
        _gameClient.PacketReceived +=  HandlePacket; 
    }
    
    public void LoadLoginPage()
    {
        CurrentPage = new LoginViewModel(this);
    }
    


    
}