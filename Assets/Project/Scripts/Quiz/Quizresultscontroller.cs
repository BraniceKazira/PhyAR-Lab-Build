// QuizResultsController.cs
// Place in: Assets/_Project/Scripts/Quiz/
// Attach to: Screen_QuizResults
//
// Shows: score ring, heading, performance breakdown only.
// Per-question review dropped — cleaner UX for KCSE students.

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuizResultsController : MonoBehaviour
{
    [Header("Score ring")]
    public Image    scoreRingImage;
    public TMP_Text scoreFractionText;
    public TMP_Text scorePercentText;

    [Header("Heading")]
    public TMP_Text headingText;

    [Header("Breakdown — Correct row")]
    public TMP_Text correctLabelText;
    public TMP_Text correctCountText;
    public RectTransform correctFill;

    [Header("Breakdown — Incorrect row")]
    public TMP_Text incorrectLabelText;
    public TMP_Text incorrectCountText;
    public RectTransform incorrectFill;

    [Header("Breakdown — Skipped row")]
    public TMP_Text skippedLabelText;
    public TMP_Text skippedCountText;
    public RectTransform skippedFill;

    public float barTrackWidth = 224f;

    [Header("Motivational message below breakdown")]
    public TMP_Text encouragementText;

    [Header("Session logger — drag AppManagers")]
    public SessionLogger sessionLogger;

    [Header("Buttons")]
    public Button retryButton;
    public Button nextTopicButton;

    void OnEnable()
    {
        QuizSession session = AppState.ActiveQuizSession;
        if (session == null)
        {
            Debug.LogWarning("[QuizResults] No active quiz session.");
            return;
        }

        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(OnRetryQuizClicked);
        }
        if (nextTopicButton != null)
        {
            nextTopicButton.onClick.RemoveAllListeners();
            nextTopicButton.onClick.AddListener(OnNextTopicClicked);
        }

        PopulateScreen(session);
        if (sessionLogger != null) LogSession(session);
    }

    void PopulateScreen(QuizSession session)
    {
        int total   = session.TotalQuestions;
        int correct = session.Score;
        int skipped = 0, incorrect = 0;

        for (int i = 0; i < session.SelectedAnswers.Count; i++)
        {
            if (session.SelectedAnswers[i] < 0) skipped++;
            else if (session.SelectedAnswers[i] !=
                     session.QuizData.questions[i].correctOptionIndex) incorrect++;
        }

        float pct = (float)correct / Mathf.Max(total, 1);

        // Score ring
        if (scoreRingImage != null)     scoreRingImage.fillAmount = pct;
        if (scoreFractionText != null)  scoreFractionText.text    = $"{correct}/{total}";
        if (scorePercentText != null)   scorePercentText.text     = $"{Mathf.RoundToInt(pct * 100)}%";

        // Heading
        string heading = correct == total ? "Perfect score!" :
                         pct >= 0.7f      ? "Great work!"   :
                         pct >= 0.5f      ? "Good effort!"  : "Keep practising!";
        if (headingText != null) headingText.text = heading;

        // Encouragement line
        if (encouragementText != null)
        {
            encouragementText.text = pct >= 0.7f
                ? "You're ready for the next topic."
                : "Review the flashcards and try again — you've got this.";
        }

        // Breakdown
        SetRow(correctLabelText,   correctCountText,   correctFill,   "Correct",   correct,   total);
        SetRow(incorrectLabelText, incorrectCountText, incorrectFill, "Incorrect", incorrect, total);
        SetRow(skippedLabelText,   skippedCountText,   skippedFill,   "Skipped",   skipped,   total);
    }

    void SetRow(TMP_Text label, TMP_Text count, RectTransform fill,
                string labelText, int value, int total)
    {
        if (label != null) label.text = labelText;
        if (count != null) count.text = value.ToString();
        if (fill  != null)
        {
            float w = (float)value / Mathf.Max(total, 1) * barTrackWidth;
            fill.sizeDelta = new Vector2(w, fill.sizeDelta.y);
        }
    }

    void LogSession(QuizSession session)
    {
        FlashcardSession fc = AppState.ActiveFlashcardSession;
        sessionLogger.LogSession(
            topicID: session.TopicID,
            fcKnown: fc?.KnownCardIDs.Count ?? 0,
            fcStillLearning: fc?.StillLearningCardIDs.Count ?? 0,
            fcSkipped: fc?.SkippedCardIDs.Count ?? 0,
            quizScore: session.Score,
            quizTotal: session.TotalQuestions
        );
    }

    public void OnRetryQuizClicked()
    {
        AppState.ActiveQuizSession = null;
        UINavigator.Instance.ShowScreen(UINavigator.SCREEN_QUIZ_Q);
    }

    public void OnNextTopicClicked()
    {
        UINavigator.Instance.GoHome();
    }
}