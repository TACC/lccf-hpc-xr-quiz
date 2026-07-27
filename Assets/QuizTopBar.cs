using TMPro;
using UnityEngine;

public class QuizTopBar : MonoBehaviour
{
    [Header("Quiz")]
    public SuperCityManager superCityManager;

    [Header("Home Screen")]
    public GameObject nameIntroPanel;
    public TMP_InputField nameInputField;

    [Header("Mute Button")]
    public TMP_Text muteButtonText;

    private bool muted;

    private void Start()
    {
        muted = AudioListener.pause;
        UpdateMuteText();
    }

    public void ToggleMute()
    {
        muted = !muted;

        AudioListener.pause = muted;

        UpdateMuteText();
    }

    public void ReplayScene()
    {
        if (superCityManager != null)
        {
            superCityManager.ReplayCurrentScene();
        }
    }

    public void GoHome()
    {
        AudioListener.pause = false;
        muted = false;

        UpdateMuteText();

        if (superCityManager != null)
        {
            superCityManager.ReturnToHome();
        }

        if (nameIntroPanel != null)
        {
            nameIntroPanel.SetActive(true);
        }

        if (nameInputField != null)
        {
            nameInputField.text = "";
            nameInputField.Select();
            nameInputField.ActivateInputField();
        }
    }

    private void UpdateMuteText()
    {
        if (muteButtonText != null)
        {
            muteButtonText.text = muted ? "🔊Unmute" : "🔊Mute";
        }
    }
}