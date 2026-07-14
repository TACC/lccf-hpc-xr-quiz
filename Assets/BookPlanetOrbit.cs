using System.Collections;
using UnityEngine;

public class BookPlanetOrbit : MonoBehaviour
{
    [Header("Orbit Center")]
    public Transform bookshelfCenter;

    [Header("Orbit Shape")]
    public float orbitRadiusX = 2.5f;
    public float orbitRadiusY = 1.3f;

    [Header("Orbit Movement")]
    public float orbitSpeed = 50f;
    public float startingAngle = 0f;

    [Header("Orbit Plane Tilt")]
    public Vector3 orbitPlaneRotation = Vector3.zero;

    private float currentAngle;
    private bool isOrbiting = false;

    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;

    void Start()
    {
        currentAngle = startingAngle;

        SaveCurrentPositionAsOriginal();

        if (bookshelfCenter == null)
        {
            Debug.LogWarning(gameObject.name + " has no bookshelf center assigned.");
        }
    }

    void Update()
    {
        if (!isOrbiting || bookshelfCenter == null)
        {
            return;
        }

        currentAngle += orbitSpeed * Time.deltaTime;

        float radians = currentAngle * Mathf.Deg2Rad;

        Vector3 localOrbitPosition = new Vector3(
            Mathf.Cos(radians) * orbitRadiusX,
            Mathf.Sin(radians) * orbitRadiusY,
            0f
        );

        Quaternion planeTilt = Quaternion.Euler(orbitPlaneRotation);
        Vector3 tiltedOrbitPosition = planeTilt * localOrbitPosition;

        // Convert the world orbit position into the book parent's local space
        Vector3 worldOrbitPosition = bookshelfCenter.position + tiltedOrbitPosition;

        if (transform.parent != null)
        {
            transform.localPosition = transform.parent.InverseTransformPoint(worldOrbitPosition);
        }
        else
        {
            transform.position = worldOrbitPosition;
        }

        // Keeps the book from rotating
        transform.localRotation = originalLocalRotation;
    }

    public void SaveCurrentPositionAsOriginal()
    {
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;
    }

    public void StartOrbit()
    {
        currentAngle = startingAngle;
        isOrbiting = true;
    }

    public void StopOrbit()
    {
        isOrbiting = false;
    }

    public IEnumerator SlideBackToOriginalPosition(float duration)
    {
        StopOrbit();

        Vector3 startLocalPosition = transform.localPosition;
        Quaternion startLocalRotation = transform.localRotation;

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float t = elapsedTime / duration;
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.localPosition = Vector3.Lerp(
                startLocalPosition,
                originalLocalPosition,
                t
            );

            transform.localRotation = Quaternion.Lerp(
                startLocalRotation,
                originalLocalRotation,
                t
            );

            yield return null;
        }

        transform.localPosition = originalLocalPosition;
        transform.localRotation = originalLocalRotation;
    }
}