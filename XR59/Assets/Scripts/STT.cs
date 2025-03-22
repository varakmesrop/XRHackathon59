using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class STT : MonoBehaviour
{
    [SerializeField]
    private string HF_INF_API_KEY;
    const string STT_API_URL = "https://router.huggingface.co/hf-inference/models/openai/whisper-tiny";

    MemoryStream stream;

    [SerializeField]
    private LLM_Groq groqScript;

    private void Start()
    {
        if (!groqScript)
        {
            Debug.LogError("LLM_Groq component not found!");
            return;
        }

        StartSpeaking();
    }

    private void StartSpeaking()
    {
        AudioSource aud = GetComponent<AudioSource>();
        Debug.Log("Start recording");
        aud.clip = Microphone.Start(null, false, 30, 11025); // Use default mic
        StartCoroutine(RecordAudio(aud.clip));
    }

    IEnumerator RecordAudio(AudioClip clip)
    {
        while (Microphone.IsRecording(null))
        {
            yield return null;
        }

        AudioSource aud = GetComponent<AudioSource>();
        ConvertClipToWav(aud.clip);
        StartCoroutine(PerformSTT());
    }

    IEnumerator PerformSTT()
    {
        UnityWebRequest request = new UnityWebRequest(STT_API_URL, "POST");
        request.uploadHandler = new UploadHandlerRaw(stream.GetBuffer());
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + HF_INF_API_KEY);

        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            SpeechToTextData sttResponse = JsonUtility.FromJson<SpeechToTextData>(responseText);
            Debug.Log("Transcribed Text: " + sttResponse.text);

            // Pass the transcribed text to the Groq API script
            groqScript.TextToLLM(sttResponse.text);
        }
        else
        {
            Debug.LogError("API request failed: " + request.error);
        }
    }

    [Serializable]
    public class SpeechToTextData
    {
        public string text;
    }

    Stream ConvertClipToWav(AudioClip clip)
    {
        var data = new float[clip.samples * clip.channels];
        clip.GetData(data, 0);

        if (stream != null) stream.Dispose();
        stream = new MemoryStream();

        var bitsPerSample = (ushort)16;
        var chunkID = "RIFF";
        var format = "WAVE";
        var subChunk1ID = "fmt ";
        var subChunk1Size = (uint)16;
        var audioFormat = (ushort)1;
        var numChannels = (ushort)clip.channels;
        var sampleRate = (uint)clip.frequency;
        var byteRate = (uint)(sampleRate * clip.channels * bitsPerSample / 8);
        var blockAlign = (ushort)(numChannels * bitsPerSample / 8);
        var subChunk2ID = "data";
        var subChunk2Size = (uint)(data.Length * clip.channels * bitsPerSample / 8);
        var chunkSize = (uint)(36 + subChunk2Size);

        WriteString(stream, chunkID);
        WriteUInt(stream, chunkSize);
        WriteString(stream, format);
        WriteString(stream, subChunk1ID);
        WriteUInt(stream, subChunk1Size);
        WriteShort(stream, audioFormat);
        WriteShort(stream, numChannels);
        WriteUInt(stream, sampleRate);
        WriteUInt(stream, byteRate);
        WriteShort(stream, blockAlign);
        WriteShort(stream, bitsPerSample);
        WriteString(stream, subChunk2ID);
        WriteUInt(stream, subChunk2Size);

        foreach (var sample in data)
        {
            var deNormalizedSample = (short)0;
            if (sample > 0)
            {
                var temp = sample * short.MaxValue;
                if (temp > short.MaxValue)
                    temp = short.MaxValue;
                deNormalizedSample = (short)temp;
            }
            if (sample < 0)
            {
                var temp = sample * (-short.MinValue);
                if (temp < short.MinValue)
                    temp = short.MinValue;
                deNormalizedSample = (short)temp;
            }
            WriteShort(stream, (ushort)deNormalizedSample);
        }

        return stream;
    }

    void WriteUInt(Stream stream, uint data)
    {
        stream.WriteByte((byte)(data & 0xFF));
        stream.WriteByte((byte)((data >> 8) & 0xFF));
        stream.WriteByte((byte)((data >> 16) & 0xFF));
        stream.WriteByte((byte)((data >> 24) & 0xFF));
    }

    void WriteShort(Stream stream, ushort data)
    {
        stream.WriteByte((byte)(data & 0xFF));
        stream.WriteByte((byte)((data >> 8) & 0xFF));
    }

    void WriteString(Stream stream, string value)
    {
        foreach (var character in value)
            stream.WriteByte((byte)character);
    }
}

