using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AndroidTriviaGame.ViewModels;

public partial class StatsViewModel : ObservableObject
{
    private readonly MainWindowViewModel _mainWindowViewModel;

    // The list of strings you want to display
    [ObservableProperty] 
    private List<string> _statsList ;

    public StatsViewModel(MainWindowViewModel mainWindowViewModel, Dictionary<string, int> playerScores)
    {
        _mainWindowViewModel = mainWindowViewModel;
        _statsList = playerScores.OrderByDescending(x => x.Value)
            .Select(x => $"{x.Key}: {x.Value}")  
            .ToList();
        
    }

    [RelayCommand]
    private void ReturnToMenu()
    {
        _mainWindowViewModel.CurrentPage = new MenuViewModel(_mainWindowViewModel);
    }
}