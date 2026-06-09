using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using AndroidTriviaGame.ViewModels;
using AndroidTriviaGame.Views;
using Avalonia.Controls;
using Avalonia.Styling;

namespace AndroidTriviaGame;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        AppSettings? savedSettings = AppSettings.Load();

        if (savedSettings != null)
        {
            Resources["MenuFontSize"] = savedSettings.MenuFontSize;
            Resources["QuestionFontSize"] = savedSettings.QuestionFontSize;
            Resources["AnswerFontSize"] = savedSettings.AnswerFontSize;
        
            RequestedThemeVariant = savedSettings.IsDarkMode ? ThemeVariant.Dark : ThemeVariant.Light;
        }
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            
            GameClient gameClient = new GameClient(
                "100.101.207.46", 5000,
                Platform.Windows    
            );
            gameClient.ConnectToServer();
            
            if (savedSettings is null)
            {
                gameClient.Log("No previous settings were found");
            }
            
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(gameClient)
            };
        }
        else if (ApplicationLifetime is IActivityApplicationLifetime singleViewFactoryApplicationLifetime)
        {
            GameClient gameClient = new GameClient(
                "100.101.207.46", 5000,
                Platform.Android
            );
            gameClient.ConnectToServer();
            
            if (savedSettings is null)
            {
                gameClient.Log("No previous settings were found");
            }
            
            singleViewFactoryApplicationLifetime.MainViewFactory = () =>
            {
                return new MainView
                {
                    DataContext = new MainWindowViewModel(gameClient)
                };


                // var mainWindowInstance = new MainWindow 
                // { 
                //     DataContext = new MainWindowViewModel(gameClient) 
                // };
                //
                // // Return the core UI elements sitting inside the window frame
                // return (Control)mainWindowInstance.Content!;
            };
            // singleViewFactoryApplicationLifetime.MainViewFactory =
            //     () => new MainView { DataContext = new MainViewModel(gameClient) };
        }
        // else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        // {
        //     singleViewPlatform.MainView = new MainView
        //     {
        //         DataContext = new MainViewModel()
        //     };
        // }

        base.OnFrameworkInitializationCompleted();
    }
}