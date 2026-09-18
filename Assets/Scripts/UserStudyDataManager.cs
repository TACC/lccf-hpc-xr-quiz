using System;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class UserStudyDataManager : MonoBehaviour
{
    [SerializeField] private string studyFolderPath;

    private string participantId = "";
    private SessionData currentSession;
    private bool sessionActive;

    private bool analogyTimerRunning;
    private int activeAnalogyPhase = -1;
    private float analogyStartTime;

    private bool placementTimerRunning;
    private int activePlacementPhase = -1;
    private float placementStartTime;

    public bool SetParticipantId(string rawId)
    {
        if (string.IsNullOrWhiteSpace(rawId))
        {
            return false;
        }
        string cleanedId = rawId.Trim();
        foreach (char c in cleanedId)
        {
            if (!char.IsLetterOrDigit(c) && c != '-' && c != '_') 
            {
                return false;
            }
        }
        participantId = cleanedId;
        return true;
    }

    public bool BeginSession(int numberOfPhases) 
    {
        if (string.IsNullOrEmpty(participantId))
        {
            Debug.LogError("Participant ID has not been entered.");
            return false;
        }

        if (sessionActive)
        {
            Debug.LogError("A session is already active.");
            return false;
        }

        if (numberOfPhases <= 0)
        {
            Debug.LogError("Invalid number of study phases.");
            return false;
        }

        currentSession = new SessionData(participantId, numberOfPhases);
        sessionActive = true;

        ResetRuntimeTimers();

        Debug.Log("Started session " + currentSession.sessionId);
        return true;
    }

    public void BeginAnalogyPhase(int phaseIndex)
    {
        if (!SessionAndPhaseAreValid(phaseIndex))
        {
            return;
        }
        if (currentSession.analogyCompleted[phaseIndex])
        {
            return;
        }
        if (analogyTimerRunning && activeAnalogyPhase == phaseIndex)
        {
            return;
        }
        if (analogyTimerRunning)
        {
            CaptureCurrentAnalogyTime();
        }
        currentSession.analogyStarted[phaseIndex] = true;
        activeAnalogyPhase = phaseIndex;
        analogyStartTime = Time.realtimeSinceStartup;
        analogyTimerRunning = true;
    }

    public void RegisterAnalogyMistake(int phaseIndex)
    {
        if (!SessionAndPhaseAreValid(phaseIndex))
        {
            return;
        }
        if (!currentSession.analogyStarted[phaseIndex] || currentSession.analogyCompleted[phaseIndex])
        {
            return;
        }
        currentSession.analogyMistakes[phaseIndex]++;
    }

    public void RegisterAnalogyRepeat(int phaseIndex)
    {
        if (!SessionAndPhaseAreValid(phaseIndex))
        {
            return;
        }
        if (!currentSession.analogyStarted[phaseIndex] || currentSession.analogyCompleted[phaseIndex])
        {
            return;
        }
        currentSession.analogyRepeats[phaseIndex]++;
    }

    public void CompleteAnalogyPhase(int phaseIndex)
    {
        if (!SessionAndPhaseAreValid(phaseIndex))
        {
            return;
        }
        if (currentSession.analogyCompleted[phaseIndex])
        {
            return;
        }
        if (analogyTimerRunning && activeAnalogyPhase == phaseIndex)
        {
            currentSession.analogyTimes[phaseIndex] = 
                Mathf.Max(0f, Time.realtimeSinceStartup - analogyStartTime);
            analogyTimerRunning = false;
            activeAnalogyPhase = -1;
        } 
        else if (currentSession.analogyTimes[phaseIndex] < 0f)
        {
            currentSession.analogyTimes[phaseIndex] = 0f;
        }
        currentSession.analogyStarted[phaseIndex] = true;
        currentSession.analogyCompleted[phaseIndex] = true;
    }

    public void BeginPlacementPhase(int phaseIndex)
    {
        if (!SessionAndPhaseAreValid(phaseIndex))
        {
            return;
        }
        if (currentSession.placementCompleted[phaseIndex])
        {
            return;
        }
        if (placementTimerRunning && activePlacementPhase == phaseIndex)
        {
            return;
        }
        if (placementTimerRunning)
        {
            CaptureCurrentPlacementTime();
        }
        currentSession.placementStarted[phaseIndex] = true;
        activePlacementPhase = phaseIndex;
        placementStartTime = Time.realtimeSinceStartup;
        placementTimerRunning = true;
    }

    public void CompletePlacementPhase(int phaseIndex)
    {
        if (!SessionAndPhaseAreValid(phaseIndex))
        {
            return;
        }
        if (currentSession.placementCompleted[phaseIndex])
        {
            return;
        }
        if (placementTimerRunning && activePlacementPhase == phaseIndex)
        {
            currentSession.placementTimes[phaseIndex] = 
                Mathf.Max(0f, Time.realtimeSinceStartup - placementStartTime);
            placementTimerRunning = false;
            activePlacementPhase = -1;
        }
        else if (currentSession.placementTimes[phaseIndex] < 0f)
        {
            currentSession.placementTimes[phaseIndex] = 0f;
        }
        currentSession.placementStarted[phaseIndex] = true;
        currentSession.placementCompleted[phaseIndex] = true;
    }

    public bool SaveSessionForHome()
    {
        return EndSession("Home", false);
    }

    public bool CompleteSession()
    {
        return EndSession("completed", true);
    }

    private bool EndSession(string reason, bool completed)
    {
        if (!sessionActive || currentSession == null)
        {
            return true;
        }

        CaptureCurrentAnalogyTime();
        CaptureCurrentPlacementTime();

        currentSession.endUtc = DateTime.UtcNow;
        currentSession.endReason = reason;
        currentSession.completed = completed;

        if (!WriteSessionToCsv(currentSession))
        {
            return false;
        }
        sessionActive = false;
        currentSession = null;
        ResetRuntimeTimers();
        return true;
    }

    private void CaptureCurrentAnalogyTime()
    {
        if (!sessionActive || !analogyTimerRunning || 
            currentSession == null || activeAnalogyPhase < 0)
        {
            return;
        }
        currentSession.analogyTimes[activeAnalogyPhase] = 
            Mathf.Max(0f, Time.realtimeSinceStartup - analogyStartTime);
        analogyTimerRunning = false;
        activeAnalogyPhase = -1;
    }

    private void CaptureCurrentPlacementTime()
    {
        if (!sessionActive || !placementTimerRunning || 
            currentSession == null || activePlacementPhase < 0)
        {
            return;
        }
        currentSession.placementTimes[activePlacementPhase] = 
            Mathf.Max(0f, Time.realtimeSinceStartup - placementStartTime);
        placementTimerRunning = false;
        activePlacementPhase = -1;
    }

    private bool SessionAndPhaseAreValid(int phaseIndex)
    {
        return sessionActive && currentSession != null &&
            phaseIndex >= 0 && phaseIndex < currentSession.analogyTimes.Length;
    }

    private bool WriteSessionToCsv(SessionData session)
    {
        if (string.IsNullOrWhiteSpace(studyFolderPath))
        {
            Debug.LogError("Study folder path has not been assigned.");
            return false;
        }
        if (!Directory.Exists(studyFolderPath))
        {
            Debug.LogError("Study folder does not exist: " + studyFolderPath);
            return false;
        }
        string fileName = "Participant_" + session.participantId + ".csv";
        string filePath = Path.Combine(studyFolderPath, fileName);

        try 
        {
            bool needsHeader = !File.Exists(filePath) ||
                new FileInfo(filePath).Length == 0;
            UTF8Encoding encoding = new UTF8Encoding(false);

            if (needsHeader)
            {
                File.WriteAllText (
                    filePath,
                    BuildHeader(session.analogyTimes.Length) + Environment.NewLine,
                    encoding
                );
            }
            File.AppendAllText(
                filePath, 
                BuildRow(session) + Environment.NewLine,
                encoding
            );
            Debug.Log("Study data saved to: " + filePath);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError("Could not save study data: " + exception.Message);
            return false;
        }
    }

    private string BuildHeader(int phaseCount)
    {
        List<string> fields = new List<string>
        {
            "ParticipantID",
            "SessionID",
            "SessionStartUTC",
            "SessionEndUTC",
            "EndReason",
            "Completed"
        };
        for (int i = 0; i < phaseCount; i++)
        {
            int sceneNumber = i + 1;
            fields.Add("Analogy" + sceneNumber + "_TimeSeconds");
            fields.Add("Analogy" + sceneNumber + "_Mistakes");
            fields.Add("Analogy" + sceneNumber + "_Repeats");
        }
        for (int i = 0; i < phaseCount; i++)
        {
            int sceneNumber = i + 1;
            fields.Add("Placement" + sceneNumber + "_TimeSeconds");
        }
        return string.Join(",", fields.ToArray());
    }
    private string BuildRow(SessionData session)
    {
        List<string> fields = new List<string>
        {
            EscapeCsv(session.participantId),
            EscapeCsv(session.sessionId),
            EscapeCsv(session.startUtc.ToString("o")),
            EscapeCsv(session.endUtc.ToString("o")),
            EscapeCsv(session.endReason),
            session.completed ? "TRUE" : "FALSE"
        };
        for (int i = 0; i < session.analogyTimes.Length; i++)
        {
            if (session.analogyStarted[i])
            {
                fields.Add(
                    session.analogyTimes[i].ToString(
                        "F3",
                        CultureInfo.InvariantCulture)
                );
                fields.Add(session.analogyMistakes[i].ToString(CultureInfo.InvariantCulture));
                fields.Add(session.analogyRepeats[i].ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                fields.Add("");
                fields.Add("");
                fields.Add("");
            }
        }
        for (int i = 0; i < session.placementTimes.Length; i++)
        {
            if (session.placementStarted[i])
            {
                fields.Add(session.placementTimes[i].ToString("F3", CultureInfo.InvariantCulture));
            }
            else 
            {
                fields.Add("");
            }
        }
        return string.Join(",", fields.ToArray());
    }

    private string EscapeCsv(string value)
    {
        if (value == null)
        {
            return "";
        }

        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
        return value;
    }

    private void ResetRuntimeTimers()
    {
        analogyTimerRunning = false;
        activeAnalogyPhase = -1;
        analogyStartTime = 0f;

        placementTimerRunning = false;
        activePlacementPhase = -1;
        placementStartTime = 0f;
    }

    private class SessionData
    {
        public string participantId;
        public string sessionId;
        public DateTime startUtc;
        public DateTime endUtc;
        public string endReason;

        public bool completed;

        public bool[] analogyStarted;
        public bool[] analogyCompleted;
        public float[] analogyTimes;
        public int[] analogyMistakes;
        public int[] analogyRepeats;

        public bool[] placementStarted;
        public bool[] placementCompleted;
        public float[] placementTimes;

        public SessionData(string participantId, int phaseCount)
        {
            this.participantId = participantId;
            sessionId = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff") + "_" +
                Guid.NewGuid().ToString("N").Substring(0, 8);
            startUtc = DateTime.UtcNow;
            endReason = "";

            analogyStarted = new bool[phaseCount];
            analogyCompleted = new bool[phaseCount];
            analogyTimes = new float[phaseCount];
            analogyMistakes = new int[phaseCount];
            analogyRepeats = new int[phaseCount];

            placementStarted = new bool[phaseCount];
            placementCompleted = new bool[phaseCount];
            placementTimes = new float[phaseCount];

            for (int i = 0; i < phaseCount; i++)
            {
                analogyTimes[i] = -1f;
                placementTimes[i] = -1f;
            }
        }
    }
}
