// UINavigator.cs
// Place in: Assets/_Project/Scripts/UI/
//
// Singleton. Manages which screen panel is currently visible.
// Every screen transition in the app goes through this one class.
//
// HOW TO WIRE IT UP IN UNITY:
//   1. Add this script to a GameObject named "UINavigator" in your scene.
//   2. In the Inspector, set the Screens array size to 11.
//   3. For each entry, type the screen name and drag the corresponding
//      panel GameObject from the Hierarchy.
//   4. Set Initial Screen to "Splash".
//
// HOW TO USE FROM ANY OTHER SCRIPT:
//   UINavigator.Instance.ShowScreen("Home");
//   UINavigator.Instance.ShowScreen("Learn");

using UnityEngine;
using System.Collections.Generic;

public class UINavigator : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────
    public static UINavigator Instance { get; private set; }

    // ── Inspector fields ──────────────────────────────────────────────
    [System.Serializable]
    public class ScreenEntry
    {
        public string screenName;    // "Splash", "Home", "Learn", etc.
        public GameObject panel;     // drag the panel GameObject here
    }

    [Header("Register all 11 screen panels here")]
    public ScreenEntry[] screens;

    [Header("Which screen to show when the app starts")]
    public string initialScreen = "Splash";

    // ── Private ───────────────────────────────────────────────────────
    private Dictionary<string, GameObject> _screenMap;
    private string _currentScreen;
    private string _previousScreen;

    // ── Screen name constants (use these instead of typing strings) ────
    // You can reference these from other scripts: UINavigator.SCREEN_HOME
    public const string SCREEN_SPLASH       = "Splash";
    public const string SCREEN_HOME         = "Home";
    public const string SCREEN_LEARN        = "Learn";
    public const string SCREEN_AR           = "AR";
    public const string SCREEN_FLASHCARD    = "Flashcard";
    public const string SCREEN_FC_SUMMARY   = "FlashcardSummary";
    public const string SCREEN_QUIZ_Q       = "QuizQuestion";
    public const string SCREEN_QUIZ_FB      = "QuizFeedback";
    public const string SCREEN_QUIZ_RESULTS = "QuizResults";
    public const string SCREEN_PROGRESS     = "Progress";
    public const string SCREEN_TEACHER      = "Teacher";

    // ── Unity lifecycle ───────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Build the dictionary from the Inspector array
        _screenMap = new Dictionary<string, GameObject>();
        foreach (var entry in screens)
        {
            if (entry.panel != null)
                _screenMap[entry.screenName] = entry.panel;
            else
                Debug.LogWarning($"[UINavigator] Screen '{entry.screenName}' has no panel assigned.");
        }

        // Start with all screens hidden
        foreach (var panel in _screenMap.Values)
            panel.SetActive(false);

        // Show the initial screen
        ShowScreen(initialScreen);
    }

    // ── PUBLIC API ────────────────────────────────────────────────────

    /// <summary>
    /// Hides the current screen and shows the named screen.
    /// The screen's OnEnable() fires automatically when it becomes active,
    /// which is where each controller script loads its data.
    /// </summary>
    public void ShowScreen(string screenName)
    {
        if (!_screenMap.ContainsKey(screenName))
        {
            Debug.LogError($"[UINavigator] Screen '{screenName}' not found. Check the screens array in the Inspector.");
            return;
        }

        // Hide current
        if (!string.IsNullOrEmpty(_currentScreen) && _screenMap.ContainsKey(_currentScreen))
            _screenMap[_currentScreen].SetActive(false);

        // Track history for GoBack()
        _previousScreen = _currentScreen;
        _currentScreen = screenName;

        // Show new screen (this triggers OnEnable on its controller)
        _screenMap[screenName].SetActive(true);

        Debug.Log($"[UINavigator] → {screenName}");
    }

    /// <summary>
    /// Navigate to a topic's Learn screen.
    /// Call this from HomeScreenController when a TopicCard is tapped.
    /// </summary>
    public void OpenTopic(string topicID)
    {
        AppState.CurrentTopicID = topicID;
        ShowScreen(SCREEN_LEARN);
    }

    /// <summary>
    /// Go back to the previous screen.
    /// </summary>
    public void GoBack()
    {
        if (!string.IsNullOrEmpty(_previousScreen))
            ShowScreen(_previousScreen);
    }

    /// <summary>
    /// Navigate to Home and reset all session state.
    /// Use after quiz results or when the user wants to pick a new topic.
    /// </summary>
    public void GoHome()
    {
        AppState.ResetSession();
        ShowScreen(SCREEN_HOME);
    }
}