using System;

public class StudySessionRecorder
{
    private StudySession session;
    private PhaseRecord runningPhase;

    public StudySession ActiveSession
    {
        get { 
            return session; 
        }
    }

    public bool IsSessionActive
    {
        get { 
            return session != null; 
        }
    }

    public bool Begin(string participantId, int phaseCount, bool isStudyRun)
    {
        if (session != null || string.IsNullOrEmpty(participantId) || phaseCount <= 0)
        {
            return false;
        }

        session = new StudySession(participantId, phaseCount, isStudyRun);
        runningPhase = null;
        return true;
    }

    public void BeginAnalogy(int phaseIndex)
    {
        StartPhase(GetAnalogy(phaseIndex));
    }

    public void FinishAnalogy(int phaseIndex)
    {
        FinishPhase(GetAnalogy(phaseIndex));
    }

    public void AddAnalogyMistake(int phaseIndex)
    {
        PhaseRecord phase = GetAnalogy(phaseIndex);

        if (phase != null && phase.Started && !phase.Finished)
        {
            phase.Mistakes++;
        }
    }

    public void AddAnalogyRepeat(int phaseIndex)
    {
        PhaseRecord phase = GetAnalogy(phaseIndex);

        if (phase != null && phase.Started && !phase.Finished)
        {
            phase.Repeats++;
        }
    }

    public void BeginPlacement(int phaseIndex)
    {
        StartPhase(GetPlacement(phaseIndex));
    }

    public void FinishPlacement(int phaseIndex)
    {
        FinishPhase(GetPlacement(phaseIndex));
    }

    public StudySession End(string reason, bool completed)
    {
        if (session == null)
        {
            return null;
        }

        PauseRunningPhase();

        StudySession finished = session;
        finished.EndedUtc = DateTime.UtcNow;
        finished.EndReason = reason;
        finished.Completed = completed;

        session = null;
        runningPhase = null;

        return finished;
    }

    private void StartPhase(PhaseRecord phase)
    {
        if (phase == null || phase.Finished)
        {
            return;
        }

        if (runningPhase != phase)
        {
            PauseRunningPhase();
        }

        phase.Begin();
        runningPhase = phase;
    }

    private void FinishPhase(PhaseRecord phase)
    {
        if (phase == null || phase.Finished)
        {
            return;
        }

        phase.Finish();

        if (runningPhase == phase)
        {
            runningPhase = null;
        }
    }

    private void PauseRunningPhase()
    {
        if (runningPhase == null)
        {
            return;
        }

        runningPhase.Pause();
        runningPhase = null;
    }

    private PhaseRecord GetAnalogy(int phaseIndex)
    {
        return session != null && session.HasPhase(phaseIndex) ? session.Analogies[phaseIndex] : null;
    }

    private PhaseRecord GetPlacement(int phaseIndex)
    {
        return session != null && session.HasPhase(phaseIndex) ? session.Placements[phaseIndex] : null;
    }
}
