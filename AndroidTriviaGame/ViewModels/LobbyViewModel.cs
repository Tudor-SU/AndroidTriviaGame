using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AndroidTriviaGame.ViewModels;

public partial class LobbyViewModel:ObservableObject
{
    [ObservableProperty] private string _roomCode="";
    [ObservableProperty] private List<string> _players=[];
    [ObservableProperty] bool _isHost = true;
    
    MainWindowViewModel _mainWindowViewModel;
    
    public LobbyViewModel(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;
    }

    [RelayCommand]
    public void Leave()
    {
        _mainWindowViewModel.GameClient.Log("Leaving Lobby");
    }
    
    [RelayCommand]
    public void Start()
    {
        _mainWindowViewModel.GameClient.Log("Starting Lobby");
    }
}