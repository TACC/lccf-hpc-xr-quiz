using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperCityAudioController : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource explanationAudioSource;
    // Audio clips for city analogy explanations
    public AudioClip[] explanationClips;

    [Header("Timing")]
    public float pauseBeforeAudio = 0.5f;
    public float pauseAfterAudio = 0.5f;

    public bool AnalogyAudioFinished { get; private set; }
    public bool PlacementAudioFinished { get; private set;}

    // Audio clips for placement layer explanations
    public AudioClip[] placementExplanationClips;

    [SerializeField] private SuperCityManager manager;
    [SerializeField] private SuperCityAnalogyController analogyController;
    [SerializeField] private SuperCityPlacementController placementController;

    public IEnumerator PlayPlacementAudioForCurrentPhase()
    {
        PlacementAudioFinished = false;

        if (explanationAudioSource != null &&
            placementExplanationClips != null &&
            manager.currentPhase >= 0 &&
            manager.currentPhase < placementExplanationClips.Length &&
            placementExplanationClips[manager.currentPhase] != null)
        {
            explanationAudioSource.clip = placementExplanationClips[manager.currentPhase];
            explanationAudioSource.Play();

            yield return new WaitForSeconds(placementExplanationClips[manager.currentPhase].length);
        }

        PlacementAudioFinished = true;
        placementController.OnPlacementAudioFinished();
    }

    public IEnumerator PlayAnalogyAudioForCurrentPhase()
    {
        AnalogyAudioFinished = false;

        if (explanationAudioSource != null &&
            explanationClips != null &&
            manager.currentPhase >= 0 &&
            manager.currentPhase < explanationClips.Length &&
            explanationClips[manager.currentPhase] != null)
        {
            explanationAudioSource.Stop();
            explanationAudioSource.clip = explanationClips[manager.currentPhase];
            explanationAudioSource.Play();

            Debug.Log("Playing analogy audio for phase " + manager.currentPhase);

            yield return new WaitForSeconds(explanationClips[manager.currentPhase].length);
        }

        AnalogyAudioFinished = true;

        analogyController.TryStartAnalogyTransition();
    }

    public void StopAudio()
    {
        if (explanationAudioSource != null)
        {
            explanationAudioSource.Stop();
            explanationAudioSource.clip = null;
        }
    }
}
