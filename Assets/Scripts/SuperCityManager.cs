using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

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

    public IntroScreenController introScreenController;

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

    public CreditsManager creditsManager;

    public int finalPlacementPhaseIndex = 5;

    public GameObject[] finalAnalogyObjects;

    public TMP_Text finalScoreText;

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

    // Used so scene cannot move on until placement audio is finished
    private bool placementAudioFinished = false;
    private bool hardwarePlacementCompleted = false;

    private Vector3[] originalCityAnalogyPositions;
    private Vector3[] originalPlacementGroupPositions;

    private bool quizInitialized = false;

    private class TransformHomeState
    {
        public Transform transform;
        public Transform parent;
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;
        public bool activeSelf;
    }

    private readonly List<TransformHomeState> sceneTransformHomeStates =
        new List<TransformHomeState>();

    private bool analogyCorrectVisualsStarted = false;
    private bool analogyScoreCountedThisPhase = false;
    private bool brokenCityRepairStarted = false;

    private bool analogyVisualsFinished = false;
    private bool analogySolved = false;

    private bool introIsPlaying = false;

    private class AnswerHomeState
    {
        public ZDraggableItem item;
        public Transform parent;
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;
    }

    private readonly List<AnswerHomeState> answerHomeStates =
        new List<AnswerHomeState>();

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
                originalCityAnalogyPositions[i] = cityAnalogies[i].transform.position;
            }
        }

        // Home positions must be saved
        originalPlacementGroupPositions = new Vector3[placementGroups.Length];

        for (int i = 0; i < placementGroups.Length; i++)
        {
            if (placementGroups[i] != null)
            {
                originalPlacementGroupPositions[i] = placementGroups[i].transform.position;
            }
        }

        StoreSceneTransformHomeStates();
        StoreAnswerHomeStates();

        // Hide everything at the very beginning
        HideAll();

        quizInitialized = true;
    }

    public void BeginQuiz()
    {
        ResetEntireQuiz();
        HideAll();

        currentPhase = 0;
        phaseTransitionRunning = false;

        currentAnalogyHadWrongGuess = false;
        analogyScoreCountedThisPhase = false;
        analogyCorrectVisualsStarted = false;
        brokenCityRepairStarted = false;
        analogyVisualsFinished = false;
        analogySolved = false;
        analogyAudioFinished = false;

        StartAnalogyPhase(0);
        ResetCurrentAnalogyChoices();
        StartCoroutine(PlayAnalogyAudioForCurrentPhase());
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
        introIsPlaying = true;

        Debug.Log("Intro started.");

        if (explanationAudioSource != null)
        {
            explanationAudioSource.Stop();
        }

        if (introLayer != null)
        {
            introLayer.SetActive(true);
        }

        if (cityLayer != null)
        {
            cityLayer.SetActive(false);
        }

        if (placementLayer != null)
        {
            placementLayer.SetActive(false);
        }

        HideAllCityAnalogies();
        HideAllPlacementGroups();

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
            if (repairedCityModel != null &&
                child.gameObject == repairedCityModel)
            {
                child.gameObject.SetActive(false);
                continue;
            }

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

        if (repairedCityModel != null)
        {
            repairedCityModel.SetActive(false);
        }

        if (brokenPlaneObject != null)
        {
            brokenPlaneObject.SetActive(true);
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

        introIsPlaying = false;

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
        if (repairedCityModel != null)
        {
            repairedCityModel.SetActive(false);
        }

        if (brokenPlaneObject != null)
        {
            brokenPlaneObject.SetActive(true);
        }

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
    // Marks transition as running, slides analogy away, starts placement layer
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
        explanationAudioSource.Stop();
        explanationAudioSource.clip =
            placementExplanationClips[currentPhase];
        explanationAudioSource.Play();

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

        explanationAudioSource.Stop();
        explanationAudioSource.clip =
            placementExplanationClips[currentPhase];
        explanationAudioSource.Play();

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

        if (cityAnalogies != null &&
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

        Vector3 endPosition =
            startPosition + new Vector3(0f, analogySlideUpDistance, 0f);

        yield return StartCoroutine(SlideObject(
            objectToSlide,
            startPosition,
            endPosition,
            analogySlideUpDuration
        ));

        objectToSlide.SetActive(false);

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

        Vector3 finalPosition = originalPlacementGroupPositions[currentPhase];
        Vector3 startPosition = finalPosition + new Vector3(0f, -placementSlideUpDistance, 0f);

        currentPlacementGroup.transform.position = startPosition;
        currentPlacementGroup.SetActive(true);
        RestartAnimatorsUnder(currentPlacementGroup);

        yield return StartCoroutine(SlideObject(
            currentPlacementGroup,
            startPosition,
            finalPosition,
            placementSlideUpDuration
        ));

        ShowAllPlacementTargetGlows(currentPlacementGroup);


        Debug.Log("Showing placement group for phase " + currentPhase);

        // Reset placement completion state for this phase
        placementAudioFinished = false;
        hardwarePlacementCompleted = false;

        // The city-to-placement transition is done now
        // Allow hardware placement to be detected
        phaseTransitionRunning = false;

        // Start placement audio
        StartCoroutine(PlayPlacementAudioForCurrentPhase());

        Debug.Log("Waiting for user to correctly place the hardware.");

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
            yield return StartCoroutine(ShowFinalSceneRoutine());
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

        ResetCurrentAnalogyChoices();

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
        analogyAudioFinished = false;

        phaseTransitionRunning = false;

        ResetCurrentAnalogyChoices();

        StartCoroutine(PlayAnalogyAudioForCurrentPhase());

        Debug.Log("Starting analogy phase " + currentPhase);
    }

    private void ResetCurrentAnalogyChoices()
    {
        if (cityAnalogies == null ||
            currentPhase < 0 ||
            currentPhase >= cityAnalogies.Length ||
            cityAnalogies[currentPhase] == null)
        {
            return;
        }

        GameObject analogy = cityAnalogies[currentPhase];

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
            " answers for phase " + currentPhase
        );
    }

    private void HideFinalCelebrationObjects()
    {
        if (finalScoreText != null)
        {
            finalScoreText.gameObject.SetActive(false);
        }
    }

    public void ReplayCurrentScene()
    {
        if (introScreenController != null &&
            introScreenController.IsZCanvasScreenShowing())
        {
            return;
        }

        StopAllCoroutines();

        if (explanationAudioSource != null)
        {
            explanationAudioSource.Stop();
            explanationAudioSource.clip = null;
        }

        phaseTransitionRunning = false;

        bool placementIsShowing =
            placementLayer != null &&
            placementLayer.activeSelf;

        if (placementIsShowing)
        {
            ReplayCurrentPlacement();
        }
        else
        {
            ReplayCurrentAnalogy();
        }
    }

    private void ReplayIntro()
    {
        StopAllCoroutines();

        if (explanationAudioSource != null)
        {
            explanationAudioSource.Stop();
            explanationAudioSource.clip = null;
        }

        if (introOrbBurst != null)
        {
            introOrbBurst.ResetIntro();
        }

        HideAllCityAnalogies();
        ResetCurrentPlacementForReplay();
        HideAllPlacementGroups();

        if (cityLayer != null)
        {
            cityLayer.SetActive(false);
        }

        if (placementLayer != null)
        {
            placementLayer.SetActive(false);
        }

        if (introLayer != null)
        {
            introLayer.SetActive(false);
        }

        introIsPlaying = false;
        phaseTransitionRunning = false;

        StartCoroutine(PlayIntroThenStartSceneOne());

        Debug.Log("Replaying intro.");
    }
        

    private void ReplayCurrentAnalogy()
    {
        HideAllCityAnalogies();
        HideAllPlacementGroups();
        ResetAllCustomAnimations();
        RestoreAllAnswerChoices();

        if (placementLayer != null)
        {
            placementLayer.SetActive(false);
        }

        if (cityLayer != null)
        {
            cityLayer.SetActive(true);
        }

        currentAnalogyHadWrongGuess = false;
        analogyScoreCountedThisPhase = false;
        analogyCorrectVisualsStarted = false;
        brokenCityRepairStarted = false;
        analogyVisualsFinished = false;
        analogySolved = false;
        analogyAudioFinished = false;
        phaseTransitionRunning = false;

        StartAnalogyPhase(currentPhase);

        ResetCurrentAnalogyChoices();

        StartCoroutine(PlayAnalogyAudioForCurrentPhase());

        Debug.Log("Replaying analogy phase " + currentPhase);
    }

    private void ReplayCurrentPlacement()
    {
        HideAllCityAnalogies();
        ResetAllCustomAnimations();
        ResetCurrentPlacementForReplay();
        HideAllPlacementGroups();

        if (cityLayer != null)
        {
            cityLayer.SetActive(false);
        }

        if (placementLayer != null)
        {
            placementLayer.SetActive(true);
        }

        placementAudioFinished = false;
        hardwarePlacementCompleted = false;
        phaseTransitionRunning = false;

        StartCoroutine(StartPlacementLayer());

        Debug.Log("Replaying placement phase " + currentPhase);
    }

    public void ReturnToHome()
    {
        ResetEntireQuiz();
        HideAll();

        Debug.Log("Quiz reset and returned home.");
    }

    private void ResetEntireQuiz()
    {
        StopAllCoroutines();

        introIsPlaying = false;

        if (explanationAudioSource != null)
        {
            explanationAudioSource.Stop();
            explanationAudioSource.clip = null;
        }

        currentPhase = 0;
        analogyCorrectFirstTryScore = 0;

        phaseTransitionRunning = false;

        currentAnalogyHadWrongGuess = false;
        analogyScoreCountedThisPhase = false;
        analogyCorrectVisualsStarted = false;
        brokenCityRepairStarted = false;

        analogyAudioFinished = false;
        analogyVisualsFinished = false;
        analogySolved = false;

        placementAudioFinished = false;
        hardwarePlacementCompleted = false;

        if (introOrbBurst != null)
        {
            introOrbBurst.ResetIntro();
        }

        // Restore every animated/moved object before anything is shown again
        RestoreSceneTransformsAndAnimators();
        ResetAllCustomAnimations();

        RestoreAllAnswerChoices();
        ResetAllPlacementDraggables();
        ResetAllPlacementTargets();

        for (int i = 0; i < cityAnalogies.Length; i++)
        {
            if (cityAnalogies[i] == null)
            {
                continue;
            }

            cityAnalogies[i].transform.position =
                originalCityAnalogyPositions[i];

            cityAnalogies[i].SetActive(false);
        }

        if (placementGroups != null && originalPlacementGroupPositions != null)
        {
            for (int i = 0; i < placementGroups.Length; i++)
            {
                if (placementGroups[i] != null && i < originalPlacementGroupPositions.Length)
                {
                    placementGroups[i].transform.position = originalPlacementGroupPositions[i];
                }
            }
        }

        HideAllPlacementGroups();

        if (repairedCityModel != null)
        {
            repairedCityModel.SetActive(false);
        }

        if (brokenPlaneObject != null)
        {
            brokenPlaneObject.SetActive(true);
        }

        if (transitionFlashObject != null)
        {
            transitionFlashObject.SetActive(false);
        }

        HideFinalCelebrationObjects();
    }

    private void StoreSceneTransformHomeStates()
    {
        sceneTransformHomeStates.Clear();

        HashSet<Transform> storedTransforms = new HashSet<Transform>();

        StoreRootTransformStates(cityAnalogies, storedTransforms);
        StoreRootTransformStates(placementGroups, storedTransforms);

        if (brokenPlaneObject != null)
        {
            StoreTransformTree(brokenPlaneObject.transform, storedTransforms);
        }

        if (repairedCityModel != null)
        {
            StoreTransformTree(repairedCityModel.transform, storedTransforms);
        }
    }

    private void StoreRootTransformStates(GameObject[] roots, HashSet<Transform> storedTransforms)
    {
        if (roots == null)
        {
            return;
        }

        foreach (GameObject root in roots)
        {
            if (root != null)
            {
                StoreTransformTree(root.transform, storedTransforms);
            }
        }
    }

    private void StoreTransformTree(Transform root, HashSet<Transform> storedTransforms)
    {
        if (root == null || storedTransforms.Contains(root))
        {
            return;
        }

        storedTransforms.Add(root);

        sceneTransformHomeStates.Add(new TransformHomeState
        {
            transform = root,
            parent = root.parent,
            localPosition = root.localPosition,
            localRotation = root.localRotation,
            localScale = root.localScale,
            activeSelf = root.gameObject.activeSelf
        });

        foreach (Transform child in root)
        {
            StoreTransformTree(child, storedTransforms);
        }
    }

    private void RestoreSceneTransformsAndAnimators()
    {
        foreach (TransformHomeState state in sceneTransformHomeStates)
        {
            if (state == null || state.transform == null)
            {
                continue;
            }

            if (state.parent != null && state.transform.parent != state.parent)
            {
                state.transform.SetParent(state.parent, false);
            }

            state.transform.localPosition = state.localPosition;
            state.transform.localRotation = state.localRotation;
            state.transform.localScale = state.localScale;
            state.transform.gameObject.SetActive(state.activeSelf);
        }

        ResetAnimatorsUnder(cityLayer);
        ResetAnimatorsUnder(placementLayer);
        ResetAnimatorsUnder(introLayer);
    }

    private void ResetAnimatorsUnder(GameObject root)
    {
        if (root == null)
        {
            return;
        }

        Animator[] animators = root.GetComponentsInChildren<Animator>(true);

        foreach (Animator animator in animators)
        {
            if (animator == null)
            {
                continue;
            }

            animator.Rebind();
            animator.Update(0f);
        }
    }

    private void RestartAnimatorsUnder(GameObject root)
    {
        if (root == null)
        {
            return;
        }

        Animator[] animators = root.GetComponentsInChildren<Animator>(true);

        foreach (Animator animator in animators)
        {
            if (animator == null)
            {
                continue;
            }

            animator.enabled = true;
            animator.Rebind();
            animator.Update(0f);
            animator.Play(0, 0, 0f);
            animator.Update(0f);
        }
    }


    private void ResetCurrentPlacementForReplay()
    {
        if (placementGroups == null ||
            currentPhase < 0 ||
            currentPhase >= placementGroups.Length ||
            placementGroups[currentPhase] == null)
        {
            return;
        }

        Transform placementRoot = placementGroups[currentPhase].transform;

        foreach (TransformHomeState state in sceneTransformHomeStates)
        {
            if (state == null || state.transform == null)
            {
                continue;
            }

            if (state.transform != placementRoot && !state.transform.IsChildOf(placementRoot))
            {
                continue;
            }

            if (state.parent != null && state.transform.parent != state.parent)
            {
                state.transform.SetParent(state.parent, false);
            }

            state.transform.localPosition = state.localPosition;
            state.transform.localRotation = state.localRotation;
            state.transform.localScale = state.localScale;
            state.transform.gameObject.SetActive(state.activeSelf);
        }

        if (originalPlacementGroupPositions != null &&
            currentPhase < originalPlacementGroupPositions.Length)
        {
            placementGroups[currentPhase].transform.position =
                originalPlacementGroupPositions[currentPhase];
        }

        ResetAnimatorsUnder(placementGroups[currentPhase]);
        ResetPlacementDraggables(placementGroups[currentPhase]);
        ResetPlacementTargets(placementGroups[currentPhase]);
        ShowAllPlacementTargetGlows(placementGroups[currentPhase]);
    }

    private void StoreAnswerHomeStates()
    {
        answerHomeStates.Clear();

        if (cityAnalogies == null)
        {
            return;
        }

        foreach (GameObject analogy in cityAnalogies)
        {
            if (analogy == null)
            {
                continue;
            }

            ZDraggableItem[] choices =
                analogy.GetComponentsInChildren<ZDraggableItem>(true);

            foreach (ZDraggableItem choice in choices)
            {
                if (choice == null)
                {
                    continue;
                }

                AnswerHomeState state = new AnswerHomeState
                {
                    item = choice,
                    parent = choice.transform.parent,
                    localPosition = choice.transform.localPosition,
                    localRotation = choice.transform.localRotation,
                    localScale = choice.transform.localScale
                };

                answerHomeStates.Add(state);
            }
        }
    }

    private void RestoreAllAnswerChoices()
    {
        foreach (AnswerHomeState state in answerHomeStates)
        {
            if (state == null || state.item == null)
            {
                continue;
            }

            Transform itemTransform = state.item.transform;

            if (state.parent != null &&
                itemTransform.parent != state.parent)
            {
                itemTransform.SetParent(state.parent, false);
            }

            itemTransform.localPosition = state.localPosition;
            itemTransform.localRotation = state.localRotation;
            itemTransform.localScale = state.localScale;

            state.item.enabled = true;
            state.item.gameObject.SetActive(true);

            Collider[] colliders =
                state.item.GetComponentsInChildren<Collider>(true);

            foreach (Collider col in colliders)
            {
                if (col != null)
                {
                    col.enabled = true;
                }
            }

            Rigidbody[] rigidbodies =
                state.item.GetComponentsInChildren<Rigidbody>(true);

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
        }
    }

    private IEnumerator ShowFinalSceneRoutine()
    {
        GameObject currentPlacementGroup = null;

        if (placementGroups != null &&
            currentPhase >= 0 &&
            currentPhase < placementGroups.Length)
        {
            currentPlacementGroup = placementGroups[currentPhase];
        }

        if (currentPlacementGroup != null && currentPlacementGroup.activeSelf)
        {
            Vector3 startPosition = currentPlacementGroup.transform.position;
            Vector3 endPosition =
                startPosition + new Vector3(0f, placementSlideUpDistance, 0f);

            yield return StartCoroutine(SlideObject(
                currentPlacementGroup,
                startPosition,
                endPosition,
                placementSlideUpDuration
            ));

            currentPlacementGroup.SetActive(false);
        }

        HideAllCityAnalogies();
        HideAllPlacementGroups();

        if (cityLayer != null)
        {
            cityLayer.SetActive(false);
        }

        if (placementLayer != null)
        {
            placementLayer.SetActive(false);
        }

        if (explanationAudioSource != null)
        {
            explanationAudioSource.Stop();
            explanationAudioSource.clip = null;
        }

        phaseTransitionRunning = false;

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
                analogyCorrectFirstTryScore +
                " / " +
                cityAnalogies.Length + " COMPONENTS";

            finalScoreText.gameObject.SetActive(true);
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

    private void ResetAllCustomAnimations()
    {
        if (!quizInitialized)
        {
            return;
        }

        if (bookOrbitGroups != null)
        {
            foreach (BookOrbitGroup group in bookOrbitGroups)
            {
                if (group != null)
                {
                    group.ResetAllBooks();
                }
            }
        }

        if (cityLayer != null)
        {
            BuildingDarkFilter[] filters =
                cityLayer.GetComponentsInChildren<BuildingDarkFilter>(true);

            foreach (BuildingDarkFilter filter in filters)
            {
                if (filter != null)
                {
                    filter.ResetFilter();
                }
            }

            ElevatorDoorBrokenAnimation[] elevators =
                cityLayer.GetComponentsInChildren<ElevatorDoorBrokenAnimation>(true);

            foreach (ElevatorDoorBrokenAnimation elevator in elevators)
            {
                if (elevator != null)
                {
                    elevator.ResetElevatorDoors();
                }
            }

            VanDriveOff[] vans =
                cityLayer.GetComponentsInChildren<VanDriveOff>(true);

            foreach (VanDriveOff van in vans)
            {
                if (van != null)
                {
                    van.ResetVan();
                }
            }
        }

        if (placementLayer != null)
        {
            BuildingDarkFilter[] filters =
                placementLayer.GetComponentsInChildren<BuildingDarkFilter>(true);

            foreach (BuildingDarkFilter filter in filters)
            {
                if (filter != null)
                {
                    filter.ResetFilter();
                }
            }

            ElevatorDoorBrokenAnimation[] elevators =
                placementLayer.GetComponentsInChildren<ElevatorDoorBrokenAnimation>(true);

            foreach (ElevatorDoorBrokenAnimation elevator in elevators)
            {
                if (elevator != null)
                {
                    elevator.ResetElevatorDoors();
                }
            }

            VanDriveOff[] vans =
                placementLayer.GetComponentsInChildren<VanDriveOff>(true);

            foreach (VanDriveOff van in vans)
            {
                if (van != null)
                {
                    van.ResetVan();
                }
            }
        }

        if (brokenCityPieces != null)
        {
            brokenCityPieces.ResetCity();
        }
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

    private void ResetAllPlacementDraggables()
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

    private void ResetAllPlacementTargets()
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
}