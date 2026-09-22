using System;
using UnityEngine;

public class StudySession
{
    public string ParticipantId;
    public string SessionId;
    public bool IsStudyRun;
    public DateTime StartedUtc;
    public DateTime EndedUtc;
    public string EndReason = "";
    public bool Completed;
    public PhaseRecord[] Analogies;
    public PhaseRecord[] Placements;

    public StudySession(string participantId, int phaseCount, bool isStudyRun)
    {
        ParticipantId = participantId;
        IsStudyRun = isStudyRun;
        SessionId = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + "_" +
            Guid.NewGuid().ToString("N").Substring(0, 6);
        StartedUtc = DateTime.UtcNow;

        Analogies = new PhaseRecord[phaseCount];
        Placements = new PhaseRecord[phaseCount];

        for (int i = 0; i < phaseCount; i++)
        {
            Analogies[i] = new PhaseRecord();
            Placements[i] = new PhaseRecord();
        }
    }

    public int PhaseCount
    {
        get { return Analogies.Length; }
    }

    public double TotalSeconds
    {
        get { return (EndedUtc - StartedUtc).TotalSeconds; }
    }

    public bool HasPhase(int phaseIndex)
    {
        return phaseIndex >= 0 && phaseIndex < Analogies.Length;
    }
}

public class PhaseRecord
{
    public bool Started;
    public bool Finished;
    public int Mistakes;
    public int Repeats;

    private double bankedSeconds;
    private double startMark;
    private bool running;

    public double Seconds
    {
        get
        {
            if (!running)
            {
                return bankedSeconds;
            }

            double current = Time.realtimeSinceStartupAsDouble - startMark;
            return bankedSeconds + (current < 0d ? 0d : current);
        }
    }

    public void Begin()
    {
        Started = true;

        if (running)
        {
            return;
        }

        startMark = Time.realtimeSinceStartupAsDouble;
        running = true;
    }

    public void Pause()
    {
        if (!running)
        {
            return;
        }

        bankedSeconds = Seconds;
        running = false;
    }

    public void Finish()
    {
        Pause();
        Started = true;
        Finished = true;
    }
}
