using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VisualLoggerWall : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int maxLines = 10;         // Max lines to display
    [SerializeField] private float lineSpacing = 0.1f;  // Vertical spacing (world units)
    [SerializeField] private float fontSize = 0.5f;     // Font size in world units

    [Header("References")]
    [SerializeField] private TMP_Text textComponent;    // Single TextMeshPro component

    private Queue<string> messageQueue = new Queue<string>();
    private string currentText = "";

    // Singleton pattern
    private static VisualLoggerWall _instance;
    public static VisualLoggerWall Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<VisualLoggerWall>();
            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
            return;
        }
        _instance = this;

        if (textComponent == null)
        {
            textComponent = GetComponent<TMP_Text>();
            if (textComponent == null)
            {
                Debug.LogError("No TMP_Text component found!");
                enabled = false;
                return;
            }
        }

        textComponent.fontSize = fontSize;
        textComponent.lineSpacing = lineSpacing;
        Clear();
    }

    public static void Log(string message)
    {
        if (Instance != null) Instance.AddMessage(message);
    }

    public static void Clear()
    {
        if (Instance != null) Instance.ClearMessages();
    }

    private void AddMessage(string message)
    {
        messageQueue.Enqueue(message);

        // Remove oldest message if over limit
        if (messageQueue.Count > maxLines)
        {
            messageQueue.Dequeue();
        }

        // Rebuild text with all messages
        currentText = string.Join("\n", messageQueue.ToArray());
        textComponent.text = currentText;
    }

    private void ClearMessages()
    {
        messageQueue.Clear();
        textComponent.text = "";
        currentText = "";
    }
}