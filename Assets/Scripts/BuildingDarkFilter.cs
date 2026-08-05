using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class BuildingDarkFilter : MonoBehaviour
{
    [Header("Dark Filter Material")]
    public Renderer filterRenderer;

    [Header("Darkness Settings")]
    public float startAlpha = 0.85f;
    public float endAlpha = 0.35f;

    [Header("Animation Settings")]
    public float fadeDuration = 1.5f;
    public float waitAfterFade = 1.0f;

    [Header("After Fade Event")]
    public UnityEvent onFadeFinished;

    private Material filterMaterial;
    private bool hasPlayed = false;

    void Start()
    {
        InitializeMaterial();
        ResetFilter();
    }

    public void FadeToDim()
    {
        if (hasPlayed) return;

        hasPlayed = true;
        StartCoroutine(FadeRoutine());
    }

    private void InitializeMaterial()
    {
        if (filterRenderer == null)
        {
            filterRenderer = GetComponent<Renderer>();
        }

        if (filterRenderer == null)
        {
            Debug.LogWarning("No filter renderer assigned on " + name);
            return;
        }

        if (filterMaterial == null)
        {
            filterMaterial = filterRenderer.material;
        }
    }

    public void ResetFilter()
    {
        StopAllCoroutines();

        hasPlayed = false;

        InitializeMaterial();
        SetAlpha(startAlpha);
    }

    private IEnumerator FadeRoutine()
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float t = timer / fadeDuration;
            float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, t);

            SetAlpha(currentAlpha);

            yield return null;
        }

        SetAlpha(endAlpha);

        yield return new WaitForSeconds(waitAfterFade);

        onFadeFinished.Invoke();
    }

    private void SetAlpha(float alpha)
    {
        if (filterMaterial == null) return;

        Color color = filterMaterial.color;
        color.a = alpha;
        filterMaterial.color = color;
    }
}