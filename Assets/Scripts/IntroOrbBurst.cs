using System.Collections;
using UnityEngine;

public class IntroOrbBurst : MonoBehaviour
{
    [Header("Objects")]
    public GameObject talkingOrbObject;
    public GameObject innerGlowObject;
    public Transform innerGlowTransform;

    [Header("Additional Intro Object")]
    public GameObject introImageObject;

    [Header("Timing")]
    public float orbFadeDuration = 0.65f;
    public float glowGrowDuration = 0.65f;
    public float brightHoldDuration = 0.2f;
    public float glowFadeDuration = 0.65f;

    [Header("Glow Scale")]
    public float glowStartScale = 1f;
    public float glowPeakScale = 7f;

    private Vector3 originalGlowScale;
    private Vector3 originalGlowLocalPosition;

    void Awake()
    {
        if (innerGlowTransform != null)
        {
            originalGlowScale = innerGlowTransform.localScale;
            originalGlowLocalPosition = innerGlowTransform.localPosition;
        }

        if (innerGlowObject != null)
        {
            innerGlowObject.SetActive(false);
        }
    }

    public IEnumerator PlayBurstThenReveal(GameObject revealObject)
    {
        ResetIntro();

        if (innerGlowObject == null || innerGlowTransform == null)
        {
            yield break;
        }

        if (revealObject != null)
        {
            revealObject.SetActive(false);
        }

        if (introImageObject != null)
        {
            introImageObject.SetActive(false);
        }

        innerGlowObject.SetActive(true);
        innerGlowTransform.localPosition = originalGlowLocalPosition;
        innerGlowTransform.localScale = originalGlowScale * glowStartScale;
        SetObjectAlpha(innerGlowObject, 0f);

        if (talkingOrbObject != null)
        {
            talkingOrbObject.SetActive(true);
            SetObjectAlpha(talkingOrbObject, 1f);
        }

        // Fade the talking orb out while the inner glow grows
        float elapsedTime = 0f;

        Vector3 startScale = originalGlowScale * glowStartScale;
        Vector3 peakScale = originalGlowScale * glowPeakScale;

        float combinedDuration = Mathf.Max(orbFadeDuration, glowGrowDuration);

        while (elapsedTime < combinedDuration)
        {
            elapsedTime += Time.deltaTime;

            float orbT = Mathf.Clamp01(elapsedTime / orbFadeDuration);
            float glowT = Mathf.Clamp01(elapsedTime / glowGrowDuration);

            orbT = Mathf.SmoothStep(0f, 1f, orbT);
            glowT = Mathf.SmoothStep(0f, 1f, glowT);

            // Talking orb fades out
            if (talkingOrbObject != null)
            {
                SetObjectAlpha(talkingOrbObject, 1f - orbT);
            }

            // Inner glow grows and gets brighter
            innerGlowTransform.localScale = Vector3.Lerp(startScale, peakScale, glowT);
            SetObjectAlpha(innerGlowObject, glowT);

            yield return null;
        }

        // Force final state
        if (talkingOrbObject != null)
        {
            SetObjectAlpha(talkingOrbObject, 0f);
            talkingOrbObject.SetActive(false);
        }

        innerGlowTransform.localScale = peakScale;
        SetObjectAlpha(innerGlowObject, 1f);

        // Hold the bright glow for a moment
        if (brightHoldDuration > 0f)
        {
            yield return new WaitForSeconds(brightHoldDuration);
        }

        // Reveal the first analogy while glowing
        if (revealObject != null)
        {
            revealObject.SetActive(true);
            SetObjectAlpha(revealObject, 1f);
        }

        // Fade the glow away
        elapsedTime = 0f;

        while (elapsedTime < glowFadeDuration)
        {
            elapsedTime += Time.deltaTime;

            float t = elapsedTime / glowFadeDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            SetObjectAlpha(innerGlowObject, 1f - t);

            yield return null;
        }

        SetObjectAlpha(innerGlowObject, 0f);
        innerGlowObject.SetActive(false);
    }

    private void SetObjectAlpha(GameObject obj, float alpha)
    {
        if (obj == null)
        {
            return;
        }

        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
            {
                continue;
            }

            foreach (Material material in renderer.materials)
            {
                if (material == null)
                {
                    continue;
                }

                if (material.HasProperty("_BaseColor"))
                {
                    Color color = material.GetColor("_BaseColor");
                    color.a = alpha;
                    material.SetColor("_BaseColor", color);
                }
                else if (material.HasProperty("_Color"))
                {
                    Color color = material.color;
                    color.a = alpha;
                    material.color = color;
                }
            }
        }
    }

    public void ResetIntro()
    {
        StopAllCoroutines();

        if (innerGlowTransform != null)
        {
            innerGlowTransform.localScale = originalGlowScale;
            innerGlowTransform.localPosition = originalGlowLocalPosition;
        }

        if (innerGlowObject != null)
        {
            SetObjectAlpha(innerGlowObject, 0f);
            innerGlowObject.SetActive(false);
        }

        if (talkingOrbObject != null)
        {
            talkingOrbObject.SetActive(true);
            SetObjectAlpha(talkingOrbObject, 1f);
        }
    }
}