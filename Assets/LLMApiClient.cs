using System;
using System.Collections;
using System.Text;
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

// ===== MAIN API CLIENT =====
public class LLMApiClient : MonoBehaviour
{
    [Header("LLM Configuration")]
    [SerializeField] private LLMConfig config = new LLMConfig();

    private static LLMApiClient instance;

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
        TestConnection(success =>
        {
            if (success)
                Debug.Log("[LLM] Connection test successful.");
            else
                Debug.LogError("[LLM] Connection test failed.");
        });
    }

    // Main method for game state analysis
    public void AnalyzeGameState(string gameStateJSON, string customPrompt, Action<string> onSuccess, Action<string> onError)
    {
        string fullPrompt = $"{customPrompt}\n\n## Game State Data:\n```json\n{gameStateJSON}\n```";
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
}
