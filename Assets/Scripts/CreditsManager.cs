using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro; // Remove this line and swap TextMeshProUGUI -> Text below if the project uses legacy Unity UI instead

/// Displays the end-of-experience credits panel: contributor list, audio/asset attributions.
/// Triggered from SuperCityManager.ShowFinalSceneRoutine(), after introScreenController.ShowFinalScene()
public class CreditsManager : MonoBehaviour
{
    [System.Serializable]
    public class Contributor
    {
        public string name;
        public string role;
        public string link; // portfolio or LinkedIn URL, optional
    }

    [Header("UI References")]
    [Tooltip("Parent panel that holds the entire credits screen. Should start inactive.")]
    public GameObject creditsPanel;

    [Tooltip("Text element where contributor lines get written. Uses 3D TextMeshPro (world-space), matching Score/Restored/etc.")]
    public TextMeshPro creditsText;

    [Tooltip("Text element for the audio/asset attribution block at the bottom. Uses 3D TextMeshPro, same as creditsText.")]
    public TextMeshPro attributionText;

    [Header("Scroll Settings")]
    [Tooltip("If true, credits scroll upward automatically. If false, it's a static list the user dismisses.")]
    public bool autoScroll = true;
    public float scrollSpeed = 0.05f;
    public Transform scrollContent;

    [Tooltip("Seconds to hold still after credits appear before scrolling starts.")]
    public float delayBeforeScrollStarts = 3f;

    [Tooltip("How far (in local Y units) the content should scroll before stopping. Measure the distance from the top of your header text to the bottom of your 'Thank you!' text in the Scene view, then set this a bit larger so the last line clears the visible area before stopping.")]
    public float scrollDistance = 8f;

    private bool canScroll = false;
    private float scrollStartY;

    [Header("Contributors")]
    public List<Contributor> contributors = new List<Contributor>();

    [Header("Attributions")]
    [TextArea(3, 6)]
    public string audioAttribution =
        "Music by Eric Matyas\nwww.soundimage.org";

    [TextArea(3, 6)]
    public string assetAttribution =
        "Low poly sci-fi cyberpunk city by JustCreate (Licensed)\n" +
        "Low poly sci-fi cyberpunk interior by JustCreate (Licensed)";

    [Header("Events")]
    public UnityEvent onCreditsShown;
    public UnityEvent onCreditsDismissed;

    private bool isShowing = false;

    void Awake()
    {
        if (creditsPanel != null)
            creditsPanel.SetActive(false);

        // Populate default contributor list from the handoff doc if none were set in the Inspector
        if (contributors.Count == 0)
        {
            PopulateDefaultContributors();
        }
    }

    void Update()
    {
        if (isShowing && autoScroll && canScroll && scrollContent != null)
        {
            scrollContent.localPosition += Vector3.up * scrollSpeed * Time.deltaTime;

            if (scrollContent.localPosition.y >= scrollStartY + scrollDistance)
            {
                canScroll = false; // reached the end, stop and hold on the last line
            }
        }
    }

    [Header("Auto-trigger")]
    [Tooltip("How long to hold on the 'SuperCity Restored' screen before credits appear automatically.")]
    public float delayAfterFinalScene = 4f;

    [Header("Hide While Showing Credits")]
    [Tooltip("Drag in Score, Supercity, Restored, outstanding work, play again, and the 6 component icons/labels here. These get hidden when credits show, and restored when credits are dismissed.")]
    public List<GameObject> objectsToHideWhileShowingCredits = new List<GameObject>();

    /// Call this from SuperCityManager.ShowFinalSceneRoutine(), right after
    /// introScreenController.ShowFinalScene() runs. Waits, then shows credits automatically.
    public void ShowCreditsAfterDelay()
    {
        StartCoroutine(ShowCreditsAfterDelayRoutine());
    }

    private IEnumerator ShowCreditsAfterDelayRoutine()
    {
        yield return new WaitForSeconds(delayAfterFinalScene);
        ShowCredits();
    }

    private IEnumerator EnableScrollAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeScrollStarts);
        canScroll = true;
    }

    /// Call this from SuperCityManager once the completion screen has run its course
    /// (e.g. after a short delay, or on a "Continue" button press on the restored screen).
    [ContextMenu("TEST: Show Credits Now")]
    public void ShowCredits()
    {
        if (creditsPanel == null) return;

        BuildCreditsText();
        creditsPanel.SetActive(true);
        isShowing = true;
        canScroll = false;

        if (scrollContent != null)
        {
            scrollContent.localPosition = new Vector3(scrollContent.localPosition.x, 0f, scrollContent.localPosition.z);
            scrollStartY = scrollContent.localPosition.y;
        }

        if (autoScroll)
            StartCoroutine(EnableScrollAfterDelay());

        foreach (var obj in objectsToHideWhileShowingCredits)
        {
            if (obj != null) obj.SetActive(false);
        }

        onCreditsShown?.Invoke();
    }

    /// Hook this up to a ZDraggableItem/ZPointer click on a "skip"/"close" button,
    /// or call it automatically once scrolling finishes.
    [ContextMenu("TEST: Dismiss Credits Now")]
    public void DismissCredits()
    {
        if (creditsPanel == null) return;

        creditsPanel.SetActive(false);
        isShowing = false;

        foreach (var obj in objectsToHideWhileShowingCredits)
        {
            if (obj != null) obj.SetActive(true);
        }

        onCreditsDismissed?.Invoke();
    }

    private void BuildCreditsText()
    {
        if (creditsText != null)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var c in contributors)
            {
                sb.AppendLine(c.name);
                sb.AppendLine($"<size=70%><color=#AAAAAA>{c.role}</color></size>");
                sb.AppendLine();
            }
            creditsText.text = sb.ToString();
        }

        if (attributionText != null)
        {
            attributionText.text = $"Music\n{audioAttribution}\n\nModels\n{assetAttribution}";
        }
    }

    private void PopulateDefaultContributors()
    {
        contributors.AddRange(new List<Contributor>
        {
            new Contributor { name = "Andrew Solis", role = "Principal Investigator" },
            new Contributor { name = "MJ Johns", role = "Senior UX Researcher" },
            new Contributor { name = "Sanika Goyal", role = "Experience Design Lead" },
            new Contributor { name = "Ayon Das", role = "Software Engineer" },
            new Contributor { name = "Jo Wozniak", role = "RESA IV" },
            new Contributor { name = "Karen Heckel", role = "Software Engineer" },
            new Contributor { name = "Imelda Ishiekwene", role = "UX Designer" },
            new Contributor { name = "Gloria Jang", role = "Junior Visual / Experience Designer" },
            new Contributor { name = "Pascal R Garcia", role = "Contributor" },
            new Contributor { name = "Tyler Henry", role = "Contributor" },
            new Contributor { name = "Dawn Hunter", role = "Contributor" },
            new Contributor { name = "Meagan Galvan", role = "Software Engineer" },
            new Contributor { name = "Sara Sandoval", role = "Software Engineer" },
            new Contributor { name = "Risha Vankalapati", role = "Software Engineer" },
            new Contributor { name = "Adenifemi Soyemi", role = "Software Engineer" },
            new Contributor { name = "Jashanpreet Kaur", role = "Design Lead: UX Design & Research" },
            new Contributor { name = "Niyati Naveen Nair", role = "UX Researcher & Designer" },
            new Contributor { name = "Sheena Vaghela", role = "UX Researcher & Designer" },
            new Contributor { name = "Zoe Yu", role = "Contributor" },
            new Contributor { name = "Ruchika Sanghi", role = "UX Designer" },
        });
    }
}