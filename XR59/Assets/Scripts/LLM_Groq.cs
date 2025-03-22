using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LLM_Groq : MonoBehaviour
{
    [SerializeField]
    private string apiKey;
    const string apiURI = "https://api.groq.com/openai/v1/chat/completions";

    private enum LLMModel { deepseek_r1_distill_llama_70b, llama_3_8b_8192, llama3_70b_8192, llama_3X1_8b_instant, llama3X3_70b_versatile, mixtral_8x7b_32768, gemma2_9b_it }

    [SerializeField]
    private LLMModel selectedModel;
    string selectedLLMString;

    private string LLMresult = "Waiting";

    [SerializeField]
    private bool shortResponse;

    //NEW!
    [SerializeField]
    private string whoAmI = "nobody";

    [SerializeField]
    private string context;

    [SerializeField]
    private bool closedContext;

    List<Message> messageHistory;

    //AI_Orchestrator aiO;


    // Start is called before the first frame update
    void Start()
    {
        string prompt;
        DateTime currentDate = DateTime.Now;

        //aiO = GetComponent<AI_Orchestrator>();
        //if (!aiO)
        //{
        //   Debug.LogError("AI Orchestrator component not found!");
        //    return;
       // }

        selectedLLMString = selectedModel.ToString().Replace('_', '-').Replace('X', '.');
        Debug.Log("You have selected LLM: " + selectedLLMString);

        // Set the AI's role as a cashier
        whoAmI = "a cashier at a supermarket";
        context = "You are a friendly cashier at a supermarket. Your job is to assist customers with their purchases, provide information about products, and handle transactions. you can have brief small talks, if you see that you're getting asked questions that a normal cashier person wouldn't know then gently change the subject.";

        // Generate the prompt
        prompt = "You are " + whoAmI;
        if (shortResponse) prompt += "\nAnswer all questions concise and brief!";
        prompt += "\nAnswer all questions using the following context:\n===\n";
        prompt += context;
        prompt += "\nToday is " + currentDate.ToShortDateString();
        prompt += "\n===";
        if (closedContext) prompt += "\nIf the answer can't be found in the context then respond with: \"I don't know! \"";

        Debug.Log(prompt);

        // Initialize the conversation history
        messageHistory = new List<Message>();
        AppendConversation(prompt, "system");
    }


    private void AppendConversation(string mesg, string myRole)
    {
        Message newMesg = new Message
        {
            role = myRole,
            content = mesg       //UPDATED!
        };
        messageHistory.Add(newMesg);
    }


    public void TextToLLM(string mesg)       //UPDATED!
    {
        StartCoroutine(TalkToLLM(mesg));          //NEW!
    }


    private IEnumerator TalkToLLM(string mesg)
    {
        RequestBody requestBody = new RequestBody();

        AppendConversation(mesg, "user");
        requestBody.messages = messageHistory.ToArray();        //Add the complete conversation history

        foreach (var x in requestBody.messages) Debug.Log(x.content + " " + x.role);

        requestBody.model = selectedLLMString;
        string jsonRequestBody = JsonUtility.ToJson(requestBody);
        LLMresult = "Waiting";

        UnityWebRequest request = new UnityWebRequest(apiURI, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonRequestBody);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

        //headers
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            GroqCloudResponse groqCS = JsonUtility.FromJson<GroqCloudResponse>(responseText);
            LLMresult = groqCS.choices[0].message.content;  //here is the field where the actual response is!
            Debug.Log(LLMresult);

            //now lets call TTS via a single call to the central AI Orchestrator!
            //aiO.Say(LLMresult);
        }
        else Debug.Log("LLM API Request failed: " + request.error);

    }


    //=============================
    //Write JSON to LLM classes - generated with LLama!
    //=============================
    [System.Serializable]
    public class RequestBody
    {
        public Message[] messages;
        public string model;
    }


    [System.Serializable]
    public class Message
    {
        public string role;
        public string content;
    }


    //Read JSON response from LLM classes
    [System.Serializable]
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

    [System.Serializable]
    public class Choice
    {
        public int index;
        public ChoiceMessage message;
        public object logprobs;
        public string finish_reason;
    }

    [System.Serializable]
    public class ChoiceMessage
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    public class Usage
    {
        public int prompt_tokens;
        public float prompt_time;
        public int completion_tokens;
        public float completion_time;
        public int total_tokens;
        public float total_time;
    }

    [System.Serializable]
    public class XGroq
    {
        public string id;
    }
}
