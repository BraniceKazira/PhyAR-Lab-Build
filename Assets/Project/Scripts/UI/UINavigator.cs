// UINavigator.cs — Final version with guaranteed overlap fix

using UnityEngine;
using System.Collections.Generic;

public class UINavigator : MonoBehaviour
{
    public static UINavigator Instance { get; private set; }

    [System.Serializable]
    public class ScreenEntry
    {
        public string     screenName;
        public GameObject panel;
    }

    public ScreenEntry[] screens;
    public string        initialScreen = "Screen_Splash";

    private Dictionary<string, GameObject> _map;
    private string _current  = "";
    private string _previous = "";

    public const string SCREEN_SPLASH       = "Screen_Splash";
    public const string SCREEN_HOME         = "Screen_Home";
    public const string SCREEN_LEARN        = "Screen_Learn";
    public const string SCREEN_AR           = "Screen_ARExperience";
    public const string SCREEN_FLASHCARD    = "Screen_Flashcards";
    public const string SCREEN_FC_SUMMARY   = "Screen_FlashcardSummary";
    public const string SCREEN_QUIZ_Q       = "Screen_QuizQuestion";
    public const string SCREEN_QUIZ_FB      = "Screen_QuizFeedback";
    public const string SCREEN_QUIZ_RESULTS = "Screen_QuizResults";
    public const string SCREEN_PROGRESS     = "Screen_Progress";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        _map = new Dictionary<string, GameObject>();
        foreach (var e in screens)
            if (e.panel != null)
                _map[e.screenName] = e.panel;
            else
                Debug.LogWarning($"[Nav] '{e.screenName}' has no panel.");

        // Hide every panel unconditionally at start
        foreach (var p in _map.Values) p.SetActive(false);

        ShowScreen(initialScreen);
    }

    public void ShowScreen(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogWarning("[Nav] ShowScreen called with empty name.");
            return;
        }
        if (!_map.ContainsKey(name))
        {
            Debug.LogError($"[Nav] '{name}' not found. Available: {string.Join(", ", _map.Keys)}");
            return;
        }

        // ── OVERLAP FIX: hide ALL panels before showing the new one ──
        foreach (var kvp in _map)
            kvp.Value.SetActive(false);

        // Store the previous screen for GoBack, but ONLY if we're not going to Progress.
        // If we're going to Progress, we want to prevent GoBack from returning to Quiz Results.
        if (name != SCREEN_PROGRESS)
            _previous = _current;

        _current = name;
        _map[name].SetActive(true);

        Debug.Log("[Nav] → " + name);
    }

    public void OpenTopic(string topicID)
    {
        AppState.CurrentTopicID = topicID;
        Debug.Log("[Nav] Topic = " + topicID);
        ShowScreen(SCREEN_LEARN);
    }

    public void GoBack()
    {
        // ── OVERLAP FIX: if we're on Progress, go to Home, not back to Quiz Results ──
        if (_current == SCREEN_PROGRESS)
        {
            ShowScreen(SCREEN_HOME);
            return;
        }

        if (!string.IsNullOrEmpty(_previous))
            ShowScreen(_previous);
        else
            ShowScreen(SCREEN_HOME);
    }

    public void GoHome()
    {
        AppState.ResetSession();
        ShowScreen(SCREEN_HOME);
    }
}