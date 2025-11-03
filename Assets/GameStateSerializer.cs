using System.IO;
using UnityEngine;

public class GameStateSerializer
{
    private readonly string _filePath;

    public GameStateSerializer(string filePath)
    {
        _filePath = filePath;
    }

    public void Save(GameStateData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(_filePath, json);
        Debug.Log($"Game state saved to {_filePath}");
    }

    public GameStateData Load()
    {
        if (File.Exists(_filePath))
        {
            string json = File.ReadAllText(_filePath);
            Debug.Log($"Game state loaded from {_filePath}");
            return JsonUtility.FromJson<GameStateData>(json);
        }
        
        Debug.Log("No save file found. Starting new game state.");
        return new GameStateData();
    }
}