using System.Collections;
using UnityEngine;

public class SuperCityTransitionController : MonoBehaviour
{
    // variables for z depth transition
    [Header("Analogy Depth Transition")]
    public float analogyZTransitionDistance = 5f;
    public float analogySlideDuration = 1.2f;

    [Header("Placement Depth Transition")]
    public float placementZTransitionDistance = 5f;
    public float placementSlideDuration = 1.2f;

    // Slides any GameObject from one position to another
    public IEnumerator SlideObject(GameObject obj, Vector3 startPosition, Vector3 endPosition, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {

            elapsedTime += Time.deltaTime;

            float t = elapsedTime / duration;

            t = Mathf.SmoothStep(0f, 1f, t);

            if (obj != null)
            {
                // Lerp = "linear interpolation"
                obj.transform.position = Vector3.Lerp(startPosition, endPosition, t);
            }

            yield return null;
        }

        // Force the object exactly to the final position
        if (obj != null)
        {
            obj.transform.position = endPosition;
        }
    }
}
