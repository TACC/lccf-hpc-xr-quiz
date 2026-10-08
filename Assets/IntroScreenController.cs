using UnityEngine;
using TMPro;

public class IntroScreenController : MonoBehaviour
{
    public GameObject beginScene;
    public GameObject beginTwo;
    public GameObject finalScene;
    public GameObject topBar;
    public SuperCityManager superCityManager;

    public AudioSource screenAudioSource;
    public AudioClip firstBeginAudio;
    public AudioClip secondBeginAudio;
    public AudioClip finalSceneAudio;

    [SerializeField] private TMP_InputField participantIdInput;
    [SerializeField] private UserStudyDataManager userStudyDataManager;

    void Start()
    {
        ShowFirstIntro();
    }

    public void ShowFirstIntro()
    {
        beginScene.SetActive(true);
        beginTwo.SetActive(false);
        finalScene.SetActive(false);
        topBar.SetActive(true);

        PlayScreenAudio(firstBeginAudio);
    }

    public void ShowSecondIntro()
    {
        beginScene.SetActive(false);
        beginTwo.SetActive(true);
        finalScene.SetActive(false);
        topBar.SetActive(true);

        PlayScreenAudio(secondBeginAudio);
    }

    public void StartQuiz()
    {
        if (participantIdInput == null)
        {
            Debug.LogError("Participant ID input field is not assigned.");
            return;
        }

        string participantId = participantIdInput.text.Trim();

        if (string.IsNullOrEmpty(participantId))
        {
            Debug.LogError("Plase enter a participant ID.");
            return; 
        }

        if (userStudyDataManager == null)
        {
            Debug.LogError("UserStudyDataManager is not assigned.");
            return;
        }

        if (!userStudyDataManager.SetParticipantId(participantId))
        {
            Debug.LogError("Invalid participant ID. Use only letters, numbers, hyphens, or underscores.");
            return;
        }

        beginScene.SetActive(false);
        beginTwo.SetActive(false);
        finalScene.SetActive(false);
        topBar.SetActive(true);

        StopScreenAudio();

        if (superCityManager != null)
        {
            superCityManager.BeginQuiz();
        }
        else 
        {
            Debug.LogError("SuperCityManager is not assigned.");
        }
    }

    public void ShowFinalScene()
    {
        beginScene.SetActive(false);
        beginTwo.SetActive(false);
        finalScene.SetActive(true);
        topBar.SetActive(true);

        PlayScreenAudio(finalSceneAudio);
    }

    public bool IsZCanvasScreenShowing()
    {
        return
            (beginScene != null && beginScene.activeSelf) ||
            (beginTwo != null && beginTwo.activeSelf) ||
            (finalScene != null && finalScene.activeSelf);
    }

    public void ReturnToHome()
    {
        beginScene.SetActive(false);
        beginTwo.SetActive(false);
        finalScene.SetActive(false);

        StopScreenAudio();

        if (superCityManager != null)
        {
            superCityManager.ReturnToHome();
        }

        ShowFirstIntro();
    }

    private void PlayScreenAudio(AudioClip clip)
    {
        if (screenAudioSource == null)
        {
            return;
        }

        screenAudioSource.Stop();
        screenAudioSource.clip = clip;

        if (clip != null)
        {
            screenAudioSource.Play();
        }
    }

    private void StopScreenAudio()
    {
        if (screenAudioSource != null)
        {
            screenAudioSource.Stop();
            screenAudioSource.clip = null;
        }
    }
}