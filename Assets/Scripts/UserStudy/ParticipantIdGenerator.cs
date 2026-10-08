using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class ParticipantIdGenerator
{
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int RandomAttempts = 25;
    private const int SuffixLength = 6;

    private readonly string filePath;
    private readonly string prefix;
    private readonly HashSet<string> usedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private readonly System.Random random = new System.Random();

    public ParticipantIdGenerator(string folderPath, string fileName, string idPrefix)
    {
        filePath = Path.Combine(folderPath, fileName);
        prefix = string.IsNullOrEmpty(idPrefix) ? "P" : idPrefix;

        Load();
    }

    public string CreateId()
    {
        for (int attempt = 0; attempt < RandomAttempts; attempt++)
        {
            string candidate = BuildRandomId();

            if (!usedIds.Contains(candidate))
            {
                return Remember(candidate);
            }
        }

        int counter = usedIds.Count + 1;
        string fallback;

        do
        {
            fallback = prefix + "-" + counter.ToString("D5");
            counter++;
        }
        while (usedIds.Contains(fallback));

        return Remember(fallback);
    }

    private string BuildRandomId()
    {
        StringBuilder builder = new StringBuilder();

        builder.Append(prefix);
        builder.Append('-');

        for (int i = 0; i < SuffixLength; i++)
        {
            builder.Append(Alphabet[random.Next(Alphabet.Length)]);
        }

        return builder.ToString();
    }

    private string Remember(string participantId)
    {
        usedIds.Add(participantId);

        try
        {
            string folder = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            File.AppendAllText(filePath, participantId + Environment.NewLine);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("[UserStudy] Could not update the participant ID list: " + exception.Message);
        }

        return participantId;
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            foreach (string line in File.ReadAllLines(filePath))
            {
                if (!string.IsNullOrEmpty(line) && line.Trim().Length > 0)
                {
                    usedIds.Add(line.Trim());
                }
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning("[UserStudy] Could not read the participant ID list: " + exception.Message);
        }
    }
}
