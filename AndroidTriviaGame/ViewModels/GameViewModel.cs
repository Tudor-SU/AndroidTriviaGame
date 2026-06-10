using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AndroidTriviaGame.ViewModels;

public partial class GameViewModel : ObservableObject
{
    private readonly MainWindowViewModel _mainWindowViewModel;

    // --- UI State Properties ---
    [ObservableProperty] private bool _isGameOver = false;
    [ObservableProperty] private string _questionProgress = "0/0";
    
    // --- 4 Hardcoded Player Slots ---
    [ObservableProperty] private string _player1Text = "";
    [ObservableProperty] private string _player2Text = "";
    [ObservableProperty] private string _player3Text = "";
    [ObservableProperty] private string _player4Text = "";
    
    // --- Current Question Properties ---
    [ObservableProperty] private string _currentQuestionText = "Waiting for players...";
    [ObservableProperty] private List<string> _currentAnswers = [];
    [ObservableProperty] private int _selectedAnswerIndex = -1; 
    [ObservableProperty] private bool _hasSubmittedAnswer = false;

    public GameViewModel(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;
    }

    // 🎯 Called when the server sends a new question
    // public void OnGameStateUpdateReceived(GameStateUpdate incomingState)
    // {
    //     if (IsGameOver) return; 
    //
    //     CurrentQuestionText = incomingState.CurrentQuestion.QuestionText;
    //     CurrentAnswers = incomingState.CurrentQuestion.Answers;
    //     QuestionProgress = $"{incomingState.CurrentQuestionIndex + 1}/{incomingState.TotalQuestions}";
    //     
    //     SelectedAnswerIndex = -1; 
    //     HasSubmittedAnswer = false; 
    //
    //     UpdateScores(incomingState.PlayerScores);
    // }

    // // 🎯 Called when the server says the match is finished
    // public void OnGameOverReceived(GameOverUpdate finalState)
    // {
    //     IsGameOver = true;
    //     UpdateScores(finalState.FinalScores);
    // }
    //
    // // Maps the dictionary safely to exactly 4 slots. Missing players become empty strings (0 pixels wide).
    // private void UpdateScores(Dictionary<string, int> scoresToApply)
    // {
    //     var playerList = scoresToApply.OrderBy(x => x.Key).ToList();
    //
    //     Player1Text = playerList.Count > 0 ? $"{playerList[0].Key}: {playerList[0].Value}" : "";
    //     Player2Text = playerList.Count > 1 ? $"{playerList[1].Key}: {playerList[1].Value}" : "";
    //     Player3Text = playerList.Count > 2 ? $"{playerList[2].Key}: {playerList[2].Value}" : "";
    //     Player4Text = playerList.Count > 3 ? $"{playerList[3].Key}: {playerList[3].Value}" : "";
    // }

    [RelayCommand]
    private void SubmitAnswer()
    {
        if (SelectedAnswerIndex == -1 || HasSubmittedAnswer) return;

        HasSubmittedAnswer = true; 
        
        Console.WriteLine($"Answer: {SelectedAnswerIndex}");
        
        NetworkStream? stream = _mainWindowViewModel.GameClient.GetNetworkStream();
        if (stream != null)
        {
            NetworkingAPI.SendPacket(
                stream, PacketType.SubmitAnswerUpdate,
                new AnswerUpdate(
                    _mainWindowViewModel.GameClient.Username,
                    SelectedAnswerIndex
                )
            );
        }
    }

    [RelayCommand]
    private void Leave()
    {
        _mainWindowViewModel.GameClient.Log("Leaving Game");
        
        NetworkStream? stream = _mainWindowViewModel.GameClient.GetNetworkStream();
        if (stream != null)
        {
            NetworkingAPI.SendPacket(stream, PacketType.LeaveLobbyUpdate, _mainWindowViewModel.GameClient.Username);
        }

        _mainWindowViewModel.CurrentPage = new MenuViewModel(_mainWindowViewModel);
    }
}