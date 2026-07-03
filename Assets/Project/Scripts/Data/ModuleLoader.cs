// ModuleLoader.cs
// Attach to: AppManagers GameObject

using UnityEngine;
using System.Collections.Generic;

public class ModuleLoader : MonoBehaviour
{
    public static ModuleLoader Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // ── TEMPORARY DIAGNOSTIC — tells us exactly what Unity can see ──
        RunDiagnostic();
    }

    void RunDiagnostic()
    {
        Debug.Log("════════════ RESOURCES DIAGNOSTIC ════════════");

        TextAsset[] allInLearn = Resources.LoadAll<TextAsset>("Data/Learn");
        Debug.Log($"Files found in Resources/Data/Learn: {allInLearn.Length}");
        foreach (var asset in allInLearn)
            Debug.Log("  → FOUND: " + asset.name);

        TextAsset topicsCheck = Resources.Load<TextAsset>("Data/topics");
        Debug.Log("Data/topics found: " + (topicsCheck != null));

        TextAsset[] allInData = Resources.LoadAll<TextAsset>("Data");
        Debug.Log($"Files found directly in Resources/Data: {allInData.Length}");
        foreach (var asset in allInData)
            Debug.Log("  → FOUND: " + asset.name);

        Debug.Log("════════════ END DIAGNOSTIC ════════════");
    }

    private Dictionary<string, object> _cache = new Dictionary<string, object>();

    public TopicListWrapper LoadTopicList()
    {
        const string key = "topics";
        if (_cache.ContainsKey(key)) return (TopicListWrapper)_cache[key];
        var data = LoadJson<TopicListWrapper>("Data/topics");
        if (data != null) _cache[key] = data;
        return data;
    }

    public LearnData LoadLearnData(string topicID)
    {
        string key = "learn_" + topicID;
        if (_cache.ContainsKey(key)) return (LearnData)_cache[key];
        var data = LoadJson<LearnData>($"Data/Learn/learn_{topicID}");
        if (data != null) _cache[key] = data;
        return data;
    }

    public FlashcardDeckData LoadFlashcards(string topicID)
    {
        string key = "flashcards_" + topicID;
        if (_cache.ContainsKey(key)) return (FlashcardDeckData)_cache[key];
        var data = LoadJson<FlashcardDeckData>($"Data/FlashCards/flashcards_{topicID}");
        if (data != null) _cache[key] = data;
        return data;
    }

    public QuizData LoadQuiz(string topicID)
    {
        string key = "quiz_" + topicID;
        if (_cache.ContainsKey(key)) return (QuizData)_cache[key];
        var data = LoadJson<QuizData>($"Data/Quiz/quiz_{topicID}");
        if (data != null) _cache[key] = data;
        return data;
    }

    private T LoadJson<T>(string resourcePath) where T : class
    {
        TextAsset textAsset = Resources.Load<TextAsset>(resourcePath);
        if (textAsset == null)
        {
            Debug.LogError($"[ModuleLoader] JSON file not found at Resources/{resourcePath}.json");
            return null;
        }
        T result = JsonUtility.FromJson<T>(textAsset.text);
        if (result == null)
            Debug.LogError($"[ModuleLoader] Failed to parse JSON at Resources/{resourcePath}.json");
        return result;
    }
}