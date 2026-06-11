using UnityEngine;

public class UINavigator : MonoBehaviour
{
    // Singleton instance so other scripts can call it easily
    public static UINavigator Instance;

    [Header("All Screens")]
    public GameObject splashScreen;
    public GameObject homeScreen;
    public GameObject learnScreen;
    public GameObject arTransitionScreen;
    public GameObject flashcardsScreen;
    public GameObject flashcardSummaryScreen;
    public GameObject quizScreen;
    public GameObject quizFeedbackScreen;
    public GameObject quizResultsScreen;
    public GameObject progressScreen;
    public GameObject teacherPanelScreen;

    // Internal list of all screens for easy looping
    GameObject[] allScreens;

    void Awake()
    {
        Instance = this;

        // Put every screen into an array so we can turn them on/off together
        allScreens = new GameObject[]
        {
            splashScreen, homeScreen, learnScreen,
            arTransitionScreen, flashcardsScreen,
            flashcardSummaryScreen, quizScreen,
            quizFeedbackScreen, quizResultsScreen,
            progressScreen, teacherPanelScreen
        };

        // Start with the splash screen visible
        ShowScreen(splashScreen);
    }

    // Turns off all screens, then turns on only the target screen
    public void ShowScreen(GameObject target)
    {
        foreach (var s in allScreens)
        {
            if (s != null)
                s.SetActive(s == target);
        }
    }

    // Shortcut methods – these get hooked up to UI buttons in the Inspector
    public void GoToHome()           => ShowScreen(homeScreen);
    public void GoToLearn()          => ShowScreen(learnScreen);
    public void GoToFlashcards()     => ShowScreen(flashcardsScreen);
    public void GoToFlashcardSummary() => ShowScreen(flashcardSummaryScreen);
    public void GoToQuiz()           => ShowScreen(quizScreen);
    public void GoToQuizFeedback()   => ShowScreen(quizFeedbackScreen);
    public void GoToQuizResults()    => ShowScreen(quizResultsScreen);
    public void GoToProgress()       => ShowScreen(progressScreen);
    public void GoToTeacherPanel()   => ShowScreen(teacherPanelScreen);
    public void GoToHome_FromResults() => ShowScreen(homeScreen);
}