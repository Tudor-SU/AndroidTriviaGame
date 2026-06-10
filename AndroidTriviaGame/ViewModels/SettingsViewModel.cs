using System;

using Avalonia;

using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AndroidTriviaGame.ViewModels;

public partial class SettingsViewModel : ObservableObject 
{
    [ObservableProperty]
    private double _menuFontSize;

    [ObservableProperty]
    private double _questionFontSize;

    [ObservableProperty]
    private double _answerFontSize;

    [ObservableProperty] 
    private bool _unsavedChanges; 
    
    private readonly MainWindowViewModel _mainWindowViewModel;
    private AppSettings? _appSettingsCopy;

    public SettingsViewModel(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;

        if (Application.Current is null) return;
        {
            if (Application.Current.Resources.TryGetValue("MenuFontSize", out var menuSize))
                MenuFontSize = Convert.ToDouble(menuSize);
        
            if (Application.Current.Resources.TryGetValue("QuestionFontSize", out var questionSize))
                QuestionFontSize = Convert.ToDouble(questionSize);
        
            if (Application.Current.Resources.TryGetValue("AnswerFontSize", out var answerSize))
                AnswerFontSize = Convert.ToDouble(answerSize);
            
            _appSettingsCopy = new AppSettings(
                Application.Current.ActualThemeVariant == ThemeVariant.Dark,
                MenuFontSize, QuestionFontSize, AnswerFontSize
            );
        }
        UnsavedChanges = false;
    }
    
    partial void OnMenuFontSizeChanged(double value)
    {
        if (Application.Current != null)
        {
            Application.Current.Resources["MenuFontSize"] = value;
            UnsavedChanges =  true;
        }

    }

    partial void OnQuestionFontSizeChanged(double value)
    {
        if (Application.Current != null)
        {
            Application.Current.Resources["QuestionFontSize"] = value;
            UnsavedChanges =  true;
        }

    }

    partial void OnAnswerFontSizeChanged(double value)
    {
        if (Application.Current != null)
        {
            Application.Current.Resources["AnswerFontSize"] = value;
            UnsavedChanges =  true;
        }

    }
    
    [RelayCommand]
    private void ToggleTheme()
    {
        if (Application.Current != null)
        {
            Application.Current.RequestedThemeVariant = 
                Application.Current.ActualThemeVariant == ThemeVariant.Light 
                    ? ThemeVariant.Dark 
                    : ThemeVariant.Light;
            UnsavedChanges =  true;
        }
    }
    
    [RelayCommand]
    private void Back()
    {
        if (Application.Current != null && _appSettingsCopy != null && UnsavedChanges)
        {
            Application.Current.Resources["MenuFontSize"] = _appSettingsCopy.MenuFontSize;
            Application.Current.Resources["QuestionFontSize"] = _appSettingsCopy.QuestionFontSize;
            Application.Current.Resources["AnswerFontSize"] = _appSettingsCopy.AnswerFontSize;

            bool isDarkMode = Application.Current.ActualThemeVariant == ThemeVariant.Dark;
            if (_appSettingsCopy.IsDarkMode != isDarkMode)
            {
                ToggleTheme();
            }
        }
        
        _mainWindowViewModel.CurrentPage = new MenuViewModel(_mainWindowViewModel);
    }

    [RelayCommand]
    private void Save()
    {
        if (Application.Current is null || _appSettingsCopy is null) return;
        
        UnsavedChanges = false;
        _appSettingsCopy.AnswerFontSize = AnswerFontSize;
        _appSettingsCopy.MenuFontSize = MenuFontSize;
        _appSettingsCopy.QuestionFontSize = QuestionFontSize;
        _appSettingsCopy.IsDarkMode = Application.Current.ActualThemeVariant == ThemeVariant.Dark;
        
        bool status = AppSettings.Save(_appSettingsCopy);
        
        _mainWindowViewModel.GameClient.Log(
            status ? $"Settings Saved at {AppSettings.GetFilePath()}" : 
                "Settings Not Saved"
        );
    }
}