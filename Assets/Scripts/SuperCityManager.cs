using System.Collections;
using UnityEngine;

// SuperCityManager controls changing between scenes

// Plays intro, city analogy phases, audio, city animations, sliding the 
// analogies away, and placement layers
public class SuperCityManager : MonoBehaviour
{
    public int currentPhase = 0;

    [SerializeField] private UserStudyDataManager userStudyDataManager;

    private int analogyCorrectFirstTryScore = 0;
    private bool currentAnalogyHadWrongGuess = false;
    private bool analogyScoreCountedThisPhase = false;

    [Header("Controllers")]
    [SerializeField] private SuperCityAnalogyController analogyController;
    [SerializeField] private SuperCityPlacementController placementController;
    [SerializeField] private SuperCityAudioController audioController;
    [SerializeField] private SuperCityReplayController replayController;
    [SerializeField] private SuperCityFinalSceneController finalSceneController;
    [SerializeField] private SuperCitySceneStateController sceneStateController;
    [SerializeField] private SuperCityAnimationController animationController;
    [SerializeField] private SuperCityTransitionController transitionController;

    // Runs once when the scene begins
    void Start()
    {
        analogyController.Initialize();
        placementController.Initialize();
        sceneStateController.InitializeHomeStates();

        HideAll();
    }

    public void BeginQuiz()
    {
        if (userStudyDataManager == null)
        {
            if (!userStudyDataManager.BeginSession(analogyController.NumberOfPhases))
            {
                Debug.LogError("Could not begin the user study session.");
            }
        } 
        else 
        {
             Debug.LogError("UserStudyDataManager is not assigned.");
        }

        replayController.ResetEntireQuiz();
        HideAll();

        currentPhase = 0;
        PhaseTransitionRunning = false;
        ResetAnalogyAttemptState();

        analogyController.StartAnalogyPhase(0);
        analogyController.ResetCurrentAnalogyChoices();
        StartCoroutine(audioController.PlayAnalogyAudioForCurrentPhase());
    }

    // Hides every major layer and phase object
    private void HideAll()
    {
        if (analogyController.cityLayer != null)
        {
            analogyController.cityLayer.SetActive(false);
        }

        if (placementController.placementLayer != null)
        {
            placementController.placementLayer.SetActive(false);
        }

        if (analogyController.repairedCityModel != null)
        {
            analogyController.repairedCityModel.SetActive(false);
        }

        analogyController.HideAllCityAnalogies();
        placementController.HideAllPlacementGroups();
        finalSceneController.HideFinalCelebrationObjects();
    }

    // Called when the user selects the correct answer in the city analogy layer
    public void OnAnalogySolved()
    {
        if (PhaseTransitionRunning)
        {
            return;
        }

        if (userStudyDataManager != null)
        {
            userStudyDataManager.CompleteAnalogyPhase(currentPhase);
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

        analogyController.OnAnalogySolved();
    }

    public void OnAnalogyWrongGuess()
    {
        if (userStudyDataManager != null)
        {
            userStudyDataManager.RegisterAnalogyMistake(currentPhase);
        }

        currentAnalogyHadWrongGuess = true;
        Debug.Log("Wrong analogy guess. This phase no longer counts as first try.");
    }

    // Called when the hardware placement is finished
    public void OnHardwarePlaced()
    {
        if (PhaseTransitionRunning)
        {
            return;
        }

        if (userStudyDataManager != null)
        {
            userStudyDataManager.CompletePlacementPhase(currentPhase);
        }

        placementController.OnHardwarePlaced();
    }

    public void ReturnToHome()
    {
        if (userStudyDataManager != null)
        {
            bool saved = userStudyDataManager.SaveSessionForHome();
            if (!saved)
            {
                Debug.LogError("User study session could not be saved.");
            }
        }

        replayController.ResetEntireQuiz();
        HideAll();

        Debug.Log("Quiz reset and returned home.");
    }

    public int AnalogyCorrectFirstTryScore 
    {
        get { return analogyCorrectFirstTryScore; }
    }

    public bool PhaseTransitionRunning { get; set; } = false;

    public UserStudyDataManager StudyData
    {
        get { return userStudyDataManager; }
    }

    public void ResetAnalogyAttemptState()
    {
        currentAnalogyHadWrongGuess = false;
        analogyScoreCountedThisPhase = false;
    }

    public void ResetGameState()
    {
        currentPhase = 0;
        analogyCorrectFirstTryScore = 0;
        currentAnalogyHadWrongGuess = false;
        analogyScoreCountedThisPhase = false;
        PhaseTransitionRunning = false;
    }

    public void ReplayCurrentScene()
    {
        replayController.ReplayCurrentScene();
    }
}