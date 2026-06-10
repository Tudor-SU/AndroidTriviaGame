using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace AndroidTriviaGame;

public static class QuizManager
{
    private static List<QuizQuestion> _globalQuestionPool = new();
    
    public static int LoadQuestions(string filePath)
    {
       
        if (!File.Exists(filePath))
        {
            throw  new FileNotFoundException($"File {filePath} does not exist");
        }

        string jsonText = File.ReadAllText(filePath);
        _globalQuestionPool = JsonSerializer.Deserialize<List<QuizQuestion>>(jsonText) ?? new();
        
        return _globalQuestionPool.Count;
    }
    
    public static List<QuizQuestion> ExtractQuestions(int count = 10)
    {
        if (_globalQuestionPool.Count <= count)
        {
            return _globalQuestionPool.ToList(); 
        }

        return _globalQuestionPool
            .OrderBy(_ => Guid.NewGuid()) 
            .Take(count)
            .ToList();
    }
}