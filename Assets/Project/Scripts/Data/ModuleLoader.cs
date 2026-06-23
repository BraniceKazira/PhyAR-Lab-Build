// ModuleLoader.cs
// Place in: Assets/_Project/Scripts/Data/
//
// Singleton MonoBehaviour. Loads JSON files from the Resources folder.
// Any controller script calls ModuleLoader.Instance.LoadXxx() to get data.
//
// JSON files must be placed in: Assets/Resources/Data/
//   Topics:      Resources/Data/topics.json
//   Learn:       Resources/Data/Learn/learn_{topicid}.json
//   Flashcards:  Resources/Data/Flashcards/flashcards_{topicid}.json
//   Quiz:        Resources/Data/Quiz/quiz_{topicid}.json
//
// Resources.Load path is relative to the Resources folder,
// no file extension, no "Resources/" prefix.

using UnityEngine;
using System.Collections.Generic;

public class ModuleLoader : MonoBehaviour
{
    // ── Singleton setup ───────────────────────────────────────────────
    public static ModuleLoader Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // survives scene loads if needed
    }

    // ── Simple in-memory cache (avoids re-reading same file twice) ────
    private Dictionary<string, object> _cache = new Dictionary<string, object>();

    // ── PUBLIC LOAD METHODS ───────────────────────────────────────────

    /// <summary>
    /// Returns the list of all 5 topics (full and shell).
    /// Reads: Resources/Data/topics.json
    /// </summary>
    public TopicListWrapper LoadTopicList()
    {
        const string key = "topics";
        if (_cache.ContainsKey(key)) return (TopicListWrapper)_cache[key];

        var data = LoadJson<TopicListWrapper>("Data/topics");
        if (data != null) _cache[key] = data;
        return data;
    }

    /// <summary>
    /// Returns learn content for a specific topic.
    /// Reads: Resources/Data/Learn/learn_{topicID}.json
    /// Example: LoadLearnData("em_induction")
    /// </summary>
    public LearnData LoadLearnData(string topicID)
    {
        string key = "learn_" + topicID;
        if (_cache.ContainsKey(key)) return (LearnData)_cache[key];

        var data = LoadJson<LearnData>($"Data/Learn/learn_{topicID}");
        if (data != null) _cache[key] = data;
        return data;
    }

    /// <summary>
    /// Returns the full flashcard deck for a topic.
    /// Reads: Resources/Data/Flashcards/flashcards_{topicID}.json
    /// </summary>
    public FlashcardDeckData LoadFlashcards(string topicID)
    {
        string key = "flashcards_" + topicID;
        if (_cache.ContainsKey(key)) return (FlashcardDeckData)_cache[key];

        var data = LoadJson<FlashcardDeckData>($"Data/Flashcards/flashcards_{topicID}");
        if (data != null) _cache[key] = data;
        return data;
    }

    /// <summary>
    /// Returns the quiz data for a topic.
    /// Reads: Resources/Data/Quiz/quiz_{topicID}.json
    /// </summary>
    public QuizData LoadQuiz(string topicID)
    {
        string key = "quiz_" + topicID;
        if (_cache.ContainsKey(key)) return (QuizData)_cache[key];

        var data = LoadJson<QuizData>($"Data/Quiz/quiz_{topicID}");
        if (data != null) _cache[key] = data;
        return data;
    }

    // ── PRIVATE HELPER ────────────────────────────────────────────────

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