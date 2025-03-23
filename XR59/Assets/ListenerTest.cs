using Meta.WitAi.Dictation.Events;
using Oculus.Voice.Dictation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ListenerTest : MonoBehaviour
{
    [Header("Wit Dictation Reference")]
    public AppDictationExperience dictationExperience;

    void Start()
    {
        if (dictationExperience == null)
        {
            Debug.LogError("🎤 DictationExperience is not assigned!");
            return;
        }

        dictationExperience.DictationEvents.OnStartListening.AddListener(OnStartDictation);


    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("q");
            dictationExperience.Activate();
        }
    }

    private void OnStartDictation()
    {
        Debug.Log("🎙️ Dictation started...");
    }

    private void OnFullTranscription(string text)
    {
        Debug.Log("📜 Transcription: " + text);

        // Send to chatbot or store for use
        // e.g., ChatManager.SendToAI(text);
    }

    private void OnStopDictation()
    {
        Debug.Log("🛑 Dictation stopped.");
    }

    // Optional button triggers
    public void StartDictation() => dictationExperience.Activate();
    public void StopDictation() => dictationExperience.Deactivate();
}
