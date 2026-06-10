using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AndroidTriviaGame.ViewModels;

public partial class LobbyViewModel:ObservableObject
{
    [ObservableProperty] private string _roomCode="";
    [ObservableProperty] private ObservableCollection<string> _players=[];
    [ObservableProperty] private bool _isHost = false;
    [ObservableProperty] private bool _lobbyWasCanceled = false;
    
    MainWindowViewModel _mainWindowViewModel;
    
    public LobbyViewModel(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;
    }

    [RelayCommand]
    public void Leave()
    {
        _mainWindowViewModel.GameClient.Log("Leaving Lobby");
        _mainWindowViewModel.CurrentPage = new JoinLobbyViewModel(_mainWindowViewModel);
        
        if(LobbyWasCanceled) return;
        
        NetworkStream? stream = _mainWindowViewModel.GameClient.GetNetworkStream();
        if (stream is null) return;
        
        NetworkingAPI.SendPacket(
            stream, PacketType.LeaveLobbyUpdate,
            _mainWindowViewModel.GameClient.Username
        );
    }
    
    [RelayCommand]
    public void Start()
    {
        _mainWindowViewModel.GameClient.Log("Starting Lobby");

        NetworkStream? stream = _mainWindowViewModel.GameClient.GetNetworkStream();
        if(stream is null) return;
        
        NetworkingAPI.SendPacket(
            stream, PacketType.StartGameRequest,
            RoomCode
        );

    }
}