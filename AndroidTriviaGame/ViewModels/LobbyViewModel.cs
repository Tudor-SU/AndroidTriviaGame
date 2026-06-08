using CommunityToolkit.Mvvm.ComponentModel;

namespace AndroidTriviaGame.ViewModels;

public partial class LobbyViewModel:ObservableObject
{
    MainWindowViewModel _mainWindowViewModel;
    public LobbyViewModel(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;
    }
}