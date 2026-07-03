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

    private Dictionary<string, GameObject> _screenMap;
    private string _currentScreen;
    private string _previousScreen;

    // ── Constants — these now match the "Screen_" naming you typed in the Inspector ──
    public const string SCREEN_SPLASH        = "Screen_Splash";
    public const string SCREEN_HOME          = "Screen_Home";
    public const string SCREEN_LEARN         = "Screen_Learn";
    public const string SCREEN_AR            = "Screen_ARExperience";
    public const string SCREEN_FLASHCARD     = "Screen_Flashcards";
    public const string SCREEN_FC_SUMMARY    = "Screen_FlashcardSummary";
    public const string SCREEN_QUIZ_Q        = "Screen_QuizQuestion";
    public const string SCREEN_QUIZ_FB       = "Screen_QuizFeedback";
    public const string SCREEN_QUIZ_RESULTS  = "Screen_QuizResults";
    public const string SCREEN_PROGRESS      = "Screen_Progress";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        _screenMap = new Dictionary<string, GameObject>();
        foreach (var entry in screens)
        {
            if (entry.panel != null)
                _screenMap[entry.screenName] = entry.panel;
            else
                Debug.LogWarning($"[UINavigator] '{entry.screenName}' has no panel assigned.");
        }

        // Hide ALL panels first
        foreach (var panel in _screenMap.Values)
            panel.SetActive(false);

        ShowScreen(initialScreen);
    }

    public void ShowScreen(string screenName)
    {
        // Guard against empty string calls from unwired buttons
        if (string.IsNullOrEmpty(screenName))
        {
            Debug.LogWarning("[UINavigator] ShowScreen called with empty name — check your button OnClick string fields.");
            return;
        }

        if (!_screenMap.ContainsKey(screenName))
        {
            Debug.LogError($"[UINavigator] Screen '{screenName}' not found. " +
                           $"Available screens: {string.Join(", ", _screenMap.Keys)}");
            return;
        }

        if (!string.IsNullOrEmpty(_currentScreen) && _screenMap.ContainsKey(_currentScreen))
            _screenMap[_currentScreen].SetActive(false);

        _previousScreen = _currentScreen;
        _currentScreen  = screenName;
        _screenMap[screenName].SetActive(true);

        Debug.Log($"[UINavigator] → {screenName}");
    }

    // Called by TopicCardUI when a topic card is tapped
    public void OpenTopic(string topicID)
    {
        AppState.CurrentTopicID = topicID;
        ShowScreen(SCREEN_LEARN);
    }

    public void GoBack()
    {
        if (!string.IsNullOrEmpty(_previousScreen))
            ShowScreen(_previousScreen);
    }

    public void GoHome()
    {
        AppState.ResetSession();
        ShowScreen(SCREEN_HOME);
    }
}