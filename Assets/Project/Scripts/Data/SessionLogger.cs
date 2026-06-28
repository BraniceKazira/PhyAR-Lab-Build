// SessionLogger.cs
// Place in: Assets/_Project/Scripts/Data/
// Attach to: a persistent GameObject (e.g. your ModuleLoader or UINavigator GameObject)
//
// Saves each quiz session to device storage as JSON.
// Teacher panel calls ExportAllToDownloads() to write files externally.

using UnityEngine;
using System;
using System.IO;

public class SessionLogger : MonoBehaviour
{
    // ── File path ─────────────────────────────────────────────────────
    // Application.persistentDataPath on Android = /data/data/{package}/files/
    // This is internal storage — only accessible by this app and ADB.
    private string SavePath => Path.Combine(Application.persistentDataPath, "session_log.json");

    // ── Load existing logs ────────────────────────────────────────────
    public SessionLogList LoadAllSessions()
    {
        if (!File.Exists(SavePath))
            return new SessionLogList();

        string json = File.ReadAllText(SavePath);
        return JsonUtility.FromJson<SessionLogList>(json) ?? new SessionLogList();
    }

    // ── Save a new session ────────────────────────────────────────────
    public void LogSession(string topicID,
                           int fcKnown, int fcStillLearning, int fcSkipped,
                           int quizScore, int quizTotal)
    {
        SessionLogList logList = LoadAllSessions();

        SessionLog entry = new SessionLog
        {
            sessionID          = Guid.NewGuid().ToString(),
            deviceID           = SystemInfo.deviceUniqueIdentifier,
            topicID            = topicID,
            dateTime           = DateTime.UtcNow.ToString("o"), // ISO 8601
            durationSeconds    = 0, // wire up a timer if needed
            flashcardsKnown    = fcKnown,
            flashcardsStillLearning = fcStillLearning,
            flashcardsSkipped  = fcSkipped,
            quizScore          = quizScore,
            quizTotal          = quizTotal
        };

        logList.sessions.Add(entry);

        string json = JsonUtility.ToJson(logList, prettyPrint: true);
        File.WriteAllText(SavePath, json);

        Debug.Log($"[SessionLogger] Session saved: {topicID} | Quiz {quizScore}/{quizTotal}");
    }

    // ── Export to Downloads ───────────────────────────────────────────
    // Called by the teacher panel "Export all sessions" button.
    // On Android, writes to /sdcard/Download/ which is accessible via USB.
    public void ExportAllToDownloads()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("[SessionLogger] No session log found to export.");
            return;
        }

        // On Android, /sdcard/Download is the public Downloads folder
        string downloadsPath = "/sdcard/Download";

        // Fallback for non-Android (editor testing)
        if (Application.platform != RuntimePlatform.Android)
            downloadsPath = Application.persistentDataPath;

        string fileName    = $"PhyAR_Sessions_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        string destination = Path.Combine(downloadsPath, fileName);

        try
        {
            File.Copy(SavePath, destination, overwrite: true);
            Debug.Log($"[SessionLogger] Exported to: {destination}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SessionLogger] Export failed: {e.Message}");
        }
    }

    // ── Get summary for progress dashboard ───────────────────────────
    public (int topicsDone, int quizzesTaken, float avgScore, int totalCards)
        GetProgressSummary()
    {
        SessionLogList logList = LoadAllSessions();
        if (logList.sessions.Count == 0)
            return (0, 0, 0f, 0);

        int quizzesTaken = logList.sessions.Count;
        float totalScore = 0;
        foreach (var s in logList.sessions)
            totalScore += (float)s.quizScore / Mathf.Max(s.quizTotal, 1);

        float avg = totalScore / quizzesTaken;

        // Count unique topics that have quiz scores >= 50%
        var doneTopics = new System.Collections.Generic.HashSet<string>();
        foreach (var s in logList.sessions)
        {
            if ((float)s.quizScore / Mathf.Max(s.quizTotal, 1) >= 0.5f)
                doneTopics.Add(s.topicID);
        }

        int totalCards = 0;
        foreach (var s in logList.sessions)
            totalCards += s.flashcardsKnown;

        return (doneTopics.Count, quizzesTaken, avg, totalCards);
    }
}