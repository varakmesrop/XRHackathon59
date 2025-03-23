using Meta.WitAi.Dictation.Events;
using Meta.WitAi.Json;
using Oculus.Voice.Dictation;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ListenerTest : MonoBehaviour
{
    [Header("Wit Dictation Reference")]
    public AppDictationExperience dictationExperience;

    [System.Serializable]
    public class TranscriptionResult
    {
        public string text;
    }

    [Header("Events")]
    public UnityEvent<string> OnMessageReceived;

    private bool dicStarted = false;
    private string lastTranscript = "";

    void Start()
    {
        if (dictationExperience == null)
        {
            Debug.LogError("🎤 DictationExperience is not assigned!");
            return;
        }

        dictationExperience.DictationEvents.OnStartListening.AddListener(OnStartDictation);
        dictationExperience.DictationEvents.OnStoppedListening.AddListener(OnStopDictation);
        dictationExperience.DictationEvents.OnRawResponse.AddListener(OnFullTranscription);
    }

    private void Update()
    {
        if (!dicStarted)
        {
            StartDictation();
        }
    }

    private void OnStartDictation()
    {
        dicStarted = true;
        lastTranscript = "";
        Debug.Log("🎙️ Dictation started...");
    }

    private void OnFullTranscription(string json)
    {
        TranscriptionResult result = JsonUtility.FromJson<TranscriptionResult>(json);
        lastTranscript = result.text;
        Debug.Log("📝 Extracted text: " + lastTranscript);
    }

    private void OnStopDictation()
    {
        dicStarted = false;
        Debug.Log("🛑 Dictation stopped.");

        if (!string.IsNullOrEmpty(lastTranscript) && lastTranscript.Length > 1)
        {
            Debug.Log("📣 Invoking OnMessageReceived with: " + lastTranscript);
            OnMessageReceived?.Invoke(lastTranscript);
        }
    }

    public void StartDictation() => dictationExperience.Activate();
    public void StopDictation() => dictationExperience.Deactivate();
}
