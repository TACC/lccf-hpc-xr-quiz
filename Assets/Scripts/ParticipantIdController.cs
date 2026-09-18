using TMPro;
using UnityEngine;

public class ParticipantIdController : MonoBehaviour
{
    [SerializeField] private TMP_InputField participantIdInput;
    [SerializeField] private TMP_Text errorText;
    [SerializeField] private UserStudyDataManager userStudyDataManager;
    [SerializeField] private IntroScreenController introScreenController;

    public void SubmitParticipantId()
    {
        if (participantIdInput == null || userStudyDataManager == null ||
            introScreenController == null)
        {
            Debug.LogError("Participant ID controller is missing references.");
            return;
        }
        bool accepted = userStudyDataManager.SetParticipantId(participantIdInput.text);

        if (!accepted)
        {
            if (errorText != null)
            {
                errorText.text = "Enter a valid participant ID using letters, numbers, - or _.";
            }

            return;
        }
        if (errorText != null)
        {
            errorText.text = "";
        }
        introScreenController.ShowSecondIntro();
    }
}
