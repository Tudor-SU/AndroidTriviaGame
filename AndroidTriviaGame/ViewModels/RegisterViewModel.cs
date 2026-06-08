using System;
using System.Net.Sockets;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;


namespace AndroidTriviaGame.ViewModels;

public partial class RegisterViewModel : ObservableObject
{
    [ObservableProperty]
    private string _username = "john";
    
    [ObservableProperty]
    private string _password = "123";
    
    [ObservableProperty] 
    private string _errMsg = "";

    private readonly MainWindowViewModel _mainWindowViewModel;

    public RegisterViewModel(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;
        if (!_mainWindowViewModel.GameClient.IsConnected())
        {
            _errMsg = "Server is unreachable!";
        }
    }

    [RelayCommand]
    private void BtnRegisterAction()
    {
        try
        {
            NetworkStream? stream = _mainWindowViewModel.GameClient.GetNetworkStream();
            if (stream == null) return;
        
            NetworkingAPI.SendPacket(
                stream,
                PacketType.RegisterRequest,
                new Credentials(Username, Password)
            );
            _mainWindowViewModel.GameClient.Log($"Register {Username} {Password}");
        }
        catch (Exception e)
        {
            _mainWindowViewModel.GameClient.Log("Register failed: " + e.Message);
        }
    }

    [RelayCommand]
    private void BtnLoginAction()
    {
        _mainWindowViewModel.CurrentPage = new LoginViewModel(_mainWindowViewModel);
    }

}