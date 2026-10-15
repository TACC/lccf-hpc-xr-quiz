using UnityEngine;
using UnityEngine.UI;

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

    [Header("User Study")]
    [SerializeField] private UserStudyService userStudyService;
    [SerializeField] private Toggle studyParticipantToggle;

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

        if (studyParticipantToggle != null && userStudyService != null)
        {
            studyParticipantToggle.SetIsOnWithoutNotify(userStudyService.IsStudyRun);
        }

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
        if (superCityManager == null || userStudyService == null)
        {
            Debug.LogError("SuperCityManager or UserStudyService is not assigned.");
            return;
        }

        if (studyParticipantToggle != null)
        {
            userStudyService.SetStudyRun(studyParticipantToggle.isOn);
        }

        beginScene.SetActive(false);
        beginTwo.SetActive(false);
        finalScene.SetActive(false);
        topBar.SetActive(true);

        StopScreenAudio();

        superCityManager.BeginQuiz();
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