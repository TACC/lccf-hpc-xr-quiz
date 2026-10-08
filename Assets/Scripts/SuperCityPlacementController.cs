using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperCityPlacementController : MonoBehaviour
{
    [Header("Placement Layer")]
    // The parent object that holds the placement scene
    public GameObject placementLayer;
    // Stores the placement scene for each phase
    public GameObject[] placementGroups;

    private Vector3[] originalPlacementGroupPositions;

    // Used so scene cannot move on until placement audio is finished
    private bool hardwarePlacementCompleted = false;

    [SerializeField] private SuperCityManager manager;
    [SerializeField] private SuperCityAudioController audioController;
    [SerializeField] private SuperCityTransitionController transitionController;
    [SerializeField] private SuperCityAnimationController animationController;
    [SerializeField] private SuperCityAnalogyController analogyController;
    [SerializeField] private SuperCityFinalSceneController finalSceneController;
    [SerializeField] private SuperCitySceneStateController sceneStateController;

    public void Initialize()
    {
        originalPlacementGroupPositions = new Vector3[placementGroups.Length];
        for (int i = 0; i < placementGroups.Length; i++)
        {
            if (placementGroups[i] != null)
            {
                originalPlacementGroupPositions[i] = placementGroups[i].transform.position;
            }
        }
    }

     // Starts the placement layer for the current phase
    public IEnumerator StartPlacementLayer()
    {
        if (placementLayer != null)
        {
            placementLayer.SetActive(true);
        }

        HideAllPlacementGroups();

        GameObject currentPlacementGroup = null;

        if (placementGroups != null &&
            manager.currentPhase >= 0 &&
            manager.currentPhase < placementGroups.Length &&
            placementGroups[manager.currentPhase] != null)
        {
            currentPlacementGroup = placementGroups[manager.currentPhase];
        }

        if (currentPlacementGroup == null)
        {
            Debug.LogWarning("No placement group assigned for phase " + manager.currentPhase);
            yield break;
        }

        Vector3 finalPosition = originalPlacementGroupPositions[manager.currentPhase];
        Vector3 startPosition = finalPosition + new Vector3(0f, 0f, transitionController.placementZTransitionDistance);

        currentPlacementGroup.transform.position = startPosition;
        currentPlacementGroup.SetActive(true);
        animationController.RestartAnimatorsUnder(currentPlacementGroup);

        yield return StartCoroutine(transitionController.SlideObject(
            currentPlacementGroup,
            startPosition,
            finalPosition,
            transitionController.placementSlideDuration
        ));

        ShowAllPlacementTargetGlows(currentPlacementGroup);

        if (manager.StudyData != null)
        {
            manager.StudyData.BeginPlacementPhase(manager.currentPhase);
        }

        Debug.Log("Showing placement group for phase " + manager.currentPhase);

        hardwarePlacementCompleted = false;

        // The city-to-placement transition is done now
        // Allow hardware placement to be detected
        manager.PhaseTransitionRunning = false;

        // Start placement audio
        StartCoroutine(audioController.PlayPlacementAudioForCurrentPhase());

        Debug.Log("Waiting for user to correctly place the hardware.");

    }

    private IEnumerator HardwarePlacedRoutine()
    {
        if (manager.PhaseTransitionRunning)
        {
            yield break;
        }
        
        manager.PhaseTransitionRunning = true;

        Debug.Log("Hardware placed correctly or timer finished for phase " + manager.currentPhase);

        if (manager.currentPhase == finalSceneController.FinalPlacementPhaseIndex)
        {
            yield return StartCoroutine(finalSceneController.ShowFinalSceneRoutine());
            yield break;
        }

        GameObject currentPlacementGroup = null;

        if (placementGroups != null &&
            manager.currentPhase >= 0 &&
            manager.currentPhase < placementGroups.Length)
        {
            currentPlacementGroup = placementGroups[manager.currentPhase];
        }

        // Slide the placement group upward out of view
        if (currentPlacementGroup != null && currentPlacementGroup.activeSelf)
        {
            Vector3 startPosition = currentPlacementGroup.transform.position;
            Vector3 endPosition = startPosition + new Vector3(0f, 0f, -transitionController.placementZTransitionDistance);

            yield return StartCoroutine(
                transitionController.SlideObject(
                currentPlacementGroup,
                startPosition,
                endPosition,
                transitionController.placementSlideDuration));

            currentPlacementGroup.SetActive(false);
        }

        HideAllPlacementGroups();

        if (placementLayer != null)
        {
            placementLayer.SetActive(false);
        }

        manager.currentPhase++;

        if (manager.currentPhase < analogyController.NumberOfPhases)
        {
            yield return StartCoroutine(
                analogyController.StartAnalogyPhaseWithSlide(manager.currentPhase));
        }
        else
        {
            Debug.Log("All phases completed.");
        }

        manager.PhaseTransitionRunning = false;
    }

    // Hides every placement group in the placementGroups array
    public void HideAllPlacementGroups()
    {
        if (placementGroups == null)
        {
            return;
        }

        foreach (GameObject group in placementGroups)
        {
            if (group != null)
            {
                group.SetActive(false);
            }
        }
    }

    public void ResetCurrentPlacementForReplay()
    {
        if (placementGroups == null ||
            manager.currentPhase < 0 ||
            manager.currentPhase >= placementGroups.Length ||
            placementGroups[manager.currentPhase] == null)
        {
            return;
        }

        sceneStateController.RestoreCurrentPlacementTransforms(placementGroups[manager.currentPhase]);

        if (originalPlacementGroupPositions != null &&
            manager.currentPhase < originalPlacementGroupPositions.Length)
        {
            placementGroups[manager.currentPhase].transform.position =
                originalPlacementGroupPositions[manager.currentPhase];
        }

        animationController.ResetAnimatorsUnder(placementGroups[manager.currentPhase]);
        ResetPlacementDraggables(placementGroups[manager.currentPhase]);
        ResetPlacementTargets(placementGroups[manager.currentPhase]);
        ShowAllPlacementTargetGlows(placementGroups[manager.currentPhase]);
    }

    private void ResetPlacementDraggables(GameObject placementGroup)
    {
        if (placementGroup == null)
        {
            return;
        }

        PlacementDraggableItem[] draggableItems =
            placementGroup.GetComponentsInChildren<PlacementDraggableItem>(true);

        foreach (PlacementDraggableItem item in draggableItems)
        {
            if (item != null)
            {
                item.ResetForReplay();
            }
        }
    }

    public void ResetAllPlacementDraggables()
    {
        if (placementGroups == null)
        {
            return;
        }

        foreach (GameObject placementGroup in placementGroups)
        {
            if (placementGroup == null)
            {
                continue;
            }

            PlacementDraggableItem[] draggableItems =
                placementGroup.GetComponentsInChildren<PlacementDraggableItem>(true);

            foreach (PlacementDraggableItem item in draggableItems)
            {
                if (item != null)
                {
                    item.ResetForReplay();
                }
            }
        }
    }

    private void ShowAllPlacementTargetGlows(GameObject placementGroup)
    {
        if (placementGroup == null)
        {
            return;
        }

        PlacementDropTarget[] targets =
            placementGroup.GetComponentsInChildren<PlacementDropTarget>(true);

        foreach (PlacementDropTarget target in targets)
        {
            if (target != null)
            {
                target.ShowGlow();
            }
        }
    }

    private void ResetPlacementTargets(GameObject placementGroup)
    {
        if (placementGroup == null)
        {
            return;
        }

        PlacementDropTarget[] targets =
            placementGroup.GetComponentsInChildren<PlacementDropTarget>(true);

        foreach (PlacementDropTarget target in targets)
        {
            if (target != null)
            {
                target.ResetTarget();
            }
        }
    }

    public void ResetAllPlacementTargets()
    {
        if (placementGroups == null)
        {
            return;
        }

        foreach (GameObject placementGroup in placementGroups)
        {
            if (placementGroup != null)
            {
                ResetPlacementTargets(placementGroup);
            }
        }
    }

    public void OnHardwarePlaced()
    {
        hardwarePlacementCompleted = true;

        if (!audioController.PlacementAudioFinished)
        {
            Debug.Log("Hardware placed, but waiting for placement audio to finish.");
            return;
        }

        Debug.Log("Hardware placed and audio already finished. Moving on.");

        StartCoroutine(HardwarePlacedRoutine());
    }

    public void OnPlacementAudioFinished()
    {
        if (hardwarePlacementCompleted)
        {
            StartCoroutine(HardwarePlacedRoutine());
        }
    }

    public void RestoreOriginalPlacementPositions()
    {
        if (placementGroups == null || originalPlacementGroupPositions == null)
        {
            return;
        }

        for (int i = 0; i < placementGroups.Length; i++)
        {
            if (placementGroups[i] != null && i < originalPlacementGroupPositions.Length)
            {
                placementGroups[i].transform.position = originalPlacementGroupPositions[i];
            }
        }
    }

    public void ResetPlacementState()
    {
        hardwarePlacementCompleted = false;
    }
}