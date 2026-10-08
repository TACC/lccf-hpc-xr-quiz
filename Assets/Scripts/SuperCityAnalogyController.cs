using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperCityAnalogyController : MonoBehaviour
{
    // Array stores each city analogy phase
    public GameObject[] cityAnalogies;
    public GameObject cityLayer;

    [Header("Book Orbit")]
    public BookOrbitGroup[] bookOrbitGroups;
    public float bookReturnDuration = 1.2f;

    // Connects to BrokenCityPieces script
    public BrokenCityPieces brokenCityPieces;

    // First phase uses the broken city repair animation
    public int brokenCityPhaseIndex = 0;

    // Full model appears after repairs
    public GameObject repairedCityModel;

    // Parent object holding the broken city version
    public GameObject brokenPlaneObject;

    public float holdAfterRepairDuration = 3f;

    private bool analogyCorrectVisualsStarted = false;
    private bool analogyVisualsFinished = false;
    private bool analogySolved = false;

    [SerializeField] private SuperCityManager manager;
    [SerializeField] private SuperCityPlacementController placementController;
    [SerializeField] private SuperCityAudioController audioController;
    [SerializeField] private SuperCityTransitionController transitionController;

    private Vector3[] originalCityAnalogyPositions;

    public void Initialize()
    {
        originalCityAnalogyPositions = new Vector3[cityAnalogies.Length];

        for (int i = 0; i < cityAnalogies.Length; i++)
        {
            if (cityAnalogies[i] != null)
            {
                originalCityAnalogyPositions[i] = cityAnalogies[i].transform.position;
            }
        }
    }

    // Starts a specific analogy phase
    public void StartAnalogyPhase(int phaseIndex)
    {
        if (cityAnalogies == null || phaseIndex < 0 || phaseIndex >= cityAnalogies.Length)
        {
            Debug.LogWarning("Invalid city analogy phase: " + phaseIndex);
            return;
        }

        manager.currentPhase = phaseIndex;

        manager.PhaseTransitionRunning = false;

        if (cityLayer != null)
        {
            cityLayer.SetActive(true);
        }
        if (placementController.placementLayer != null)
        {
            placementController.placementLayer.SetActive(false);
        }

        HideAllCityAnalogies();
        placementController.HideAllPlacementGroups();

        if (repairedCityModel != null)
        {
            repairedCityModel.SetActive(false);
        }
        if (brokenPlaneObject != null)
        {
            brokenPlaneObject.SetActive(IsBrokenCityPhase());
        }

        GameObject currentAnalogy = cityAnalogies[manager.currentPhase];

        if (currentAnalogy != null)
        {
            currentAnalogy.transform.position = originalCityAnalogyPositions[manager.currentPhase];

            currentAnalogy.SetActive(true);

            if (IsBrokenCityPhase())
            {
                if (brokenPlaneObject != null)
                {
                    brokenPlaneObject.SetActive(true);
                }

                if (repairedCityModel != null)
                {
                    repairedCityModel.SetActive(false);
                }

                if (brokenCityPieces != null)
                {
                    brokenCityPieces.ResetCity();
                }
            }


            if (bookOrbitGroups != null &&
                manager.currentPhase >= 0 &&
                manager.currentPhase < bookOrbitGroups.Length &&
                bookOrbitGroups[manager.currentPhase] != null)
            {
                bookOrbitGroups[manager.currentPhase].RefreshBookOrbits();
                bookOrbitGroups[manager.currentPhase].SaveCurrentBookPositionsAsHome();
                bookOrbitGroups[manager.currentPhase].StartAllBookOrbits();
            }
        }

        if (manager.StudyData != null)
        {
            manager.StudyData.BeginAnalogyPhase(manager.currentPhase);
        }

        Debug.Log("Starting analogy phase " + manager.currentPhase);
    }

    // Starts a specific analogy phase with slide-in
    public IEnumerator StartAnalogyPhaseWithSlide(int phaseIndex)
    {
        if (cityAnalogies == null || phaseIndex < 0 || phaseIndex >= cityAnalogies.Length)
        {
            Debug.LogWarning("Invalid city analogy phase: " + phaseIndex);
            yield break;
        }

        manager.currentPhase = phaseIndex;

        if (cityLayer != null)
        {
            cityLayer.SetActive(true);
        }
        if (placementController.placementLayer != null)
        {
            placementController.placementLayer.SetActive(false);
        }

        HideAllCityAnalogies();
        placementController.HideAllPlacementGroups();

        if (repairedCityModel != null)
        {
            repairedCityModel.SetActive(false);
        }
        if (brokenPlaneObject != null)
        {
            brokenPlaneObject.SetActive(IsBrokenCityPhase());
        }

        GameObject currentAnalogy = cityAnalogies[manager.currentPhase];

        if (currentAnalogy == null)
        {
            yield break;
        }

        Vector3 finalPosition = originalCityAnalogyPositions[manager.currentPhase];
        Vector3 startPosition = finalPosition + 
            new Vector3(0f, 0f, transitionController.analogyZTransitionDistance);

        // Put the whole analogy below the screen first
        currentAnalogy.transform.position = startPosition;
        currentAnalogy.SetActive(true);

        ResetCurrentAnalogyChoices();

        // Books orbiting animation
        if (bookOrbitGroups != null &&
            manager.currentPhase >= 0 &&
            manager.currentPhase < bookOrbitGroups.Length &&
            bookOrbitGroups[manager.currentPhase] != null)
        {
            bookOrbitGroups[manager.currentPhase].RefreshBookOrbits();
            bookOrbitGroups[manager.currentPhase].SaveCurrentBookPositionsAsHome();
            bookOrbitGroups[manager.currentPhase].StartAllBookOrbits();
        }

        // Slide the whole analogy parent into place
        yield return StartCoroutine(
            transitionController.SlideObject(
            currentAnalogy,
            startPosition,
            finalPosition,
            transitionController.analogySlideDuration));

        manager.ResetAnalogyAttemptState();
        analogyCorrectVisualsStarted = false;
        analogyVisualsFinished = false;
        analogySolved = false;

        manager.PhaseTransitionRunning = false;

        if (manager.StudyData != null)
        {
            manager.StudyData.BeginAnalogyPhase(manager.currentPhase);
        }

        ResetCurrentAnalogyChoices();

        StartCoroutine(audioController.PlayAnalogyAudioForCurrentPhase());

        Debug.Log("Starting analogy phase " + manager.currentPhase);
    }

    public void TryStartAnalogyTransition()
    {
        if (!analogySolved)
        {
            return;
        }
        if (!audioController.AnalogyAudioFinished)
        {
            Debug.Log("Waiting for analogy audio to finish.");
            return;
        }
        if (!analogyVisualsFinished)
        {
            Debug.Log("Waiting for analogy visuals to finish.");
            return;
        }
        if (manager.PhaseTransitionRunning)
        {
            return;
        }

        StartCoroutine(AnalogySolvedRoutine());
    }

    private IEnumerator StartCorrectAnalogyVisualsImmediately()
    {
        if (analogyCorrectVisualsStarted)
        {
            yield break;
        }
        analogyCorrectVisualsStarted = true;
        analogyVisualsFinished = false;

        // Bookshelf animation
        if (bookOrbitGroups != null &&
            manager.currentPhase >= 0 &&
            manager.currentPhase < bookOrbitGroups.Length &&
            bookOrbitGroups[manager.currentPhase] != null)
        {
            bookOrbitGroups[manager.currentPhase].StopAllBookOrbits();

            yield return StartCoroutine(
                bookOrbitGroups[manager.currentPhase].SlideAllBooksBack(bookReturnDuration)
            );
        }

        // Broken city animation
        if (IsBrokenCityPhase())
        {

            if (brokenCityPieces != null)
            {
                Debug.Log("Starting broken city repair immediately.");

                brokenCityPieces.RepairCity();

                yield return new WaitForSeconds(brokenCityPieces.repairDuration);

                if (repairedCityModel != null)
                {
                    repairedCityModel.SetActive(true);
                }

                if (brokenPlaneObject != null)
                {
                    brokenPlaneObject.SetActive(false);
                }

                if (holdAfterRepairDuration > 0f)
                {
                    yield return new WaitForSeconds(holdAfterRepairDuration);
                }
            }
        }

        analogyVisualsFinished = true;

        Debug.Log("Analogy visuals finished.");

        TryStartAnalogyTransition();
    }

    // Controls what happens after the user solves an analogy
    // Marks transition as running, slides analogy away, starts placement layer
    private IEnumerator AnalogySolvedRoutine()
    {
        manager.PhaseTransitionRunning = true;

        Debug.Log("Audio and visuals finished. Starting scene transition.");

        yield return new WaitForSeconds(audioController.pauseBeforeAudio);

        yield return new WaitForSeconds(audioController.pauseAfterAudio);

        yield return StartCoroutine(SlideCurrentAnalogyOut());

        yield return StartCoroutine(placementController.StartPlacementLayer());

        manager.PhaseTransitionRunning = false;
    }

    // Hides every city analogy in the cityAnalogies array
    public void HideAllCityAnalogies()
    {
        if (cityAnalogies == null)
        {
            return;
        }

        foreach (GameObject analogy in cityAnalogies)
        {
            if (analogy != null)
            {
                analogy.SetActive(false);
            }
        }
    }

    // Hides the draggable answer choices for the current city analogy
    // Used before the broken city repair effect starts
    private void HideCurrentPhaseAnswerChoices()
    {
        if (cityAnalogies == null ||
            manager.currentPhase < 0 ||
            manager.currentPhase >= cityAnalogies.Length ||
            cityAnalogies[manager.currentPhase] == null)
        {
            return;
        }

        // Find every ZDraggableItem inside the current analogy
        ZDraggableItem[] answerChoices =
            cityAnalogies[manager.currentPhase].GetComponentsInChildren<ZDraggableItem>(true);

        foreach (ZDraggableItem choice in answerChoices)
        {
            if (choice != null)
            {
                choice.gameObject.SetActive(false);
            }
        }

        Debug.Log("Hid " + answerChoices.Length + " orbiting answer choices.");
    }

    public void ResetCurrentAnalogyChoices()
    {
        if (cityAnalogies == null ||
            manager.currentPhase < 0 ||
            manager.currentPhase >= cityAnalogies.Length ||
            cityAnalogies[manager.currentPhase] == null)
        {
            return;
        }

        GameObject analogy = cityAnalogies[manager.currentPhase];

        ZDraggableItem[] choices =
            analogy.GetComponentsInChildren<ZDraggableItem>(true);

        foreach (ZDraggableItem choice in choices)
        {
            if (choice == null)
            {
                continue;
            }

            choice.gameObject.SetActive(true);
            choice.enabled = true;

            Collider[] colliders =
                choice.GetComponentsInChildren<Collider>(true);

            foreach (Collider col in colliders)
            {
                if (col != null)
                {
                    col.enabled = true;
                }
            }

            Rigidbody[] rigidbodies =
                choice.GetComponentsInChildren<Rigidbody>(true);

            foreach (Rigidbody rb in rigidbodies)
            {
                if (rb == null)
                {
                    continue;
                }

                rb.isKinematic = true;
                rb.useGravity = false;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            choice.ResetPosition();

            Debug.Log(
                "Enabled answer: " + choice.name +
                " | Script: " + choice.enabled +
                " | Active: " + choice.gameObject.activeInHierarchy +
                " | Colliders: " + colliders.Length
            );
        }

        Debug.Log(
            "Prepared " + choices.Length +
            " answers for phase " + manager.currentPhase
        );
    }

    // Checks if the current phase should use the broken city repair effect
    private bool IsBrokenCityPhase()
    {
        return manager.currentPhase == brokenCityPhaseIndex && brokenCityPieces != null;
    }

    public int NumberOfPhases
    {
        get
        {
            return cityAnalogies != null ? cityAnalogies.Length : 0;
        }
    }

    public void OnAnalogySolved()
    {
        analogySolved = true;
        StartCoroutine(StartCorrectAnalogyVisualsImmediately());
        TryStartAnalogyTransition();
    }

    private IEnumerator SlideCurrentAnalogyOut()
    {
        GameObject objectToSlide = null;

        if (cityAnalogies != null &&
            manager.currentPhase >= 0 &&
            manager.currentPhase < cityAnalogies.Length)
        {
            objectToSlide = cityAnalogies[manager.currentPhase];
        }

        if (objectToSlide == null)
        {
            yield break;
        }

        Vector3 startPosition = objectToSlide.transform.position;

        Vector3 endPosition =
            startPosition + new Vector3(0f, 0f, -transitionController.analogyZTransitionDistance);

        yield return StartCoroutine(
            transitionController.SlideObject(objectToSlide,
            startPosition, endPosition,
            transitionController.analogySlideDuration) );

        objectToSlide.SetActive(false);

        if (repairedCityModel != null)
        {
            repairedCityModel.SetActive(false);
        }
    }

    public void ResetAnalogyState()
    {
        analogyCorrectVisualsStarted = false;
        analogyVisualsFinished = false;
        analogySolved = false;
    }

    public void RestoreOriginalAnalogyPositions()
    {
        if (cityAnalogies == null || originalCityAnalogyPositions == null)
        {
            return;
        }

        for (int i = 0; i <cityAnalogies.Length; i++)
        {
            if (cityAnalogies[i] == null || i >= originalCityAnalogyPositions.Length)
            {
                continue;
            }

            cityAnalogies[i].transform.position = originalCityAnalogyPositions[i];
            cityAnalogies[i].SetActive(false);
        }
    }
}
