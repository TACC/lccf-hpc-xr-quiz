using TMPro;
using UnityEngine;

public class PlayerNameIntro : MonoBehaviour
{
    [Header("Intro UI")]
    public TMP_InputField nameInputField;
    public GameObject introPanel;

    [Header("Quiz")]
    public SuperCityManager superCityManager;

    public static string PlayerName { get; private set; } = "Player";

    public void BeginQuiz()
    {
        string enteredName = nameInputField.text.Trim();

        if (string.IsNullOrEmpty(enteredName))
        {
            nameInputField.placeholder
                .GetComponent<TMP_Text>().text = "Please enter your name";

            return;
        }

        PlayerName = enteredName;

        Debug.Log("Player name saved: " + PlayerName);

        introPanel.SetActive(false);

        if (superCityManager != null)
        {
            superCityManager.BeginQuiz();
        }
    }
}