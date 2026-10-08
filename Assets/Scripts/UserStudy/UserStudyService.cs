using System.IO;
using UnityEngine;

[DisallowMultipleComponent]
public class UserStudyService : MonoBehaviour
{
    public string studyFolderPath = "";
    public string fileNamePrefix = "Participant_";
    public string participantIdPrefix = "P";

    private readonly StudySessionRecorder recorder = new StudySessionRecorder();

    private ParticipantIdGenerator idGenerator;
    private StudyCsvWriter writer;
    private string folderInUse = "";

    public bool IsStudyRun { 
        get; 
        private set; 
    }

    public string ParticipantId { 
        get; 
        private set; 
    } = "";

    private void Awake()
    {
        Setup();
    }

    private void Setup()
    {
        if (writer != null)
        {
            return;
        }

        string configured = studyFolderPath == null ? "" : studyFolderPath.Trim();

        if (configured.Length == 0)
        {
            folderInUse = Path.Combine(Application.persistentDataPath, "UserStudyData");
            Debug.LogWarning("[UserStudy] No Box Drive folder is set. Data is being written to " +
                folderInUse + " instead.");
        }
        else
        {
            folderInUse = configured;
        }

        idGenerator = new ParticipantIdGenerator(folderInUse, "participant_ids.txt", participantIdPrefix);
        writer = new StudyCsvWriter(folderInUse, fileNamePrefix);
    }

    public void SetStudyRun(bool isStudyRun)
    {
        Setup();

        IsStudyRun = isStudyRun;

        if (isStudyRun)
        {
            ParticipantId = idGenerator.CreateId();
            Debug.Log("[UserStudy] Study mode ON for participant " + ParticipantId);
        }
        else
        {
            ParticipantId = "";
            Debug.Log("[UserStudy] Study mode OFF");
        }
    }

    public void ToggleStudyRun()
    {
        SetStudyRun(!IsStudyRun);
    }

    public bool BeginSession(int phaseCount)
    {
        Setup();

        if (recorder.IsSessionActive)
        {
            EndSession("Home", false);
        }

        if (string.IsNullOrEmpty(ParticipantId))
        {
            ParticipantId = idGenerator.CreateId();
        }

        if (!recorder.Begin(ParticipantId, phaseCount, IsStudyRun))
        {
            Debug.LogError("[UserStudy] Could not start a run for " + phaseCount + " phases.");
            return false;
        }

        Debug.Log("[UserStudy] Run started as " + ParticipantId + (IsStudyRun ? " (study)" : " (not part of the study)"));
        return true;
    }

    public void BeginAnalogyPhase(int phaseIndex)
    {
        recorder.BeginAnalogy(phaseIndex);
    }

    public void RegisterAnalogyMistake(int phaseIndex)
    {
        recorder.AddAnalogyMistake(phaseIndex);
    }

    public void RegisterAnalogyRepeat(int phaseIndex)
    {
        recorder.AddAnalogyRepeat(phaseIndex);
    }

    public void CompleteAnalogyPhase(int phaseIndex)
    {
        recorder.FinishAnalogy(phaseIndex);
    }

    public void BeginPlacementPhase(int phaseIndex)
    {
        recorder.BeginPlacement(phaseIndex);
    }

    public void CompletePlacementPhase(int phaseIndex)
    {
        recorder.FinishPlacement(phaseIndex);
    }

    public bool SaveSessionForHome()
    {
        return EndSession("Home", false);
    }

    public bool CompleteSession()
    {
        return EndSession("Completed", true);
    }

    private void OnApplicationQuit()
    {
        if (recorder.IsSessionActive)
        {
            EndSession("Quit", false);
        }
    }

    private bool EndSession(string reason, bool completed)
    {
        Setup();

        StudySession finished = recorder.End(reason, completed);

        if (finished == null)
        {
            return true;
        }

        if (!IsStudyRun)
        {
            ParticipantId = "";
        }

        if (writer.Save(finished))
        {
            Debug.Log("[UserStudy] Run " + finished.SessionId + " saved to " + writer.LastFilePath);
            return true;
        }

        Debug.LogError("[UserStudy] Could not save run " + finished.SessionId + ": " + writer.LastError);
        return false;
    }
}
