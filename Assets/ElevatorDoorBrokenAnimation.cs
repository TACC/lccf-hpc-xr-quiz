using System.Collections;
using UnityEngine;

public class ElevatorDoorBrokenAnimation : MonoBehaviour
{
    [Header("Door Pieces")]
    public Transform L;
    public Transform R;
    public Transform L_2;
    public Transform R_2;

    [Header("Broken Movement")]
    public float brokenMoveAmount = 0.35f;
    public float brokenMoveSpeed = 2.5f;

    [Header("Fix Timing")]
    public float returnToNormalDuration = 0.4f;
    public float openDuration = 1.2f;

    [Header("Open Settings")]
    public float openDistance = 1.5f;

    [Header("Manager")]
    public SuperCityManager superCityManager;
    public bool callManagerWhenDone = true;

    private Vector3 L_Start;
    private Vector3 R_Start;
    private Vector3 L2_Start;
    private Vector3 R2_Start;

    private bool isBroken = true;
    private bool isFixing = false;

    void Start()
    {
        if (superCityManager == null)
        {
            superCityManager = FindObjectOfType<SuperCityManager>();
        }

        SaveStartPositions();
    }

    void Update()
    {
        if (isBroken && !isFixing)
        {
            AnimateBrokenDoors();
        }
    }

    void SaveStartPositions()
    {
        if (L != null) L_Start = L.localPosition;
        if (R != null) R_Start = R.localPosition;
        if (L_2 != null) L2_Start = L_2.localPosition;
        if (R_2 != null) R2_Start = R_2.localPosition;
    }

    void AnimateBrokenDoors()
    {
        float move = Mathf.Sin(Time.time * brokenMoveSpeed) * brokenMoveAmount;

        if (L != null)
        {
            L.localPosition = L_Start + Vector3.down * move;
        }

        if (R != null)
        {
            R.localPosition = R_Start + Vector3.down * move;
        }

        if (L_2 != null)
        {
            L_2.localPosition = L2_Start + Vector3.down * move;
        }

        if (R_2 != null)
        {
            R_2.localPosition = R2_Start + Vector3.down * move;
        }
    }

    public void TriggerElevatorSuccess()
    {
        if (isFixing) return;

        StartCoroutine(FixThenOpenDoors());
    }

    IEnumerator FixThenOpenDoors()
    {
        isFixing = true;
        isBroken = false;

        yield return StartCoroutine(ReturnDoorsToNormal());

        yield return StartCoroutine(OpenDoors());

        HideDoors();

        if (callManagerWhenDone && superCityManager != null)
        {
            superCityManager.OnAnalogySolved();
        }
    }

    IEnumerator ReturnDoorsToNormal()
    {
        float timer = 0f;

        Vector3 L_Current = L != null ? L.localPosition : Vector3.zero;
        Vector3 R_Current = R != null ? R.localPosition : Vector3.zero;
        Vector3 L2_Current = L_2 != null ? L_2.localPosition : Vector3.zero;
        Vector3 R2_Current = R_2 != null ? R_2.localPosition : Vector3.zero;

        while (timer < returnToNormalDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / returnToNormalDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            if (L != null)
            {
                L.localPosition = Vector3.Lerp(L_Current, L_Start, smoothT);
            }

            if (R != null)
            {
                R.localPosition = Vector3.Lerp(R_Current, R_Start, smoothT);
            }

            if (L_2 != null)
            {
                L_2.localPosition = Vector3.Lerp(L2_Current, L2_Start, smoothT);
            }

            if (R_2 != null)
            {
                R_2.localPosition = Vector3.Lerp(R2_Current, R2_Start, smoothT);
            }

            yield return null;
        }

        if (L != null) L.localPosition = L_Start;
        if (R != null) R.localPosition = R_Start;
        if (L_2 != null) L_2.localPosition = L2_Start;
        if (R_2 != null) R_2.localPosition = R2_Start;
    }

    IEnumerator OpenDoors()
    {
        float timer = 0f;

        Vector3 L_End = L_Start + Vector3.left * openDistance;
        Vector3 R_End = R_Start + Vector3.right * openDistance;

        Vector3 L2_End = L2_Start + Vector3.left * openDistance;
        Vector3 R2_End = R2_Start + Vector3.right * openDistance;

        while (timer < openDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / openDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            if (L != null)
            {
                L.localPosition = Vector3.Lerp(L_Start, L_End, smoothT);
            }

            if (R != null)
            {
                R.localPosition = Vector3.Lerp(R_Start, R_End, smoothT);
            }

            if (L_2 != null)
            {
                L_2.localPosition = Vector3.Lerp(L2_Start, L2_End, smoothT);
            }

            if (R_2 != null)
            {
                R_2.localPosition = Vector3.Lerp(R2_Start, R2_End, smoothT);
            }

            yield return null;
        }

        if (L != null) L.localPosition = L_End;
        if (R != null) R.localPosition = R_End;
        if (L_2 != null) L_2.localPosition = L2_End;
        if (R_2 != null) R_2.localPosition = R2_End;
    }

    void HideDoors()
    {
        if (L != null) L.gameObject.SetActive(false);
        if (R != null) R.gameObject.SetActive(false);
        if (L_2 != null) L_2.gameObject.SetActive(false);
        if (R_2 != null) R_2.gameObject.SetActive(false);
    }
}