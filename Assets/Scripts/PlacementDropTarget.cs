using UnityEngine;

// Goes on the correct drop area on the motherboard
public class PlacementDropTarget : MonoBehaviour
{
    [Header("Drop Settings")]
    public string expectedItemID;

    [Header("Snap Point")]
    public Transform snapPoint;

    [Header("Target Glow")]
    public Renderer targetRenderer;

    public Material glowMaterial;

    public bool hideTargetAtStart = true;

    [Header("Manager")]
    public SuperCityManager superCityManager;

    private bool alreadyTriggered = false;

    private Material originalMaterial;

    [Header("Installed Object")]
    public GameObject installedObject;

    void Start()
    {
        if (snapPoint == null)
        {
            snapPoint = transform;
        }

        if (superCityManager == null)
        {
            superCityManager = FindObjectOfType<SuperCityManager>();
        }

        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }

        if (targetRenderer != null)
        {
            originalMaterial = targetRenderer.material;

            if (hideTargetAtStart)
            {
                targetRenderer.enabled = false;
            }
        }

        if (installedObject != null)
        {
            installedObject.SetActive(false);
        }
    }

    void OnEnable()
    {
        alreadyTriggered = false;

        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }

        if (targetRenderer != null && hideTargetAtStart)
        {
            targetRenderer.enabled = false;
        }
    }

    public void ShowGlow()
    {
        // Makes the snap target visible after the first wrong guess
        if (targetRenderer != null)
        {
            targetRenderer.enabled = true;

            if (glowMaterial != null)
            {
                targetRenderer.material = glowMaterial;
            }
        }
    }

    public void HideGlow()
    {
        // Hides the snap target again once placed correctly
        if (targetRenderer != null)
        {
            if (originalMaterial != null)
            {
                targetRenderer.material = originalMaterial;
            }

            targetRenderer.enabled = false;
        }
    }

    public void TriggerSuccess()
    {
        if (alreadyTriggered)
        {
            return;
        }

        alreadyTriggered = true;

        HideGlow();

        Debug.Log("Correct placement: " + expectedItemID);

        if (installedObject != null)
        {
            installedObject.SetActive(true);
        }

        if (superCityManager == null)
        {
            superCityManager = FindObjectOfType<SuperCityManager>();
        }

        if (superCityManager != null)
        {
            superCityManager.OnHardwarePlaced();
        }
        else
        {
            Debug.LogWarning("SuperCityManager could not be found for " + gameObject.name);
        }
    }

    public void ResetTarget()
    {
        if (installedObject != null)
        {
            installedObject.SetActive(false);
        }
    }
}