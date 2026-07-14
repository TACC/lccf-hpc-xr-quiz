using UnityEngine;
using UnityEngine.Events;


// Goes on the correct placement zone
//
// Stores expected itemID, changes color based on item,
// can make target glow, and runs UnityEvents
public class ZDropTarget : MonoBehaviour
{
    [Header("Drop Settings")]

    public string expectedItemID;

    [Header("Scene Type")]

    public bool isAnalogyDropTarget = false;

    [Header("Feedback Colors")]

    // Default color of the target
    public Color normalColor = Color.white;

    // Color the target changes to when the correct item is dropped
    public Color correctColor = Color.green;

    public Color incorrectColor = Color.red;

    [Header("Glow Settings")]

    // The object will use emission glow when it changes color
    public bool useGlow = true;

    // How strong the glow is
    public float glowIntensity = 2f;


    [Header("Events")]

    // Runs when the correct item is dropped
    // Calls SuperCityManager.OnHardwarePlaced()
    public UnityEvent onCorrectDrop;

    // Runs when the wrong item is dropped
    public UnityEvent onIncorrectDrop;


    // Stores all Renderer components of this object and its children
    private Renderer[] renderers;

    // Stores the actual Material instances from those renderers
    private Material[] materials;

    // Stores the original color of each material
    private Color[] originalColors;


    // Runs once when the object first becomes active
    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();

        materials = new Material[renderers.Length];

        originalColors = new Color[renderers.Length];

        // Loop through every renderer found
        for (int i = 0; i < renderers.Length; i++)
        {
            // Get the material
            materials[i] = renderers[i].material;

            if (materials[i].HasProperty("_Color"))
            {

                // Used later in ClearFeedback to restore the target
                originalColors[i] = materials[i].color;
            }
            else
            {
                // Save normalColor as a fallback
                originalColors[i] = normalColor;
            }
        }
    }

    // ZDraggableItem script calls this when the correct item is dropped on target 
    public void TriggerSuccess()
    {
        Debug.Log("Correct drop triggered on " + gameObject.name);

        SetColor(correctColor);

        // Run anything assigned to onCorrectDrop: SuperCityManager.OnHardwarePlaced()
        // move to next phase
        onCorrectDrop.Invoke();

        // Wait 1 second, then call ClearFeedback
        Invoke(nameof(ClearFeedback), 1.0f);
    }


    // Called when the wrong draggable item is dropped on target
    public void TriggerFailure()
    {
        Debug.Log("Incorrect drop triggered on " + gameObject.name);

        SetColor(incorrectColor);

        if (isAnalogyDropTarget)
        {
            SuperCityManager manager = FindObjectOfType<SuperCityManager>();

            if (manager != null)
            {
                manager.OnAnalogyWrongGuess();
            }
        }

        // Run anything assigned to onIncorrectDrop in the Inspector
        onIncorrectDrop.Invoke();

        // Wait 0.5 seconds, then reset the color/glow
        Invoke(nameof(ClearFeedback), 0.5f);
    }


    // Resets the target's original color and turns off emission glow
    public void ClearFeedback()
    {
        // Loop through every stored material
        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] == null)
            {
                continue;
            }

            // Restore the original color saved in Start()
            if (materials[i].HasProperty("_Color"))
            {
                materials[i].color = originalColors[i];
            }

            if (materials[i].HasProperty("_EmissionColor"))
            {
                // Set emission color to black for no visible glow
                materials[i].SetColor("_EmissionColor", Color.black);

                materials[i].DisableKeyword("_EMISSION");
            }
        }
    }


    // Changes the target's color and optional glow
    // Used by TriggerSuccess() and TriggerFailure()
    private void SetColor(Color color)
    {
        // Loop through every stored material
        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] == null)
            {
                continue;
            }

            // Set to feedback color
            if (materials[i].HasProperty("_Color"))
            {
                materials[i].color = color;
            }

            // Make it glow using the same color
            if (useGlow && materials[i].HasProperty("_EmissionColor"))
            {
                materials[i].EnableKeyword("_EMISSION");

                // Set the emission color
                materials[i].SetColor("_EmissionColor", color * glowIntensity);
            }
        }
    }
}