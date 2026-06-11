using System;
using System.Collections.Generic;
using System.Net.Sockets;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AndroidTriviaGame.ViewModels;

public partial class GameViewModel : ObservableObject
{
    private readonly MainWindowViewModel _mainWindowViewModel;

    [ObservableProperty] private bool _isGameOver = false;
    [ObservableProperty] private string _questionProgress = "0/0";
    
    [ObservableProperty] private string _player1Text = "";
    [ObservableProperty] private string _player2Text = "";
    [ObservableProperty] private string _player3Text = "";
    [ObservableProperty] private string _player4Text = "";
    
    [ObservableProperty] private string _currentQuestionText = "";
    [ObservableProperty] private List<string> _currentAnswers = [];
    [ObservableProperty] private int _selectedAnswerIndex = -1; 
    [ObservableProperty] private bool _hasSubmittedAnswer = false;

    public GameViewModel(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;
    }
    
    
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