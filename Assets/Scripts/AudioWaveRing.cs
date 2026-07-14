using UnityEngine;

// LineRenderer is used to draw the ring shape
[RequireComponent(typeof(LineRenderer))]
public class AudioWaveRing : MonoBehaviour
{
    [Header("Audio")]

    public AudioSource audioSource;


    [Header("Ring")]

    // How many points are used to draw the circle
    public int pointCount = 360;

    public float baseRadius = 1.5f;

    [Header("Audio Reaction")]

    // How strongly the audio pushes the ring outward (spikes/waves)
    public float audioStrength = 25f;

    public float maxWaveSize = 0.25f;

    // How quickly the ring reacts when the audio gets louder
    public float attackSpeed = 40f;

    // This controls how slowly the ring shrinks back when loudness decreases
    public float releaseSpeed = 12f;

    [Header("Motion")]

    // Makes the wave pattern rotate around the center
    public float rotateSpeed = 12f;

    // Small wave even when the audio is quiet
    public float idleWaveAmount = 0.02f;

    // How many idle waves appear around the ring
    public float idleWaveFrequency = 5f;

    public float idleWaveSpeed = 2f;

    private LineRenderer lineRenderer;

    // Stores 512 audio wave values from the AudioSource
    private float[] audioSamples = new float[512];

    // Stores the current outward movement for each ring point
    private float[] currentOffsets;


    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        // Makes the LineRenderer connect the last point back to the first point
        lineRenderer.loop = true;

        // False means the points are positioned relative to this GameObject
        lineRenderer.useWorldSpace = false;

        // Tell the LineRenderer how many points it needs to draw
        lineRenderer.positionCount = pointCount;

        // Keeps track of how far each point is currently pushed outward
        currentOffsets = new float[pointCount];
    }


    void Update()
    {
        if (audioSource == null)
        {
            return;
        }

        // Fill the audioSamples array with the current sound wave data
        audioSource.GetOutputData(audioSamples, 0);

        // Go through every point in the ring
        for (int i = 0; i < pointCount; i++)
        {
            // Percent tells us how far around the circle this point is
            float percent = (float)i / pointCount;

            float angle = percent * Mathf.PI * 2f;

            // Pick an audio sample that matches this point's position around the ring
            int audioIndex = Mathf.FloorToInt(percent * audioSamples.Length);

            // Get the audio value at that sample
            float audioValue = Mathf.Abs(audioSamples[audioIndex]);

            // Turn the audio value into an outward movement amount
            float audioOffset = audioValue * audioStrength;

            audioOffset = Mathf.Clamp(audioOffset, 0f, maxWaveSize);


            // angle * idleWaveFrequency controls how many waves go around the circle
            // Time.time * idleWaveSpeed makes the wave move over time
            // Multiplying by idleWaveAmount keeps the idle wave small
            float idleOffset = Mathf.Sin(
                angle * idleWaveFrequency + Time.time * idleWaveSpeed
            ) * idleWaveAmount;

            // The final target offset is audio movement plus idle movement
            float targetOffset = audioOffset + idleOffset;

            float speed;

            // target offset > current offset = moving outward
            if (targetOffset > currentOffsets[i])
            {
                speed = attackSpeed;
            }
            // shrinks back smoothly
            else
            {
                speed = releaseSpeed;
            }

            // Smoothly move the current offset toward the target offset
            currentOffsets[i] = Mathf.Lerp(
                currentOffsets[i],
                targetOffset,
                Time.deltaTime * speed
            );

            // Calculate the final radius for this point
            float radius = baseRadius + currentOffsets[i];

            // Add rotation to the angle
            float rotatedAngle = angle + Time.time * rotateSpeed * Mathf.Deg2Rad;

            // Convert the angle and radius into an x position
            float x = Mathf.Cos(rotatedAngle) * radius;

            // Convert the angle and radius into a y position.
            float y = Mathf.Sin(rotatedAngle) * radius;

            // Set this point's position in the LineRenderer
            lineRenderer.SetPosition(i, new Vector3(x, y, 0f));
        }
    }
}