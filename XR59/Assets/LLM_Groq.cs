using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class LLM_Groq : MonoBehaviour
{
    [SerializeField] private string apiKey;
    private const string apiURI = "https://api.groq.com/openai/v1/chat/completions";

    private enum LLMModel
    {
        deepseek_r1_distill_llama_70b,
        llama_3_8b_8192,
        llama3_70b_8192,
        llama_3X1_8b_instant,
        llama3X3_70b_versatile,
        mixtral_8x7b_32768,
        gemma2_9b_it
    }

    [SerializeField] private LLMModel selectedModel;
    private string selectedLLMString;

    [SerializeField] private bool shortResponse;
    [SerializeField] private string whoAmI = "nobody";
    [SerializeField] private string context;
    [SerializeField] private bool closedContext;

    private List<Message> messageHistory;
    private bool isProcessing = false;

    [Header("LLM Response Event")]
    public UnityEvent<string> OnLLMResponse;

    void Start()
    {
        selectedLLMString = selectedModel.ToString().Replace('_', '-').Replace('X', '.');
        Debug.Log("🧠 Selected LLM: " + selectedLLMString);

        // Setup role and context
        whoAmI = "a cashier at a supermarket";
        context = "You are a friendly cashier at a supermarket. Your job is to assist customers with their purchases, provide information about products, and handle transactions. You can have brief small talks. If you're asked something a cashier wouldn't know, gently change the subject.";

        string prompt = $"You are {whoAmI}";
        if (shortResponse) prompt += "\nAnswer all questions concise and brief! Maximum 280 characters.";
        prompt += $"\nAnswer all questions using the following context:\n===\n{context}\nToday is {DateTime.Now.ToShortDateString()}\n===";
        if (closedContext) prompt += "\nIf the answer can't be found in the context then respond with: \"I don't know!\"";

        messageHistory = new List<Message>();
        AppendConversation(prompt, "system");

        // Listen to messages
        ListenerTest listener = FindAnyObjectByType<ListenerTest>();
        if (listener != null)
        {
            listener.OnMessageReceived.AddListener(HandleIncomingMessage);
        }
    }

    private void HandleIncomingMessage(string message)
    {
        if (!isProcessing)
        {
            StartCoroutine(TalkToLLM(message));
        }
        else
        {
            Debug.Log("⏳ Still processing a previous request.");
        }
    }

    private void AppendConversation(string content, string role)
    {
        messageHistory.Add(new Message { role = role, content = content });
    }

    private IEnumerator TalkToLLM(string message)
    {
        isProcessing = true;
        AppendConversation(message, "user");

        RequestBody requestBody = new RequestBody
        {
            messages = messageHistory.ToArray(),
            model = selectedLLMString
        };

        string jsonRequest = JsonUtility.ToJson(requestBody);

        UnityWebRequest request = new UnityWebRequest(apiURI, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonRequest);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            GroqCloudResponse response = JsonUtility.FromJson<GroqCloudResponse>(request.downloadHandler.text);
            string rawReply = response.choices[0].message.content;
            string reply = StripThinkingBlock(rawReply);
            Debug.Log("💬 LLM Response: " + reply);

            OnLLMResponse?.Invoke(reply);
        }
        else
        {
            Debug.LogError("❌ LLM API Request failed: " + request.error);
        }

        isProcessing = false;
    }

    private string StripThinkingBlock(string message)
    {
        int thinkStart = message.IndexOf("<think>", StringComparison.OrdinalIgnoreCase);
        int thinkEnd = message.IndexOf("</think>", StringComparison.OrdinalIgnoreCase);

        if (thinkStart >= 0 && thinkEnd > thinkStart)
        {
            // Remove the thinking block
            string stripped = message.Substring(thinkEnd + "</think>".Length);
            return stripped.TrimStart();
        }

        // No thinking block detected
        return message.Trim();
    }

    [Serializable]
    public class RequestBody
    {
        public Message[] messages;
        public string model;
    }

    [Serializable]
    public class Message
    {
        public string role;
        public string content;
    }

    [Serializable]
    public class GroqCloudResponse
    {
        public string id;
        public string @object;
        public int created;
        public string model;
        public Choice[] choices;
        public Usage usage;
        public string system_fingerprint;
        public XGroq x_groq;
    }

    [Serializable]
    public class Choice
    {
        public int index;
        public ChoiceMessage message;
        public object logprobs;
        public string finish_reason;
    }

    [Serializable]
    public class ChoiceMessage
    {
        public string role;
        public string content;
    }

    [Serializable]
    public class Usage
    {
        public int prompt_tokens;
        public float prompt_time;
        public int completion_tokens;
        public float completion_time;
        public int total_tokens;
        public float total_time;
    }

    [Serializable]
    public class XGroq
    {
        public string id;
    }
}
