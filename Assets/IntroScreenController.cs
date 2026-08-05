using UnityEngine;

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
        beginScene.SetActive(false);
        beginTwo.SetActive(false);
        finalScene.SetActive(false);
        topBar.SetActive(true);

        StopScreenAudio();

        if (superCityManager != null)
        {
            superCityManager.BeginQuiz();
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