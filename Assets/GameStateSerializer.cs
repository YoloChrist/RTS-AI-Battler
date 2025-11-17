using System.IO;
using UnityEngine;

public class GameStateSerializer
{
    private readonly string _filePath;
    private static GameStateSerializer instance;
    public static GameStateSerializer Instance => instance;

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
        return Load<GameStateData>();
    }

    public T Load<T>() where T : new()
    {
        if (File.Exists(_filePath))
        {
            string json = File.ReadAllText(_filePath);
            Debug.Log($"Game state loaded from {_filePath}");
            return JsonUtility.FromJson<T>(json);
        }
        
        Debug.Log("No save file found. Starting new game state.");
        return new T();
    }

    public string LoadAsString()
    {
        if (File.Exists(_filePath))
        {
            string json = File.ReadAllText(_filePath);
            Debug.Log($"Game state loaded as string from {_filePath}");
            return json;
        }
        
        Debug.Log("No save file found.");
        return string.Empty;
    }
}