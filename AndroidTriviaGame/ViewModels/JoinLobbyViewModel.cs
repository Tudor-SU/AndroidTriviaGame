using System;
using System.Net.Sockets;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AndroidTriviaGame.ViewModels;

public partial class JoinLobbyViewModel:ObservableObject
{
    [ObservableProperty]
    private string _lobbyCode = "";
    
    [ObservableProperty]
    private string _errMsg = "";
    
    
    MainWindowViewModel _mainWindowViewModel;
    
    public JoinLobbyViewModel(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;
    }
    
    [RelayCommand]
    public void Join()
    {
        _mainWindowViewModel.GameClient.Log($"Joining lobby {LobbyCode}");
        NetworkStream? stream = _mainWindowViewModel.GameClient.GetNetworkStream();
        if (stream is null) return;
        
        NetworkingAPI.SendPacket(
            stream, PacketType.JoinLobbyRequest,
            new JoinLobbyInfo(_mainWindowViewModel.GameClient.Username, LobbyCode)
        );
    }

    [RelayCommand]
    public void CreateLobby()
    {
     _mainWindowViewModel.CurrentPage = new CreateLobbyViewModel(_mainWindowViewModel);   
    }

    [RelayCommand]
    public void Back()
    {
        _mainWindowViewModel.CurrentPage = new MenuViewModel(_mainWindowViewModel);
    }
}