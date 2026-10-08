using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SuperCityFinalSceneController : MonoBehaviour
{
    public IntroScreenController introScreenController;

    [Header("Final Celebration")]
    public CreditsManager creditsManager;
    public int finalPlacementPhaseIndex = 5;
    public GameObject[] finalAnalogyObjects;
    public TMP_Text finalScoreText;

    [SerializeField] private SuperCityManager manager;
    [SerializeField] private SuperCityPlacementController placementController;
    [SerializeField] private SuperCityAnalogyController analogyController;
    [SerializeField] private SuperCityTransitionController transitionController;
    [SerializeField] private SuperCityAudioController audioController;

    public void HideFinalCelebrationObjects()
    {
        if (finalScoreText != null)
        {
            finalScoreText.gameObject.SetActive(false);
        }
    }

    public IEnumerator ShowFinalSceneRoutine()
    {
        GameObject currentPlacementGroup = null;

        if (placementController.placementGroups != null &&
            manager.currentPhase >= 0 &&
            manager.currentPhase < placementController.placementGroups.Length)
        {
            currentPlacementGroup = placementController.placementGroups[manager.currentPhase];
        }

        if (currentPlacementGroup != null && currentPlacementGroup.activeSelf)
        {
            Vector3 startPosition = currentPlacementGroup.transform.position;
            Vector3 endPosition =
                startPosition + new Vector3(0f, 0f, -transitionController.placementZTransitionDistance);

            yield return StartCoroutine(transitionController.SlideObject(
                currentPlacementGroup,
                startPosition,
                endPosition,
                transitionController.placementSlideDuration
            ));

            currentPlacementGroup.SetActive(false);
        }

        analogyController.HideAllCityAnalogies();
        placementController.HideAllPlacementGroups();

        if (analogyController.cityLayer != null)
        {
            analogyController.cityLayer.SetActive(false);
        }

        if (placementController.placementLayer != null)
        {
            placementController.placementLayer.SetActive(false);
        }

        audioController.StopAudio();

        manager.PhaseTransitionRunning = false;

        if (finalAnalogyObjects != null)
        {
            foreach (GameObject obj in finalAnalogyObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
        }

        if (finalScoreText != null)
        {
            finalScoreText.text =
                manager.AnalogyCorrectFirstTryScore +
                " / " +
                analogyController.NumberOfPhases + " COMPONENTS";

            finalScoreText.gameObject.SetActive(true);
        }

        if (manager.StudyData != null)
        {
            bool saved = manager.StudyData.CompleteSession();
            if (!saved)
            {
                Debug.LogError("Completed user study session could not be saved.");
            }
        }

        if (introScreenController != null)
        {
            introScreenController.ShowFinalScene();
        }
        else
        {
            Debug.LogError("IntroScreenController is not assigned on SuperCityManager.");
        }

        if (creditsManager != null)
        {
            creditsManager.ShowCreditsAfterDelay();
        }
    }

    public int FinalPlacementPhaseIndex
    {
        get { return finalPlacementPhaseIndex; }
    }
}
