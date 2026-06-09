using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization; // 🎯 Add this using statement!

namespace AndroidTriviaGame;

public class AppSettings
{
    public bool IsDarkMode { get; set; } 
    public double MenuFontSize { get; set;} 
    public double QuestionFontSize { get;set;}
    public double AnswerFontSize { get; set;}

    private const string FileName = "settings.json";
    
    [JsonConstructor] 
    public AppSettings(bool isDarkMode, double menuFontSize, double questionFontSize, double answerFontSize)
    {
        IsDarkMode = isDarkMode;
        MenuFontSize = menuFontSize;
        QuestionFontSize = questionFontSize;
        AnswerFontSize = answerFontSize;
    }
    
    public static string GetFilePath()
    {
        string basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        
        string appFolder = Path.Combine(basePath, "AndroidTriviaGame");
        
        if (!Directory.Exists(appFolder))
        {
            Directory.CreateDirectory(appFolder);
        }
        
        return Path.Combine(appFolder, "settings.json");
    }
    
    public static AppSettings? Load()
    {
        string path = GetFilePath();

        if (!File.Exists(path))
            return null;

        try
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<AppSettings>(json);
        }
        catch 
        {
            return null; 
        }
    }

    public static bool Save(AppSettings settings)
    {
        string path = GetFilePath();
        
        try
        {
            string json = JsonSerializer.Serialize(settings);
            File.WriteAllText(path, json);
            return true;
        }
        catch 
        { 
            return false;
        }
    }
}