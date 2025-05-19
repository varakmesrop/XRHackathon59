using Meta.WitAi.TTS.Utilities;
using System.Collections;
using UnityEngine;

public class SpeakerTest : MonoBehaviour
{
    [SerializeField] private TTSSpeaker _speaker;
    [SerializeField] private Animator _animator;

    private bool isSpeaking = false;

    void Start()
    {
        FindAnyObjectByType<LLM_Groq>()?.OnLLMResponse.AddListener(ParrotTalk);
        _speaker.Events.OnAudioClipPlaybackFinished.AddListener(SpeakFinish);
    }

    private void SpeakFinish(AudioClip clip)
    {
        isSpeaking = false;
        UpdateAnimator();
        VisualLoggerWall.Log("Speak finished...");
    }

    private void ParrotTalk(string text)
    {
        VisualLoggerWall.Log("Parrotting...");

        if (isSpeaking)
        {
            VisualLoggerWall.Log("Still speaking");
            return;
        }

        if (text.Length < 5)
        {
            VisualLoggerWall.Log("Message not long enough");
            return;
        }

        if (text.Length > 275)
        {
            text = "I'm sorry, I am not sure I understand you";
        }

        isSpeaking = true;
        UpdateAnimator();

        _speaker.Speak(text);
    }

    private void UpdateAnimator()
    {
        if (_animator != null)
        {
            _animator.SetBool("Talking", isSpeaking);
        }
    }
}
