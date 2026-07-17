using UnityEngine;

// Makes a GameObject pulse according to audio volume
// Used for intro ring movement
public class AudioRingPulse : MonoBehaviour
{
    [Header("Audio")]

    // AudioSource used for ring movement, dragged in Unity
    public AudioSource audioSource;


    [Header("Pulse Settings")]

    // Smallest scale possible, 1 = original size
    public float minScale = 1f;

    public float maxScale = 1.25f;

    // This controls how strongly the audio affects the ring
    // Higher sensitivity = bigger reactions
    public float sensitivity = 100f;

    // Controls rate of reactions for smooth movement
    public float smoothSpeed = 8f;

    private Vector3 originalScale;

    // Array stores pieces of the current audio wave
    // GetOutputData fills this array with audio sample values
    private float[] samples = new float[128];

    private float smoothedVolume = 0f;


    void Start()
    {
        // Save the ring's starting scale
        originalScale = transform.localScale;
    }


    void Update()
    {
        // Runs once every frame

        if (audioSource == null)
        {
            return;
        }

        // Get the current loudness of the audio
        float volume = GetAudioVolume();

        // Mathf.Lerp moves smoothedVolume toward volume gradually
        // smoothedVolume = the current smooth value
        // volume = the new target value
        // Time.deltaTime * smoothSpeed = how fast it moves toward the target
        smoothedVolume = Mathf.Lerp(
            smoothedVolume,
            volume,
            Time.deltaTime * smoothSpeed
        );

        // Convert the volume into a scale multiplier
        // smoothedVolume * sensitivity increases size based on audio loudness
        float targetScaleMultiplier = 1f + smoothedVolume * sensitivity;

        // Clamp used to keep the scale multiplier within range
        targetScaleMultiplier = Mathf.Clamp(
            targetScaleMultiplier,
            minScale,
            maxScale
        );

        // Calculate the size the ring should move toward
        Vector3 targetScale = originalScale * targetScaleMultiplier;

        // Smoothly move the current scale toward the target scale
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * smoothSpeed
        );
    }


    private float GetAudioVolume()
    {
        // Calculates how loud the audio is

        // Fill the samples array with the current audio waveform data
        audioSource.GetOutputData(samples, 0);

        // Holds the total loudness
        float total = 0f;

        // for each audio sample in the array
        for (int i = 0; i < samples.Length; i++)
        {
            // absolute value to measure loudness instead of direction (wave values)
            total += Mathf.Abs(samples[i]);
        }

        // return average loudness
        return total / samples.Length;
    }
}