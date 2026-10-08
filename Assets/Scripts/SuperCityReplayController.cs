using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperCityReplayController : MonoBehaviour
{
    [SerializeField] private SuperCityManager manager;
    [SerializeField] private SuperCityAnalogyController analogyController;
    [SerializeField] private SuperCityPlacementController placementController;
    [SerializeField] private SuperCityAudioController audioController;
    [SerializeField] private SuperCitySceneStateController sceneStateController;
    [SerializeField] private SuperCityAnimationController animationController;
    [SerializeField] private SuperCityFinalSceneController finalSceneController;
    [SerializeField] private IntroScreenController introScreenController;

    public void ReplayCurrentScene()
    {
        if (introScreenController != null &&
            introScreenController.IsZCanvasScreenShowing())
        {
            return;
        }

        bool placementIsShowing = placementController.placementLayer != null &&
            placementController.placementLayer.activeSelf;

        if (!placementIsShowing && manager.StudyData != null)
        {
            manager.StudyData.RegisterAnalogyRepeat(manager.currentPhase);
        }

        StopAllCoroutines();

        audioController.StopAudio();

        manager.PhaseTransitionRunning = false;

        if (placementIsShowing)
        {
            ReplayCurrentPlacement();
        }
        else
        {
            ReplayCurrentAnalogy();
        }

    }

    private void ReplayCurrentAnalogy()
    {
        analogyController.HideAllCityAnalogies();
        placementController.HideAllPlacementGroups();

        animationController.ResetAllCustomAnimations();
        sceneStateController.RestoreAllAnswerChoices();

        if (placementController.placementLayer != null)
        {
            placementController.placementLayer.SetActive(false);
        }

        if (analogyController.cityLayer != null)
        {
            analogyController.cityLayer.SetActive(true);
        }

        manager.PhaseTransitionRunning = false;

        analogyController.ResetAnalogyState();

        analogyController.StartAnalogyPhase(manager.currentPhase);

        analogyController.ResetCurrentAnalogyChoices();

        StartCoroutine(audioController.PlayAnalogyAudioForCurrentPhase());

        Debug.Log("Replaying analogy phase " + manager.currentPhase);
    }

    private void ReplayCurrentPlacement()
    {
        analogyController.HideAllCityAnalogies();
        animationController.ResetAllCustomAnimations();
        placementController.ResetCurrentPlacementForReplay();
        placementController.HideAllPlacementGroups();

        if (analogyController.cityLayer != null)
        {
            analogyController.cityLayer.SetActive(false);
        }
        if (placementController.placementLayer != null)
        {
            placementController.placementLayer.SetActive(true);
        }

        placementController.ResetPlacementState();
        manager.PhaseTransitionRunning = false;

        StartCoroutine(placementController.StartPlacementLayer());

        Debug.Log("Replaying placement phase " + manager.currentPhase);
    }

    public void ResetEntireQuiz()
    {
        StopAllCoroutines();

        audioController.StopAudio();

        manager.ResetGameState();

        analogyController.ResetAnalogyState();
        placementController.ResetPlacementState();

        sceneStateController.RestoreSceneTransformsAndAnimators();
        animationController.ResetAllCustomAnimations();
        sceneStateController.RestoreAllAnswerChoices();

        placementController.ResetAllPlacementDraggables();
        placementController.ResetAllPlacementTargets();
        analogyController.RestoreOriginalAnalogyPositions();
        placementController.RestoreOriginalPlacementPositions();
        placementController.HideAllPlacementGroups();

        if (analogyController.repairedCityModel != null)
        {
            analogyController.repairedCityModel.SetActive(false);
        }

        if (analogyController.brokenPlaneObject != null)
        {
            analogyController.brokenPlaneObject.SetActive(true);
        }

        finalSceneController.HideFinalCelebrationObjects();
    }
}
