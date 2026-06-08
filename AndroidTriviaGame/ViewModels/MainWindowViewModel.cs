using System;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json;

namespace AndroidTriviaGame.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableObject _currentPage;
    
    private GameClient _gameClient;
    public GameClient GameClient => _gameClient;

    private void HandlePacket(object? sender, PacketReceivedEventArgs e)
    {
        switch (e.Packet.Type)
        {
            case PacketType.LoginResponse:
                var result = JsonSerializer.Deserialize<LoginResponse>(e.Packet.Data);

                if (result is null) return;
                
                Console.WriteLine($"Login Response: {result}");
                if (CurrentPage is LoginViewModel lvm)
                {
                    _gameClient.Username = lvm.Username;
                    _gameClient.UId = result.UId;
                    LoadMenuPage();
                }
                
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
    
    public void LoadMenuPage()
    {
        CurrentPage = new MenuViewModel(this);
    }

    
}