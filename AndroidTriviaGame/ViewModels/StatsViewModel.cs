using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AndroidTriviaGame.ViewModels;

public record PlayerStat(int Rank, string Name, int Score);

public partial class StatsViewModel : ObservableObject
{
    private readonly MainWindowViewModel _mainWindowViewModel;

    [ObservableProperty] 
    private List<PlayerStat> _statsList;

    public StatsViewModel(MainWindowViewModel mainWindowViewModel, Dictionary<string, int> playerScores)
    {
        _mainWindowViewModel = mainWindowViewModel;
        
        StatsList = playerScores
            .OrderByDescending(x => x.Value)
            .Select((kvp, index) => new PlayerStat(index + 1, kvp.Key, kvp.Value))
            .ToList();
    }

    [RelayCommand]
    private void ReturnToMenu()
    {
        _mainWindowViewModel.CurrentPage = new MenuViewModel(_mainWindowViewModel);
    }
}