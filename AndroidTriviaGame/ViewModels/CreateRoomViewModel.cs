using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AndroidTriviaGame.ViewModels;

public partial class CreateRoomViewModel:ObservableObject
{
    [ObservableProperty]
    private string _roomCode = "";
    
    MainWindowViewModel _mainWindowViewModel;
    
    public CreateRoomViewModel(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;
    }
    
    [RelayCommand]
    public void Create()
    {
        Console.WriteLine($"Creating room");
    }

    [RelayCommand]
    public void JoinRoom()
    {
        _mainWindowViewModel.CurrentPage = new JoinRoomViewModel(_mainWindowViewModel);
    }

    [RelayCommand]
    public void Back()
    {
        _mainWindowViewModel.CurrentPage = new MenuViewModel(_mainWindowViewModel);
    }
}