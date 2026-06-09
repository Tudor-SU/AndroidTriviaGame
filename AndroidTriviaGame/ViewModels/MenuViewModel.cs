using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AndroidTriviaGame.ViewModels;

public partial class MenuViewModel:ObservableObject
{
    private readonly MainWindowViewModel _mainWindowViewModel;
    [ObservableProperty]
    string _welcomeMessage;
    
    public MenuViewModel(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;
        _welcomeMessage = $"Welcome {_mainWindowViewModel.GameClient.Username}!";
    }

    [RelayCommand]
    public void Play()
    {
        _mainWindowViewModel.CurrentPage = new JoinLobbyViewModel(_mainWindowViewModel);
    }
}