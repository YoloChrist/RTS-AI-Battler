using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Networking;

// config
[Serializable]
public class LLMConfig
{
    public string serverUrl = "http://localhost:8080";
    public string endpoint = "/v1/chat/completions";
    public string modelName = "llama-3";
    public int maxTokens = 1024;
    public float temperature = 0.7f;
    public string systemPrompt = "You are a tactical AI assistant for an RTS game. Analyze the game state and provide strategic recommendations.";

    public string GetFullUrl() => serverUrl + endpoint;
}

// request & response classes
[Serializable]
public class LLMRequest
{
    public string model;
    public LLMMessage[] messages;
    public int max_tokens;
    public float temperature;
    public bool stream = false;
}

[Serializable]
public class LLMMessage
{
    public string role; // "system", "user", "assistant"
    public string content;
}

[Serializable]
public class LLMResponse
{
    public string id;
    public string obj;
    public long created;
    public string model;
    public LLMChoice[] choices;
    public LLMUsage usage;
}

[Serializable]
public class LLMChoice
{
    public int index;
    public LLMMessage message;
    public string finish_reason;
}

[Serializable]
public class LLMUsage
{
    public int prompt_tokens;
    public int completion_tokens;
    public int total_tokens;
}

// Events
[Serializable]
public class LLMResponseEvent : UnityEngine.Events.UnityEvent<string> { }

// ===== MAIN API CLIENT =====
public class LLMApiClient : MonoBehaviour
{
    [Header("LLM Configuration")]
    [SerializeField] private LLMConfig config = new LLMConfig();

    [Header("Events")]
    public LLMResponseEvent OnResponse = new LLMResponseEvent();

    private static LLMApiClient instance;
    public static LLMApiClient Instance => instance;

    private string gameStateJSON;



    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Invoke(nameof(ManualAnalyze), 5f);
    }

    // Main method for game state analysis
    private void AnalyzeCurrentGameState(string customPrompt, Action<string> onSuccess, Action<string> onError)
    {
        if (GameStateManager.Instance == null)
        {
            onError?.Invoke("GameStateManager instance not found.");
            return;
        }

        string gameStateData = GameStateManager.Instance.SaveGameState();
        AnalyzeGameState(gameStateData, customPrompt, onSuccess, onError);
    }

    private void AnalyzeGameState(string gameStateJSON, string customPrompt, Action<string> onSuccess, Action<string> onError)
    {
        string fullPrompt = $"{customPrompt}\n\n## Game State Data:\n```json\n{gameStateJSON}\n```";
        Debug.Log(fullPrompt);
        StartCoroutine(SendLLMRequest(fullPrompt, onSuccess, onError));
    }

    // Simple prompt without game state
    public void SendPrompt(string prompt, Action<string> onSuccess, Action<string> onError)
    {
        StartCoroutine(SendLLMRequest(prompt, onSuccess, onError));
    }

    private IEnumerator SendLLMRequest(string userPrompt, Action<string> onSuccess, Action<string> onError)
    {
        // Build request
        LLMRequest requestData = new LLMRequest
        {
            model = config.modelName,
            max_tokens = config.maxTokens,
            temperature = config.temperature,
            stream = false,
            messages = new LLMMessage[]
            {
                new LLMMessage { role = "system", content = config.systemPrompt },
                new LLMMessage { role = "user", content = userPrompt }
            }
        };

        string jsonBody = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(config.GetFullUrl(), "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 30;

            Debug.Log($"[LLM] Sending request to {config.GetFullUrl()}...");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log($"[LLM] Response received: {responseText}");

                // Parse response
                try
                {
                    LLMResponse response = JsonUtility.FromJson<LLMResponse>(responseText);

                    if (response?.choices != null && response.choices.Length > 0)
                    {
                        string content = response.choices[0].message.content;
                        onSuccess?.Invoke(content);
                        OnResponse?.Invoke(content);
                    }
                    else
                        onError?.Invoke("No valid response content");
                }
                catch (Exception e)
                {
                    onError?.Invoke($"Parse error: {e.Message}");
                }
            }
            else
            {
                string errorMsg = $"Request failed: {request.error}\n{request.downloadHandler?.text}";
                Debug.LogError($"[LLM] {errorMsg}");
                onError?.Invoke(errorMsg);
            }
        }
    }

    // Update config at runtime
    public void UpdateConfig(Action<LLMConfig> configAction)
    {
        configAction?.Invoke(config);
    }

    // Test connection
    public void TestConnection(Action<bool> callback)
    {
        StartCoroutine(TestConnectionCoroutine(callback));
    }

    private IEnumerator TestConnectionCoroutine(Action<bool> callback)
    {
        bool success = false;
        bool completed = false;

        SendPrompt("Respond with 'OK'", response =>
        {
            Debug.Log($"[LLM] Test connection successful: {response}");
            success = true;
            completed = true;
        },
        error =>
        {
            Debug.LogError($"[LLM] Test connection failed: {error}");
            success = false;
            completed = true;
        });

        // Wait for completion
        float timeout = 30f;
        float elapsed = 0f;
        while (!completed && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!completed)
        {
            Debug.LogError("[LLM] Test connection timed out after 30 seconds");
        }

        callback?.Invoke(success && completed);
    }

    private void ManualAnalyze()
    {
        Debug.Log("[LLM] Analyzing current game state...");
        AnalyzeCurrentGameState("Provide strategic recommendations based on the current game state.",
            onSuccess: response => Debug.Log($"[LLM] Strategic recommendations: {response}"),
            onError: error => Debug.LogError($"[LLM] Analysis error: {error}")
            );
    }
}
