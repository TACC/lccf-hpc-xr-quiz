using TMPro;
using UnityEngine;

public class StudyModeToggle : MonoBehaviour
{
    public UserStudyService userStudy;
    public GameObject studyModeIndicator;
    public TMP_Text studyModeLabel;

    public KeyCode toggleKey = KeyCode.S;
    public bool requireControl = true;
    public bool requireShift = true;

    private void Start()
    {
        Refresh();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey) && ModifiersHeld())
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        if (userStudy == null)
        {
            Debug.LogError("[UserStudy] StudyModeToggle has no UserStudyService assigned.", this);
            return;
        }

        userStudy.ToggleStudyRun();
        Refresh();
    }

    private bool ModifiersHeld()
    {
        if (requireControl &&
            !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
        {
            return false;
        }

        if (requireShift &&
            !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
        {
            return false;
        }

        return true;
    }

    private void Refresh()
    {
        bool on = userStudy != null && userStudy.IsStudyRun;

        if (studyModeIndicator != null)
        {
            studyModeIndicator.SetActive(on);
        }

        if (studyModeLabel != null)
        {
            studyModeLabel.text = on ? "STUDY MODE  " + userStudy.ParticipantId : "";
        }
    }
}
