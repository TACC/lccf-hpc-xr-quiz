using System.Collections;
using UnityEngine;

public class VanDriveOff : MonoBehaviour
{
    [Header("Manager")]
    public SuperCityManager superCityManager;

    [Header("Wheel Mesh Objects")]
    public GameObject[] wheels;

    [Header("Wheel Appear Timing")]
    public float timeBetweenWheelAppear = 0.25f;
    public float delayAfterWheelsAppear = 0.4f;

    [Header("Drive Settings")]
    public Vector3 driveDirection = Vector3.right;
    public float driveDistance = 8f;
    public float driveDuration = 3f;

    [Header("Wheel Spin Settings")]
    public Vector3 wheelSpinAxis = Vector3.forward;
    public float wheelSpinSpeed = -720f;

    private Transform[] wheelSpinPivots;
    private bool isDriving = false;

    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;
    private Quaternion[] originalPivotLocalRotations;
    private Vector3[] originalWheelLocalPositions;
    private Quaternion[] originalWheelLocalRotations;

    void Start()
    {
        if (superCityManager == null)
        {
            superCityManager = FindObjectOfType<SuperCityManager>();
        }

        if (wheelSpinAxis.sqrMagnitude < 0.001f)
        {
            wheelSpinAxis = Vector3.forward;
        }

        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;

        wheelSpinAxis.Normalize();

        wheelSpinPivots = new Transform[wheels.Length];

        originalPivotLocalRotations = new Quaternion[wheels.Length];
        originalWheelLocalPositions = new Vector3[wheels.Length];
        originalWheelLocalRotations = new Quaternion[wheels.Length];

        for (int i = 0; i < wheels.Length; i++)
        {
            if (wheels[i] != null)
            {
                wheelSpinPivots[i] = CreateSpinPivotForWheel(wheels[i]);

                originalPivotLocalRotations[i] = wheelSpinPivots[i].localRotation;
                originalWheelLocalPositions[i] = wheels[i].transform.localPosition;
                originalWheelLocalRotations[i] = wheels[i].transform.localRotation;

                wheels[i].SetActive(false);
            }
        }
    }

    Transform CreateSpinPivotForWheel(GameObject wheel)
    {
        Renderer[] renderers = wheel.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            Debug.LogWarning("No Renderer found on " + wheel.name + ". Wheel may still spin wrong.");
            return wheel.transform;
        }

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        Vector3 wheelCenter = bounds.center;

        GameObject pivotObject = new GameObject(wheel.name + "_SpinPivot");
        Transform pivot = pivotObject.transform;

        pivot.SetParent(wheel.transform.parent, true);

        pivot.position = wheelCenter;
        pivot.rotation = wheel.transform.rotation;
        pivot.localScale = Vector3.one;

        wheel.transform.SetParent(pivot, true);

        return pivot;
    }

    public void TriggerVanSuccess()
    {
        if (isDriving) return;

        StartCoroutine(ShowWheelsThenDriveOff());
    }

    IEnumerator ShowWheelsThenDriveOff()
    {
        isDriving = true;

        foreach (GameObject wheel in wheels)
        {
            if (wheel != null)
            {
                wheel.SetActive(true);
            }

            yield return new WaitForSeconds(timeBetweenWheelAppear);
        }

        yield return new WaitForSeconds(delayAfterWheelsAppear);

        Vector3 start = transform.position;
        Vector3 end = start + driveDirection.normalized * driveDistance;

        float timer = 0f;

        while (timer < driveDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / driveDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(start, end, smoothT);

            SpinWheels();

            yield return null;
        }

        transform.position = end;

        if (superCityManager != null)
        {
            superCityManager.OnAnalogySolved();
        }
        else
        {
            Debug.LogWarning("SuperCityManager was not found. Could not call OnAnalogySolved.");
        }
    }

    void SpinWheels()
    {
        for (int i = 0; i < wheelSpinPivots.Length; i++)
        {
            if (wheelSpinPivots[i] != null)
            {
                wheelSpinPivots[i].Rotate(
                    wheelSpinAxis * wheelSpinSpeed * Time.deltaTime,
                    Space.Self
                );
            }
        }
    }

    public void ResetVan()
    {
        StopAllCoroutines();

        isDriving = false;

        transform.localPosition = originalLocalPosition;
        transform.localRotation = originalLocalRotation;

        if (wheels == null)
        {
            return;
        }

        for (int i = 0; i < wheels.Length; i++)
        {
            if (wheels[i] == null)
            {
                continue;
            }

            if (wheelSpinPivots != null &&
                i < wheelSpinPivots.Length &&
                wheelSpinPivots[i] != null)
            {
                if (wheels[i].transform.parent != wheelSpinPivots[i])
                {
                    wheels[i].transform.SetParent(wheelSpinPivots[i], false);
                }

                if (originalPivotLocalRotations != null &&
                    i < originalPivotLocalRotations.Length)
                {
                    wheelSpinPivots[i].localRotation =
                        originalPivotLocalRotations[i];
                }
            }

            if (originalWheelLocalPositions != null &&
                i < originalWheelLocalPositions.Length)
            {
                wheels[i].transform.localPosition =
                    originalWheelLocalPositions[i];
            }

            if (originalWheelLocalRotations != null &&
                i < originalWheelLocalRotations.Length)
            {
                wheels[i].transform.localRotation =
                    originalWheelLocalRotations[i];
            }

            wheels[i].SetActive(false);
        }
    }
}