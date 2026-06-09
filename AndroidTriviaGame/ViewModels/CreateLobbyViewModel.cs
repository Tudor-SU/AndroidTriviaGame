using System;
using System.Net.Sockets;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AndroidTriviaGame.ViewModels;

public partial class CreateLobbyViewModel:ObservableObject
{
    [ObservableProperty]
    private string _roomCode = "";
    
    MainWindowViewModel _mainWindowViewModel;
    
    public CreateLobbyViewModel(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;
    }
    
    [RelayCommand]
    public void Create()
    {
        try
        {
            NetworkStream? stream = _mainWindowViewModel.GameClient.GetNetworkStream();
            if (stream == null) return;
            
            _mainWindowViewModel.GameClient.Log("Sending request to create lobby:");

            NetworkingAPI.SendPacket(
                stream,
                PacketType.CreateLobbyRequest,
                _mainWindowViewModel.GameClient.Username
            );
            
        }
        catch (Exception e)
        {
            Console.WriteLine("Login failed: " + e.Message);
        }
        // _mainWindowViewModel.GameClient.Log($"Creating lobby");
        // NetworkingAPI.SendPacket(
        //     _mainWindowViewModel.GameClient.GetNetworkStream(),
        //     PacketType.CreateLobbyRequest
        //     );
    }

    [RelayCommand]
    public void JoinLobby()
    {
        _mainWindowViewModel.CurrentPage = new JoinLobbyViewModel(_mainWindowViewModel);
    }

    [RelayCommand]
    public void Back()
    {
        _mainWindowViewModel.CurrentPage = new MenuViewModel(_mainWindowViewModel);
    }
}