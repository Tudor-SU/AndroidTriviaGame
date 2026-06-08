using System;
using System.Net.Sockets;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AndroidTriviaGame.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    [ObservableProperty]
    private string _username = "john";
    
    [ObservableProperty]
    private string _password = "123";
    
    [ObservableProperty] 
    private string _errMsg = "";

    private readonly MainWindowViewModel _mainWindowViewModel;

    public LoginViewModel(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;
        if (!_mainWindowViewModel.GameClient.IsConnected())
        {
            _errMsg = "Server is unreachable!";
        }
    }

    [RelayCommand]
    private void BtnLoginAction()
    {
        try
        {
            NetworkStream? stream = _mainWindowViewModel.GameClient.GetNetworkStream();
            if (stream == null) return;

            NetworkingAPI.SendPacket(
                stream,
                PacketType.LoginCredentials,
                new LoginCredentials(Username, Password)
            );
            Console.WriteLine($"Login {Username} {Password}");
        }
        catch (Exception e)
        {
            Console.WriteLine("Login failed: " + e.Message);
        }

    }

}