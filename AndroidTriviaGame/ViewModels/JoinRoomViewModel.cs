using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AndroidTriviaGame.ViewModels;

public partial class JoinRoomViewModel:ObservableObject
{
    [ObservableProperty]
    private string _roomCode = "";
    
    MainWindowViewModel _mainWindowViewModel;
    
    public JoinRoomViewModel(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;
    }
    
    [RelayCommand]
    public void Join()
    {
        Console.WriteLine($"Joining room {RoomCode}");
    }

    [RelayCommand]
    public void CreateRoom()
    {
     _mainWindowViewModel.CurrentPage = new CreateRoomViewModel(_mainWindowViewModel);   
    }

    [RelayCommand]
    public void Back()
    {
        _mainWindowViewModel.CurrentPage = new MenuViewModel(_mainWindowViewModel);
    }
}