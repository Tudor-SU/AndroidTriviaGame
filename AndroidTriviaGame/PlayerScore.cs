using CommunityToolkit.Mvvm.ComponentModel;

namespace AndroidTriviaGame;

public partial class PlayerScore : ObservableObject
{
    [ObservableProperty] 
    private string _username;
    
    [ObservableProperty] 
    private int _score;

    public PlayerScore(string username, int score = 0)
    {
        Username = username;
        Score = score;
    }
}