using System.Collections;
using UnityEngine;

public class AudioSpace : MonoBehaviour
{
    [Header("Ambient Sound")]
    [SerializeField] private AudioClip ambientClip;
    [SerializeField] private float ambientVolume = 0.1f;

    [Header("Distracting Sounds")]
    [SerializeField] private AudioSource[] distractingSources; // Pre-setup with clip & transform
    [SerializeField] private float minTriggerTime = 30f;
    [SerializeField] private float maxTriggerTime = 90f;

    private AudioSource ambientSource;

    private void Start()
    {
        // Setup and play ambient audio
        ambientSource = gameObject.AddComponent<AudioSource>();
        ambientSource.clip = ambientClip;
        ambientSource.loop = true;
        ambientSource.volume = ambientVolume;
        ambientSource.spatialBlend = 0f; // 2D sound for uniform background
        ambientSource.Play();

        // Start the distracting sound coroutine
        StartCoroutine(PlayDistractingSounds());
    }

    private IEnumerator PlayDistractingSounds()
    {
        while (true)
        {
            float waitTime = Random.Range(minTriggerTime, maxTriggerTime);
            yield return new WaitForSeconds(waitTime);

            if (distractingSources.Length > 0)
            {
                int index = Random.Range(0, distractingSources.Length);
                AudioSource source = distractingSources[index];
                source.Play();
            }
        }
    }
}
