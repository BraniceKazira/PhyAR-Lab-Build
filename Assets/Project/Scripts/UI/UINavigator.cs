using UnityEngine;
using System.Collections.Generic;

public class UINavigator : MonoBehaviour
{
    public static UINavigator Instance { get; private set; }

    [System.Serializable]
    public class ScreenEntry
    {
        public string screenName;
        public GameObject panel;
    }

    [Header("Register all screen panels here")]
    public ScreenEntry[] screens;

    [Header("Screen name to show on startup")]
    public string initialScreen = "Screen_Splash";

    private Dictionary<string, GameObject> _map;
    private string _current = "";
    private string _previous = "";

    // ── Constants ──
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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _map = new Dictionary<string, GameObject>();
        foreach (var entry in screens)
        {
            if (entry.panel != null)
                _map[entry.screenName] = entry.panel;
            else
                Debug.LogWarning($"[UINavigator] '{entry.screenName}' has no panel assigned.");
        }

        // Hide ALL panels first
        foreach (var panel in _map.Values)
        {
            if (panel != null)
                panel.SetActive(false);
        }

        ShowScreen(initialScreen);
    }

    // ── Core navigation method ──
    public void ShowScreen(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogWarning("[UINavigator] ShowScreen called with empty name.");
            return;
        }

        if (!_map.ContainsKey(name))
        {
            Debug.LogError($"[UINavigator] Screen '{name}' not found. Available: {string.Join(", ", _map.Keys)}");
            return;
        }

        // ✅ CRITICAL FIX: Hide ALL screens first
        foreach (var panel in _map.Values)
        {
            if (panel != null)
                panel.SetActive(false);
        }

        // Then show the target
        _previous = _current;
        _current = name;
        _map[name].SetActive(true);

        Debug.Log($"[UINavigator] → {name}");
    }

    // ── Convenience methods ──
    public void OpenTopic(string topicID)
    {
        AppState.CurrentTopicID = topicID;
        ShowScreen(SCREEN_LEARN);
    }

    public void GoBack()
    {
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