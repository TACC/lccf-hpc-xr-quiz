using UnityEngine;

// Makes the object orbit around center object
public class OrbitAroundTarget : MonoBehaviour
{
    // The object at the center of the orbit
    public Transform centerTarget;

    // Controls the length of the orbit circle
    public float orbitRadius = 1.5f;

    // The value is in degrees per second
    public float orbitSpeed = 90f;

    public float startingAngle = 0f;

    // Stores the current angle of the object around the circle
    private float angle;

    void Start()
    {
        // Where the object begins when the scene starts
        angle = startingAngle;

        ReturnToCurrentOrbitPosition();
    }

    void Update()
    {
        // Increase the angle over time
        // Time.deltaTime makes the movement frame-rate independent
        angle += orbitSpeed * Time.deltaTime;

        // Keep within normal circle range
        if (angle >= 360f)
        {
            angle -= 360f;
        }

        // Move object to new position on the circle
        ReturnToCurrentOrbitPosition();
    }

    public void ReturnToCurrentOrbitPosition()
    {
        if (centerTarget == null) return;

        // Multiply angle by Mathf.Deg2Rad to convert degrees into radians
        // Cos gives the x position on the circle
        float x = Mathf.Cos(angle * Mathf.Deg2Rad) * orbitRadius;

        // Sin gives the y position on the circle
        float y = Mathf.Sin(angle * Mathf.Deg2Rad) * orbitRadius;

        // centerTarget.position is the center of the orbit
        // new Vector3(x, y, 0) is the offset from the center
        transform.position = centerTarget.position + new Vector3(x, y, 0);
    }
}