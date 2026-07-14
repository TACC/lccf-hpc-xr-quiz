using System.Collections;
using UnityEngine;
using TMPro;

// SuperCityManager controls changing between scenes

// Plays intro, city analogy phases, audio, city animations, sliding the 
// analogies away, and placement layers
public class SuperCityManager : MonoBehaviour
{
    [Header("Game State")]

    public int currentPhase = 0;


    [Header("Intro Layer")]

    // GameObject that contains intro scene
    public GameObject introLayer;

    public float introDuration = 15f;

    [Header("Intro Burst")]
    public IntroOrbBurst introOrbBurst;


    [Header("City Layer")]

    // Parent object that holds all city analogy scenes
    public GameObject cityLayer;

    // Array stores each city analogy phase
    public GameObject[] cityAnalogies;

    [Header("Book Orbit")]
    public BookOrbitGroup[] bookOrbitGroups;
    public float bookReturnDuration = 1.2f;

    [Header("Broken City Effect")]

    [Header("Final Celebration")]

    public int finalPlacementPhaseIndex = 5;

    public GameObject[] finalAnalogyObjects;

    public Transform finalOrbitCenter;

    public TMP_Text finalScoreText;

    public AudioClip finalAudioClip;

    public float finalObjectAppearDelay = 0.5f;

    public float finalOrbitRadius = 2.5f;

    public float finalOrbitHeight = 0.5f;

    public float finalOrbitSpeed = 45f;

    public float finalGlowIntensity = 1.5f;

    public Color finalGlowColor = Color.green;

    // Connects to BrokenCityPieces script
    public BrokenCityPieces brokenCityPieces;

    [Tooltip("Only this phase uses the broken city repair effect. First phase is 0, second phase is 1, third phase is 2.")]

    // First phase uses the broken city repair animation
    public int brokenCityPhaseIndex = 0;

    // Wait for broken city repair animation before transitioning
    public bool waitForCityRepairBeforePlacement = true;

    public float holdAfterRepairDuration = 3f;

    [Tooltip("The clean/original city model that appears after the broken pieces reconnect.")]

    // Full model appears after repairs
    public GameObject repairedCityModel;

    [Tooltip("The full broken city parent object that should hide after the repaired model appears.")]

    // Parent object holding the broken city version
    public GameObject brokenPlaneObject;


    [Header("Transition Flash")]

    // Optional flash effect between analogy scene and placement scene
    public GameObject transitionFlashObject;

    public float transitionFlashDuration = 0.5f;


    [Header("Placement Layer")]

    // The parent object that holds the placement scene
    public GameObject placementLayer;

    // Stores the placement scene for each phase
    public GameObject[] placementGroups;

    [Tooltip("This phase only shows the motherboard and then automatically moves on.")]

    // Motherboard phase does not wait for user input
    public int motherboardOnlyPlacementPhaseIndex = 0;


    [Header("Motherboard-Only Timer")]

    public float motherboardOnlyDuration = 3f;


    [Header("Audio")]

    public AudioSource explanationAudioSource;

    // Audio clips for city analogy explanations
    public AudioClip[] explanationClips;

    // Audio clips for placement layer explanations
    public AudioClip[] placementExplanationClips;


    [Header("Timing")]

    public float pauseBeforeAudio = 0.5f;

    public float pauseAfterAudio = 0.5f;


    [Header("Analogy Slide Transition")]

    // How far the city analogy moves upward when transitioning out
    public float analogySlideUpDistance = 5f;

    // How long the upward slide takes
    public float analogySlideUpDuration = 1.2f;


    [Header("Placement Slide Transition")]

    // How low the placement group starts before sliding into view
    public float placementSlideUpDistance = 5f;

    public float placementSlideUpDuration = 1.2f;


    private bool phaseTransitionRunning = false;

    private int analogyCorrectFirstTryScore = 0;
    private bool currentAnalogyHadWrongGuess = false;

    private bool analogyAudioFinished = false;
    private bool analogySolvedWhileAudioPlaying = false;

    private bool finalOrbitRunning = false;
    private float finalOrbitAngle = 0f;

    // Used so scene cannot move on until placement audio is finished
    private bool placementAudioFinished = false;
    private bool hardwarePlacementCompleted = false;

    private Vector3[] originalCityAnalogyPositions;

    private Vector3[] finalOriginalOffsets;
    private Vector3[] finalOriginalPositions;

    private bool analogyCorrectVisualsStarted = false;
    private bool analogyScoreCountedThisPhase = false;
    private bool brokenCityRepairStarted = false;

    private Quaternion[] finalOriginalRotations;

    private bool analogyVisualsFinished = false;
    private bool analogySolved = false;

    // Runs once when the scene begins
    void Start()
    {
        // Stores each analogy object's starting position
        originalCityAnalogyPositions = new Vector3[cityAnalogies.Length];

        // For each city analogy
        for (int i = 0; i < cityAnalogies.Length; i++)
        {
            if (cityAnalogies[i] != null)
            {
                // Used to reset world positions when analogy starts again
                originalCityAnalogyPositions[i] = cityAnalogies[i].transform.position;
            }
        }

        // Hide everything at the very beginning
        HideAll();

        StoreFinalOriginalPositions();

        // Start the intro - glowing orb
        StartCoroutine(PlayIntroThenStartSceneOne());
    }

    private void StoreFinalOriginalPositions()
    {
        if (finalAnalogyObjects == null || finalOrbitCenter == null)
        {
            return;
        }

        finalOriginalPositions = new Vector3[finalAnalogyObjects.Length];
        finalOriginalOffsets = new Vector3[finalAnalogyObjects.Length];
        finalOriginalRotations = new Quaternion[finalAnalogyObjects.Length];

        for (int i = 0; i < finalAnalogyObjects.Length; i++)
        {
            if (finalAnalogyObjects[i] == null)
            {
                continue;
            }

            finalOriginalPositions[i] = finalAnalogyObjects[i].transform.position;
            finalOriginalOffsets[i] = finalAnalogyObjects[i].transform.position - finalOrbitCenter.position;
            finalOriginalRotations[i] = finalAnalogyObjects[i].transform.rotation;
        }
    }

    // Hides every major layer and phase object
    private void HideAll()
    {
        if (introLayer != null)
        {
            introLayer.SetActive(false);
        }

        if (cityLayer != null)
        {
            cityLayer.SetActive(false);
        }

        if (placementLayer != null)
        {
            placementLayer.SetActive(false);
        }

        if (repairedCityModel != null)
        {
            repairedCityModel.SetActive(false);
        }

        if (transitionFlashObject != null)
        {
            transitionFlashObject.SetActive(false);
        }

        HideAllCityAnalogies();

        HideAllPlacementGroups();

        HideFinalCelebrationObjects();
    }


    // Handles the intro sequence
    private IEnumerator PlayIntroThenStartSceneOne()
    {
        Debug.Log("Intro started.");

        if (introLayer != null)
        {
            introLayer.SetActive(true);
        }

        if (cityLayer != null)
        {
            cityLayer.SetActive(false);
        }

        yield return new WaitForSeconds(introDuration);

        int phaseIndex = 0;

        if (cityAnalogies == null || phaseIndex < 0 || phaseIndex >= cityAnalogies.Length)
        {
            Debug.LogWarning("Invalid first city analogy phase.");
            yield break;
        }

        currentPhase = phaseIndex;
        phaseTransitionRunning = false;

        if (cityLayer != null)
        {
            cityLayer.SetActive(true);
        }

        if (placementLayer != null)
        {
            placementLayer.SetActive(false);
        }

        HideAllCityAnalogies();
        HideAllPlacementGroups();

        if (repairedCityModel != null)
        {
            repairedCityModel.SetActive(false);
        }

        if (brokenPlaneObject != null)
        {
            brokenPlaneObject.SetActive(IsBrokenCityPhase());
        }

        GameObject firstAnalogy = cityAnalogies[currentPhase];

        if (firstAnalogy == null)
        {
            yield break;
        }

        firstAnalogy.transform.position = originalCityAnalogyPositions[currentPhase];

        foreach (Transform child in firstAnalogy.transform)
        {
            child.gameObject.SetActive(true);
        }

        // The first analogy stays hidden until the glow reaches peak brightness
        firstAnalogy.SetActive(false);

        if (introOrbBurst != null)
        {
            yield return StartCoroutine(introOrbBurst.PlayBurstThenReveal(firstAnalogy));
        }
        else
        {
            firstAnalogy.SetActive(true);
        }

        if (introLayer != null)
        {
            introLayer.SetActive(false);
        }

        if (bookOrbitGroups != null &&
            currentPhase >= 0 &&
            currentPhase < bookOrbitGroups.Length &&
            bookOrbitGroups[currentPhase] != null)
        {
            bookOrbitGroups[currentPhase].RefreshBookOrbits();
            bookOrbitGroups[currentPhase].SaveCurrentBookPositionsAsHome();
            bookOrbitGroups[currentPhase].StartAllBookOrbits();
        }

        currentAnalogyHadWrongGuess = false;
        analogyScoreCountedThisPhase = false;
        analogyCorrectVisualsStarted = false;
        brokenCityRepairStarted = false;
        analogyVisualsFinished = false;
        analogySolved = false;

        StartCoroutine(PlayAnalogyAudioForCurrentPhase());

        Debug.Log("Intro finished. First analogy revealed.");
    }

    private void PrepareFirstAnalogyInMiddle()
    {
        int phaseIndex = 0;

        if (cityAnalogies == null || phaseIndex < 0 || phaseIndex >= cityAnalogies.Length)
        {
            Debug.LogWarning("Invalid first city analogy phase.");
            return;
        }

        currentPhase = phaseIndex;
        phaseTransitionRunning = false;

        if (cityLayer != null)
        {
            cityLayer.SetActive(true);
        }

        if (placementLayer != null)
        {
            placementLayer.SetActive(false);
        }

        HideAllCityAnalogies();
        HideAllPlacementGroups();

        if (repairedCityModel != null)
        {
            repairedCityModel.SetActive(false);
        }

        if (brokenPlaneObject != null)
        {
            brokenPlaneObject.SetActive(IsBrokenCityPhase());
        }

        GameObject firstAnalogy = cityAnalogies[currentPhase];

        if (firstAnalogy == null)
        {
            return;
        }

        // Put first analogy directly in its normal middle position
        firstAnalogy.transform.position = originalCityAnalogyPositions[currentPhase];
        firstAnalogy.SetActive(true);

        foreach (Transform child in firstAnalogy.transform)
        {
            child.gameObject.SetActive(true);
        }

        if (bookOrbitGroups != null &&
            currentPhase >= 0 &&
            currentPhase < bookOrbitGroups.Length &&
            bookOrbitGroups[currentPhase] != null)
        {
            bookOrbitGroups[currentPhase].RefreshBookOrbits();
            bookOrbitGroups[currentPhase].SaveCurrentBookPositionsAsHome();
            bookOrbitGroups[currentPhase].StartAllBookOrbits();
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

        currentPhase = phaseIndex;

        phaseTransitionRunning = false;

        if (cityLayer != null)
        {
            cityLayer.SetActive(true);
        }

        if (placementLayer != null)
        {
            placementLayer.SetActive(false);
        }

        if (transitionFlashObject != null)
        {
            transitionFlashObject.SetActive(false);
        }

        HideAllCityAnalogies();
        HideAllPlacementGroups();

        if (repairedCityModel != null)
        {
            repairedCityModel.SetActive(false);
        }

        if (brokenPlaneObject != null)
        {
            brokenPlaneObject.SetActive(IsBrokenCityPhase());
        }

        GameObject currentAnalogy = cityAnalogies[currentPhase];

        if (currentAnalogy != null)
        {
            currentAnalogy.transform.position = originalCityAnalogyPositions[currentPhase];

            currentAnalogy.SetActive(true);

            foreach (Transform child in currentAnalogy.transform)
            {
                child.gameObject.SetActive(true);
            }

            if (bookOrbitGroups != null &&
                currentPhase >= 0 &&
                currentPhase < bookOrbitGroups.Length &&
                bookOrbitGroups[currentPhase] != null)
            {
                bookOrbitGroups[currentPhase].RefreshBookOrbits();
                bookOrbitGroups[currentPhase].SaveCurrentBookPositionsAsHome();
                bookOrbitGroups[currentPhase].StartAllBookOrbits();
            }
        }

        Debug.Log("Starting analogy phase " + currentPhase);
    }


    // Hides every city analogy in the cityAnalogies array
    private void HideAllCityAnalogies()
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


    // Hides every placement group in the placementGroups array
    private void HideAllPlacementGroups()
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


    // Checks if the current phase should use the broken city repair effect
    private bool IsBrokenCityPhase()
    {
        return currentPhase == brokenCityPhaseIndex && brokenCityPieces != null;
    }


    // Checks if the current placement phase should be motherboard-only
    private bool IsMotherboardOnlyPlacementPhase()
    {
        return currentPhase == motherboardOnlyPlacementPhaseIndex;
    }


    // Called when the user selects the correct answer in the city analogy layer
    public void OnAnalogySolved()
    {
        if (phaseTransitionRunning)
        {
            return;
        }

        if (!analogyScoreCountedThisPhase)
        {
            if (!currentAnalogyHadWrongGuess)
            {
                analogyCorrectFirstTryScore++;
                Debug.Log("First try correct! Score: " + analogyCorrectFirstTryScore);
            }
            else
            {
                Debug.Log("Correct, but not on first try. Score stays: " + analogyCorrectFirstTryScore);
            }

            analogyScoreCountedThisPhase = true;
        }

        analogySolved = true;
        StartCoroutine(StartCorrectAnalogyVisualsImmediately());

        TryStartAnalogyTransition();
    }

    private void TryStartAnalogyTransition()
    {
        if (!analogySolved)
        {
            return;
        }

        if (!analogyAudioFinished)
        {
            Debug.Log("Waiting for analogy audio to finish.");
            return;
        }

        if (!analogyVisualsFinished)
        {
            Debug.Log("Waiting for analogy visuals to finish.");
            return;
        }

        if (phaseTransitionRunning)
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
            currentPhase >= 0 &&
            currentPhase < bookOrbitGroups.Length &&
            bookOrbitGroups[currentPhase] != null)
        {
            bookOrbitGroups[currentPhase].StopAllBookOrbits();

            yield return StartCoroutine(
                bookOrbitGroups[currentPhase].SlideAllBooksBack(bookReturnDuration)
            );
        }

        // Broken city animation
        if (IsBrokenCityPhase())
        {
            HideCurrentPhaseAnswerChoices();

            if (brokenCityPieces != null)
            {
                Debug.Log("Starting broken city repair immediately.");

                brokenCityPieces.RepairCity();
                brokenCityRepairStarted = true;

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

    public void OnAnalogyWrongGuess()
    {
        currentAnalogyHadWrongGuess = true;
        Debug.Log("Wrong analogy guess. This phase no longer counts as first try.");
    }


    // Controls what happens after the user solves an analogy
    //
    // Marks transition as running,
    // slides analogy away, starts placement layer
    private IEnumerator AnalogySolvedRoutine()
    {
        phaseTransitionRunning = true;

        Debug.Log("Audio and visuals finished. Starting scene transition.");

        yield return new WaitForSeconds(pauseBeforeAudio);

        yield return new WaitForSeconds(pauseAfterAudio);

        yield return StartCoroutine(SlideCurrentAnalogyUp());

        yield return StartCoroutine(PlayTransitionFlash());

        yield return StartCoroutine(StartPlacementLayer());

        phaseTransitionRunning = false;
    }

    private IEnumerator PlayPlacementAudioForCurrentPhase()
    {
        placementAudioFinished = false;

        if (explanationAudioSource != null &&
            placementExplanationClips != null &&
            currentPhase >= 0 &&
            currentPhase < placementExplanationClips.Length &&
            placementExplanationClips[currentPhase] != null)
        {
            explanationAudioSource.clip = placementExplanationClips[currentPhase];
            explanationAudioSource.Play();

            yield return new WaitForSeconds(placementExplanationClips[currentPhase].length);
        }

        placementAudioFinished = true;

        // If the user already placed the hardware while the audio was playing,
        // continue now that the audio is finished.
        if (hardwarePlacementCompleted)
        {
            StartCoroutine(HardwarePlacedRoutine());
        }
    }

    private IEnumerator PlayAnalogyAudioForCurrentPhase()
    {
        analogyAudioFinished = false;
        analogySolvedWhileAudioPlaying = false;

        if (explanationAudioSource != null &&
            explanationClips != null &&
            currentPhase >= 0 &&
            currentPhase < explanationClips.Length &&
            explanationClips[currentPhase] != null)
        {
            explanationAudioSource.Stop();
            explanationAudioSource.clip = explanationClips[currentPhase];
            explanationAudioSource.Play();

            Debug.Log("Playing analogy audio for phase " + currentPhase);

            yield return new WaitForSeconds(explanationClips[currentPhase].length);
        }

        analogyAudioFinished = true;

        TryStartAnalogyTransition();
    }

    // Slides the current analogy upward when it is done
    private IEnumerator SlideCurrentAnalogyUp()
    {
        GameObject objectToSlide = null;

        if (repairedCityModel != null && repairedCityModel.activeSelf)
        {
            objectToSlide = repairedCityModel;
        }

        // Slide the current city analogy
        else if (cityAnalogies != null &&
                 currentPhase >= 0 &&
                 currentPhase < cityAnalogies.Length)
        {
            objectToSlide = cityAnalogies[currentPhase];
        }

        if (objectToSlide == null)
        {
            yield break;
        }

        Vector3 startPosition = objectToSlide.transform.position;

        Vector3 endPosition = startPosition + new Vector3(0f, analogySlideUpDistance, 0f);

        yield return StartCoroutine(SlideObject(
            objectToSlide,
            startPosition,
            endPosition,
            analogySlideUpDuration
        ));

        // After sliding, hide the current city analogy
        if (cityAnalogies != null &&
            currentPhase >= 0 &&
            currentPhase < cityAnalogies.Length &&
            cityAnalogies[currentPhase] != null)
        {
            cityAnalogies[currentPhase].SetActive(false);
        }

        if (repairedCityModel != null)
        {
            repairedCityModel.SetActive(false);
        }
    }


    // This turns on the transition flash for a short amount of time
    private IEnumerator PlayTransitionFlash()
    {
        // Skip if no flash object is assigned
        if (transitionFlashObject == null)
        {
            yield break;
        }

        transitionFlashObject.SetActive(true);

        yield return new WaitForSeconds(transitionFlashDuration);

        transitionFlashObject.SetActive(false);
    }


    // Starts the placement layer for the current phase
    private IEnumerator StartPlacementLayer()
    {
        if (placementLayer != null)
        {
            placementLayer.SetActive(true);
        }

        HideAllPlacementGroups();

        GameObject currentPlacementGroup = null;

        if (placementGroups != null &&
            currentPhase >= 0 &&
            currentPhase < placementGroups.Length &&
            placementGroups[currentPhase] != null)
        {
            currentPlacementGroup = placementGroups[currentPhase];
        }

        if (currentPlacementGroup == null)
        {
            Debug.LogWarning("No placement group assigned for phase " + currentPhase);
            yield break;
        }

        Vector3 finalPosition = currentPlacementGroup.transform.position;
        Vector3 startPosition = finalPosition + new Vector3(0f, -placementSlideUpDistance, 0f);

        currentPlacementGroup.transform.position = startPosition;
        currentPlacementGroup.SetActive(true);

        yield return StartCoroutine(SlideObject(
            currentPlacementGroup,
            startPosition,
            finalPosition,
            placementSlideUpDuration
        ));

        Debug.Log("Showing placement group for phase " + currentPhase);

        // Reset placement completion state for this phase
        placementAudioFinished = false;
        hardwarePlacementCompleted = false;

        // The city-to-placement transition is done now
        // Allow hardware placement to be detected
        phaseTransitionRunning = false;

        // Start placement audio
        StartCoroutine(PlayPlacementAudioForCurrentPhase());

        if (IsMotherboardOnlyPlacementPhase())
        {
            Debug.Log("Motherboard-only placement phase. Moving on after " + motherboardOnlyDuration + " seconds.");

            yield return new WaitForSeconds(motherboardOnlyDuration);

            OnHardwarePlaced();
        }
        else
        {
            Debug.Log("Waiting for user to correctly place the hardware.");
        }
    }


    // Slides any GameObject from one position to another
    private IEnumerator SlideObject(GameObject obj, Vector3 startPosition, Vector3 endPosition, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {

            elapsedTime += Time.deltaTime;

            float t = elapsedTime / duration;

            t = Mathf.SmoothStep(0f, 1f, t);

            if (obj != null)
            {
                // Lerp = "linear interpolation"
                obj.transform.position = Vector3.Lerp(startPosition, endPosition, t);
            }

            yield return null;
        }

        // Force the object exactly to the final position
        if (obj != null)
        {
            obj.transform.position = endPosition;
        }
    }


    // Hides the draggable answer choices for the current city analogy
    // Used before the broken city repair effect starts
    private void HideCurrentPhaseAnswerChoices()
    {
        if (cityAnalogies == null ||
            currentPhase < 0 ||
            currentPhase >= cityAnalogies.Length ||
            cityAnalogies[currentPhase] == null)
        {
            return;
        }

        // Find every ZDraggableItem inside the current analogy
        ZDraggableItem[] answerChoices =
            cityAnalogies[currentPhase].GetComponentsInChildren<ZDraggableItem>(true);

        foreach (ZDraggableItem choice in answerChoices)
        {
            if (choice != null)
            {
                choice.gameObject.SetActive(false);
            }
        }

        Debug.Log("Hid " + answerChoices.Length + " orbiting answer choices.");
    }


    // Called when the hardware placement is finished
    public void OnHardwarePlaced()
    {
        if (phaseTransitionRunning)
        {
            return;
        }

        hardwarePlacementCompleted = true;

        if (!placementAudioFinished)
        {
            Debug.Log("Hardware placed, but waiting for placement audio to finish.");
            return;
        }

        StartCoroutine(HardwarePlacedRoutine());
    }

    private IEnumerator HardwarePlacedRoutine()
    {
        if (phaseTransitionRunning)
        {
            yield break;
        }
        
        phaseTransitionRunning = true;

        Debug.Log("Hardware placed correctly or timer finished for phase " + currentPhase);

        if (currentPhase == finalPlacementPhaseIndex)
        {
            yield return StartCoroutine(FinalCelebrationRoutine());
            yield break;
        }

        GameObject currentPlacementGroup = null;

        if (placementGroups != null &&
            currentPhase >= 0 &&
            currentPhase < placementGroups.Length)
        {
            currentPlacementGroup = placementGroups[currentPhase];
        }

        // Slide the placement group upward out of view
        if (currentPlacementGroup != null && currentPlacementGroup.activeSelf)
        {
            Vector3 startPosition = currentPlacementGroup.transform.position;
            Vector3 endPosition = startPosition + new Vector3(0f, placementSlideUpDistance, 0f);

            yield return StartCoroutine(SlideObject(
                currentPlacementGroup,
                startPosition,
                endPosition,
                placementSlideUpDuration
            ));

            currentPlacementGroup.SetActive(false);
        }

        HideAllPlacementGroups();

        if (placementLayer != null)
        {
            placementLayer.SetActive(false);
        }

        currentPhase++;

        if (cityAnalogies != null && currentPhase < cityAnalogies.Length)
        {
            yield return StartCoroutine(StartAnalogyPhaseWithSlide(currentPhase));
        }
        else
        {
            Debug.Log("All phases completed.");
        }

        phaseTransitionRunning = false;
    }

    // Starts a specific analogy phase with slide-in
    private IEnumerator StartAnalogyPhaseWithSlide(int phaseIndex)
    {
        if (cityAnalogies == null || phaseIndex < 0 || phaseIndex >= cityAnalogies.Length)
        {
            Debug.LogWarning("Invalid city analogy phase: " + phaseIndex);
            yield break;
        }

        currentPhase = phaseIndex;

        if (cityLayer != null)
        {
            cityLayer.SetActive(true);
        }

        if (placementLayer != null)
        {
            placementLayer.SetActive(false);
        }

        if (transitionFlashObject != null)
        {
            transitionFlashObject.SetActive(false);
        }

        HideAllCityAnalogies();
        HideAllPlacementGroups();

        if (repairedCityModel != null)
        {
            repairedCityModel.SetActive(false);
        }

        if (brokenPlaneObject != null)
        {
            brokenPlaneObject.SetActive(IsBrokenCityPhase());
        }

        GameObject currentAnalogy = cityAnalogies[currentPhase];

        if (currentAnalogy == null)
        {
            yield break;
        }

        Vector3 finalPosition = originalCityAnalogyPositions[currentPhase];
        Vector3 startPosition = finalPosition + new Vector3(0f, -analogySlideUpDistance, 0f);

        // Put the whole analogy below the screen first
        currentAnalogy.transform.position = startPosition;
        currentAnalogy.SetActive(true);

        foreach (Transform child in currentAnalogy.transform)
        {
            child.gameObject.SetActive(true);
        }

        // Books orbiting animation
        if (bookOrbitGroups != null &&
            currentPhase >= 0 &&
            currentPhase < bookOrbitGroups.Length &&
            bookOrbitGroups[currentPhase] != null)
        {
            bookOrbitGroups[currentPhase].RefreshBookOrbits();
            bookOrbitGroups[currentPhase].SaveCurrentBookPositionsAsHome();
            bookOrbitGroups[currentPhase].StartAllBookOrbits();
        }

        // Slide the whole analogy parent into place
        yield return StartCoroutine(SlideObject(
            currentAnalogy,
            startPosition,
            finalPosition,
            analogySlideUpDuration
        ));

        currentAnalogyHadWrongGuess = false;
        analogyScoreCountedThisPhase = false;
        analogyCorrectVisualsStarted = false;
        brokenCityRepairStarted = false;
        analogyVisualsFinished = false;
        analogySolved = false;

        StartCoroutine(PlayAnalogyAudioForCurrentPhase());

        Debug.Log("Starting analogy phase " + currentPhase);
    }

    private void SetObjectGreenGlow(GameObject obj)
    {
        Renderer[] objectRenderers = obj.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in objectRenderers)
        {
            if (renderer == null)
            {
                continue;
            }

            Material material = renderer.material;

            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", finalGlowColor * finalGlowIntensity);
            }

            if (material.HasProperty("_Color"))
            {
                Color currentColor = material.color;
                material.color = Color.Lerp(currentColor, finalGlowColor, 0.25f);
            }
        }
    }

    void Update()
    {
        if (!finalOrbitRunning)
        {
            return;
        }

        if (finalAnalogyObjects == null || finalOrbitCenter == null || finalOriginalOffsets == null)
        {
            return;
        }

        finalOrbitAngle += finalOrbitSpeed * Time.deltaTime;

        for (int i = 0; i < finalAnalogyObjects.Length; i++)
        {
            if (finalAnalogyObjects[i] == null || !finalAnalogyObjects[i].activeSelf)
            {
                continue;
            }

            // Rotate each object's original offset around the motherboard
            Quaternion rotation = Quaternion.Euler(0f, finalOrbitAngle, 0f);

            Vector3 rotatedOffset = rotation * finalOriginalOffsets[i];

            finalAnalogyObjects[i].transform.position = finalOrbitCenter.position + rotatedOffset;

            if (finalOriginalRotations != null && i < finalOriginalRotations.Length)
            {
                finalAnalogyObjects[i].transform.rotation = finalOriginalRotations[i];
            }
        }
    }

    private IEnumerator FinalCelebrationRoutine()
    {
        Debug.Log("Final placement complete. Starting final celebration.");

        finalOrbitRunning = false;
        finalOrbitAngle = 0f;

        HideFinalCelebrationObjects();

        if (explanationAudioSource != null && finalAudioClip != null)
        {
            explanationAudioSource.Stop();
            explanationAudioSource.clip = finalAudioClip;
            explanationAudioSource.Play();
        }

        if (finalAnalogyObjects != null)
        {
            for (int i = 0; i < finalAnalogyObjects.Length; i++)
            {
                GameObject obj = finalAnalogyObjects[i];

                if (obj == null)
                {
                    continue;
                }

                // Restore original position
                if (finalOriginalPositions != null && i < finalOriginalPositions.Length)
                {
                    obj.transform.position = finalOriginalPositions[i];
                }

                // Restore original rotation
                if (finalOriginalRotations != null && i < finalOriginalRotations.Length)
                {
                    obj.transform.rotation = finalOriginalRotations[i];
                }

                obj.SetActive(true);

                SetObjectGreenGlow(obj);

                yield return new WaitForSeconds(finalObjectAppearDelay);
            }
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = "Analogy Score: " + analogyCorrectFirstTryScore + " / " + cityAnalogies.Length;
            finalScoreText.gameObject.SetActive(true);
        }

        finalOrbitRunning = true;

        Debug.Log("Final score displayed: " + analogyCorrectFirstTryScore);
    }

    private void HideFinalCelebrationObjects()
    {
        if (finalAnalogyObjects != null)
        {
            foreach (GameObject obj in finalAnalogyObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }

        if (finalScoreText != null)
        {
            finalScoreText.gameObject.SetActive(false);
        }
    }
}