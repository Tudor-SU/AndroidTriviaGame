using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AndroidTriviaGame.ViewModels;

public partial class JoinLobbyViewModel:ObservableObject
{
    [ObservableProperty]
    private string _lobbyCode = "";
    
    MainWindowViewModel _mainWindowViewModel;
    
    public JoinLobbyViewModel(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;
    }
    
    [RelayCommand]
    public void Join()
    {
        Console.WriteLine($"Joining lobby {LobbyCode}");
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