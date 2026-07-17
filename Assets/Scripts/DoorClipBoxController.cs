using UnityEngine;

public class DoorClipBoxController : MonoBehaviour
{
    [Header("Invisible Box That Defines Door Opening")]
    public Transform clipBox;

    [Header("Door Renderers")]
    public Renderer[] doorRenderers;

    [Header("Settings")]
    public bool updateEveryFrame = true;

    void Start()
    {
        ApplyClipBox();
    }

    void Update()
    {
        if (updateEveryFrame)
        {
            ApplyClipBox();
        }
    }

    void ApplyClipBox()
    {
        if (clipBox == null || doorRenderers == null)
        {
            return;
        }

        Vector3 center = clipBox.position;
        Vector3 size = clipBox.lossyScale;

        Vector3 halfSize = size * 0.5f;

        Vector3 clipMin = center - halfSize;
        Vector3 clipMax = center + halfSize;

        foreach (Renderer doorRenderer in doorRenderers)
        {
            if (doorRenderer == null) continue;

            foreach (Material mat in doorRenderer.materials)
            {
                if (mat == null) continue;

                mat.SetVector("_ClipMin", clipMin);
                mat.SetVector("_ClipMax", clipMax);
            }
        }
    }
}