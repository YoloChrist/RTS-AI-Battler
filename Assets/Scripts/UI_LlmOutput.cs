using UnityEngine;
using TMPro;

public class UI_LlmOutput : MonoBehaviour
{
    private TMP_Text llmOutputText;
    private LLMApiClient llmApiClient;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        llmOutputText = GetComponent<TMP_Text>();
        llmApiClient = LLMApiClient.Instance;
        if (llmApiClient != null)
        {
            llmApiClient.OnResponse.AddListener(HandleUpdateText);
        }
        else
        {
            Debug.LogError("[UI_LlmOutput] LLMApiClient instance not found.");
        }
    }

    private void OnDestroy()
    {
        llmApiClient.OnResponse.RemoveListener(HandleUpdateText);
    }

    private void HandleUpdateText(string response)
    {
        Debug.LogWarning("[UI_LlmOutput] Received LLM response.");
        llmOutputText.text = response;
    }
}
