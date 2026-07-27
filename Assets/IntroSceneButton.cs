using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroSceneButton : MonoBehaviour
{
    public string nextSceneName;

    public void GoToNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}