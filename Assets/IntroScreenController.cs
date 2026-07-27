using UnityEngine;

public class IntroScreenController : MonoBehaviour
{
    public GameObject beginScene;
    public GameObject beginTwo;
    public GameObject topBar;
    public SuperCityManager superCityManager;

    void Start()
    {
        beginScene.SetActive(true);
        beginTwo.SetActive(false);
        topBar.SetActive(true);
    }

    public void ShowSecondIntro()
    {
        beginScene.SetActive(false);
        beginTwo.SetActive(true);
        topBar.SetActive(true);
    }

    public void StartQuiz()
    {
        beginScene.SetActive(false);
        beginTwo.SetActive(false);
        topBar.SetActive(true);

        superCityManager.BeginQuiz();
    }
}