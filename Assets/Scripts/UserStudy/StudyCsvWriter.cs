using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

public class StudyCsvWriter
{
    private static readonly UTF8Encoding Utf8NoBom = new UTF8Encoding(false);

    private readonly string folderPath;
    private readonly string filePrefix;

    public StudyCsvWriter(string folderPath, string filePrefix)
    {
        this.folderPath = folderPath;
        this.filePrefix = string.IsNullOrEmpty(filePrefix) ? "Participant_" : filePrefix;
    }

    public string LastFilePath { 
        get; 
        private set; 
    } = "";

    public string LastError { 
        get; 
        private set; 
    } = "";

    public bool Save(StudySession session)
    {
        LastError = "";

        if (session == null)
        {
            LastError = "There was no session to save.";
            return false;
        }

        if (string.IsNullOrEmpty(folderPath))
        {
            LastError = "No study folder has been set.";
            return false;
        }

        try
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string path = Path.Combine(folderPath, filePrefix + session.ParticipantId + ".csv");

            if (!File.Exists(path) || new FileInfo(path).Length == 0)
            {
                File.WriteAllText(path, BuildHeader(session.PhaseCount) + Environment.NewLine, Utf8NoBom);
            }

            File.AppendAllText(path, BuildRow(session) + Environment.NewLine, Utf8NoBom);

            LastFilePath = path;
            return true;
        }
        catch (Exception exception)
        {
            LastError = exception.Message;
            return false;
        }
    }

    private string BuildHeader(int phaseCount)
    {
        List<string> fields = new List<string>
        {
            "ParticipantID",
            "SessionID",
            "StudyRun",
            "StartUTC",
            "EndUTC",
            "TotalSeconds",
            "EndReason",
            "CompletedQuiz"
        };

        for (int i = 1; i <= phaseCount; i++)
        {
            fields.Add("Analogy" + i + "_Seconds");
            fields.Add("Analogy" + i + "_Mistakes");
            fields.Add("Analogy" + i + "_Repeats");
            fields.Add("Analogy" + i + "_Solved");
        }

        for (int i = 1; i <= phaseCount; i++)
        {
            fields.Add("Placement" + i + "_Seconds");
            fields.Add("Placement" + i + "_Placed");
        }

        return Join(fields);
    }

    private string BuildRow(StudySession session)
    {
        List<string> fields = new List<string>
        {
            session.ParticipantId,
            session.SessionId,
            Flag(session.IsStudyRun),
            session.StartedUtc.ToString("o", CultureInfo.InvariantCulture),
            session.EndedUtc.ToString("o", CultureInfo.InvariantCulture),
            Seconds(session.TotalSeconds),
            session.EndReason,
            Flag(session.Completed)
        };

        foreach (PhaseRecord phase in session.Analogies)
        {
            if (!phase.Started)
            {
                fields.Add("");
                fields.Add("");
                fields.Add("");
                fields.Add("");
                continue;
            }

            fields.Add(Seconds(phase.Seconds));
            fields.Add(phase.Mistakes.ToString(CultureInfo.InvariantCulture));
            fields.Add(phase.Repeats.ToString(CultureInfo.InvariantCulture));
            fields.Add(Flag(phase.Finished));
        }

        foreach (PhaseRecord phase in session.Placements)
        {
            if (!phase.Started)
            {
                fields.Add("");
                fields.Add("");
                continue;
            }

            fields.Add(Seconds(phase.Seconds));
            fields.Add(Flag(phase.Finished));
        }

        return Join(fields);
    }

    private static string Flag(bool value)
    {
        return value ? "TRUE" : "FALSE";
    }

    private static string Seconds(double value)
    {
        return value.ToString("F3", CultureInfo.InvariantCulture);
    }

    private static string Join(List<string> fields)
    {
        for (int i = 0; i < fields.Count; i++)
        {
            fields[i] = Escape(fields[i]);
        }

        return string.Join(",", fields.ToArray());
    }

    private static string Escape(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        if (value.IndexOf(',') >= 0 || value.IndexOf('"') >= 0 ||
            value.IndexOf('\n') >= 0 || value.IndexOf('\r') >= 0)
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        return value;
    }
}
