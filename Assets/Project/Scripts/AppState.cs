// AppState.cs
// Place in: Assets/_Project/Scripts/
//
// A static class that acts as the single source of truth for what
// is currently happening in the app — which topic is selected, which
// flashcard session is active, which quiz session is running.
//
// No MonoBehaviour — just plain static fields any script can read/write.
// Think of it as the app's shared memory between screens.

public static class AppState
{
    // ── Currently selected topic ─────────────────────────────────────
    // Set this before navigating to any topic-specific screen.
    // Values match the "id" field in topics.json:
    //   "em_induction" | "current_electricity" | "waves_ii"
    public static string CurrentTopicID = "em_induction";

    // ── Active flashcard session ─────────────────────────────────────
    // Created by FlashcardManager when a deck starts.
    // Read by FlashcardDeckController and FlashcardSummaryController.
    public static FlashcardSession ActiveFlashcardSession = null;

    // ── Active quiz session ──────────────────────────────────────────
    // Created by QuizManager when a quiz starts.
    // Read by QuizQuestionController, QuizFeedbackController, QuizResultsController.
    public static QuizSession ActiveQuizSession = null;

    // ── Last quiz answer (passed from Question screen to Feedback screen) ──
    public static bool LastAnswerWasCorrect = false;
    public static int LastSelectedOptionIndex = -1; // -1 means skipped

    // ── Utility ──────────────────────────────────────────────────────
    // Call this to fully reset the app state (e.g. when returning to Home).
    public static void ResetSession()
    {
        ActiveFlashcardSession = null;
        ActiveQuizSession = null;
        LastAnswerWasCorrect = false;
        LastSelectedOptionIndex = -1;
    }
}