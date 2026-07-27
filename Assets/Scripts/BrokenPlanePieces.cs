using System.Collections;
using UnityEngine;

// Animates pieces of the plane by moving them away from the center
public class BrokenCityPieces : MonoBehaviour
{
    [Header("Center Point")]

    // Point the pieces move away from
    public Transform centerPoint;

    [Header("Crack Spacing")]

    // Controls how far each piece moves away from the center
    public float crackGapDistance = 0.08f;

    [Header("Small Side-to-Side Movement")]

    // Controls how pieces move sideways
    public float sideToSideAmount = 0.03f;

    public float sideToSideSpeed = 1.2f;

    [Header("Down Only Movement")]

    // Controls how far pieces move downward while broken
    public float downDistance = 0.035f;

    public float downSpeed = 1.5f;

    [Header("Randomness")]

    // This gives each piece a different timing offset
    public float randomDelayAmount = 2f;

    public float randomMovementVariation = 0.25f;

    [Header("Repair Animation")]

    public float repairDuration = 4f;

    [Header("Testing")]

    public bool pressRToRepair = false;

    // Stores all the pieces of the city
    // Each piece is a Transform from a child object with a Renderer
    private Transform[] pieces;

    // Arrays store each piece's original position
    private Vector3[] originalWorldPositions;
    private Quaternion[] originalWorldRotations;

    // Arrays store the broken/cracked positions and movement directions
    private Vector3[] crackedWorldPositions;
    private Vector3[] crackDirections;
    private Vector3[] sideDirections;

    // Arrays store random movement values for each piece
    private float[] movementMultipliers;
    private float[] timeOffsets;

    private bool isBroken = true;

    private bool isRepairing = false;


    void Start()
    {
        FindPiecesAndCreateCracks();
    }


    void Update()
    {
        // For testing, R key used to repair cracks
        if (pressRToRepair && Input.GetKeyDown(KeyCode.R))
        {
            RepairCity();
        }

        // If the city is not broken, do not animate broken movement
        // If it is currently repairing, also stop broken movement
        if (!isBroken || isRepairing)
        {
            return;
        }

        // Loop through every city piece
        for (int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i] == null)
            {
                continue;
            }

            // Create a smooth wave value between -1 and 1
            float sideWave = Mathf.Sin(Time.time * sideToSideSpeed + timeOffsets[i]);

            // sideWave gives the back-and-forth motion
            // sideToSideAmount controls the distance
            // movementMultipliers[i] makes each piece move slightly differently
            float sideMovement =
                sideWave *
                sideToSideAmount *
                movementMultipliers[i];

            // Create another smooth wave for downward movement
            float downWave = Mathf.Sin(Time.time * downSpeed + timeOffsets[i] * 2f);

            // Convert to 0 to 1 range for only downward movement
            float downOnlyWave = (downWave + 1f) * 0.5f;

            float yMovement =
                -downOnlyWave *
                downDistance *
                movementMultipliers[i];

            // Set the final position of this piece
            pieces[i].position =
                crackedWorldPositions[i] +
                sideDirections[i] * sideMovement +
                new Vector3(0f, yMovement, 0f);

            // Keep the piece from rotating or spinning while broken
            pieces[i].rotation = originalWorldRotations[i];
        }
    }

    private void FindPiecesAndCreateCracks()
    {
        // Find every Renderer under this object
        // Each renderer's Transform becomes one "piece" of the broken city
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        // Arrays with the same length as the number of renderers found
        pieces = new Transform[renderers.Length];

        originalWorldPositions = new Vector3[renderers.Length];
        originalWorldRotations = new Quaternion[renderers.Length];

        crackedWorldPositions = new Vector3[renderers.Length];
        crackDirections = new Vector3[renderers.Length];
        sideDirections = new Vector3[renderers.Length];

        movementMultipliers = new float[renderers.Length];
        timeOffsets = new float[renderers.Length];

        if (centerPoint == null)
        {
            centerPoint = transform;
        }

        Debug.Log("BrokenCityPieces found " + renderers.Length + " renderers under " + gameObject.name);

        // Iterate through every renderer 
        for (int i = 0; i < renderers.Length; i++)
        {
            // Store this renderer's Transform as one piece
            pieces[i] = renderers[i].transform;

            // Save original world position and rotation to later repair the city
            originalWorldPositions[i] = pieces[i].position;
            originalWorldRotations[i] = pieces[i].rotation;

            // Find the direction from the center point to this piece
            Vector3 directionAwayFromCenter =
                pieces[i].position - centerPoint.position;

            // Remove vertical direction
            directionAwayFromCenter.y = 0f;

            // Create a random horizontal direction for pieces too close to center
            if (directionAwayFromCenter.magnitude < 0.01f)
            {
                directionAwayFromCenter = Random.insideUnitSphere;
                directionAwayFromCenter.y = 0f;
            }

            // Normalize = make the direction length equal to 1
            crackDirections[i] = directionAwayFromCenter.normalized;

            // Takes the original position and pushes it away from the center
            crackedWorldPositions[i] =
                originalWorldPositions[i] +
                crackDirections[i] * crackGapDistance;


            // Vector3.Cross creates a perpendicular direction for sideways movement
            sideDirections[i] =
                Vector3.Cross(Vector3.up, crackDirections[i]).normalized;

            if (sideDirections[i].magnitude < 0.01f)
            {
                sideDirections[i] = Vector3.right;
            }

            // Give this piece a random movement strength
            movementMultipliers[i] = Random.Range(
                1f - randomMovementVariation,
                1f + randomMovementVariation
            );

            // Give this piece a random time offset
            timeOffsets[i] = Random.Range(0f, randomDelayAmount);

            // Move the piece into its cracked position right away
            pieces[i].position = crackedWorldPositions[i];

            // Keep the piece's original rotation
            pieces[i].rotation = originalWorldRotations[i];
        }
    }


    public void ResetCity()
    {
        StopAllCoroutines();
        isRepairing = false;
        isBroken = true;
    }


    public void RepairCity()
    {
        if (!isBroken || isRepairing)
        {
            Debug.Log("RepairCity was called, but city is already repaired or currently repairing.");
            return;
        }

        Debug.Log("RepairCity started on " + gameObject.name);

        // Start the repair animation
        StartCoroutine(RepairRoutine());
    }


    private IEnumerator RepairRoutine()
    {
        isRepairing = true;

        // Store the positions and rotations at the moment the repair starts
        Vector3[] startWorldPositions = new Vector3[pieces.Length];
        Quaternion[] startWorldRotations = new Quaternion[pieces.Length];

        // Save each piece's current position and rotation
        for (int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i] == null)
            {
                continue;
            }

            startWorldPositions[i] = pieces[i].position;
            startWorldRotations[i] = pieces[i].rotation;
        }

        // Tracks how much time has passed in the repair animation
        float elapsed = 0f;

        // Keep animating until elapsed time reaches repairDuration
        while (elapsed < repairDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / repairDuration;

            // SmoothStep makes the animation ease in and ease out
            t = Mathf.SmoothStep(0f, 1f, t);

            // Move every piece toward its original position
            for (int i = 0; i < pieces.Length; i++)
            {
                if (pieces[i] == null)
                {
                    continue;
                }

                // Lerp moves from the repair-start position
                // to the original position based on t
                pieces[i].position = Vector3.Lerp(
                    startWorldPositions[i],
                    originalWorldPositions[i],
                    t
                );

                // Slerp smoothly rotates from the repair-start rotation
                // back to the original rotation
                pieces[i].rotation = Quaternion.Slerp(
                    startWorldRotations[i],
                    originalWorldRotations[i],
                    t
                );
            }

            // Wait one frame then continue the loop
            yield return null;
        }

        // Forces every piece back to original position and rotation
        for (int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i] == null)
            {
                continue;
            }

            pieces[i].position = originalWorldPositions[i];
            pieces[i].rotation = originalWorldRotations[i];
        }

        isBroken = false;

        isRepairing = false;

        Debug.Log("RepairCity finished on " + gameObject.name);
    }
}