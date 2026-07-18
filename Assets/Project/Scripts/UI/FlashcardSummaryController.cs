// FlashcardSummaryController.cs
// Folder: Assets/_Project/Scripts/UI/

using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FlashcardSummaryController : MonoBehaviour
{
    [Header("Nav")]
    public TMP_Text navTitleText;

    [Header("Score card bars")]
    public TMP_Text knownLabelText;
    public TMP_Text stillLearningLabelText;
    public TMP_Text skippedLabelText;
    public RectTransform knownFillRect;
    public RectTransform stillLearningFillRect;
    public RectTransform skippedFillRect;

    [Header("Set this to the pixel width of the bar TRACK")]
    [Tooltip("Measure the BarTrack Image width in the Inspector")]
    public float barTrackWidth = 328f; // ← CHANGE THIS TO YOUR ACTUAL TRACK WIDTH

    [Header("Buttons")]
    public Button retryButton;
    public Button quizButton;

    void OnEnable()
    {
        var session = AppState.ActiveFlashcardSession;
        if (session == null)
        {
            Debug.LogWarning("[Summary] No active flashcard session.");
            return;
        }
        PopulateScreen(session);
    }

    void PopulateScreen(FlashcardSession session)
    {
        if (navTitleText != null)
            navTitleText.text = "Flashcard Summary";

        int known = session.KnownCardIDs.Count;
        int stillLearning = session.StillLearningCardIDs.Count;
        int skipped = session.SkippedCardIDs.Count;
        int total = Mathf.Max(known + stillLearning + skipped, 1);

        // Labels
        if (knownLabelText != null)
            knownLabelText.text = $"Known {known}";
        if (stillLearningLabelText != null)
            stillLearningLabelText.text = $"Learning {stillLearning}";
        if (skippedLabelText != null)
            skippedLabelText.text = $"Skipped {skipped}";

        // Set bars with clamping
        SetBar(knownFillRect, known, total);
        SetBar(stillLearningFillRect, stillLearning, total);
        SetBar(skippedFillRect, skipped, total);
    }

    void SetBar(RectTransform fill, int value, int total)
    {
        if (fill == null) return;
        
        // ✅ FIX: Calculate width and clamp to barTrackWidth
        float calculatedWidth = (float)value / total * barTrackWidth;
        float clampedWidth = Mathf.Clamp(calculatedWidth, 0f, barTrackWidth);
        fill.sizeDelta = new Vector2(clampedWidth, fill.sizeDelta.y);
        
        // Debug log to verify
        Debug.Log($"[Summary] value={value}, total={total}, trackWidth={barTrackWidth}, clampedWidth={clampedWidth}");
    }

    public void OnRetryUnknownClicked()
    {
        var s = AppState.ActiveFlashcardSession;
        if (s != null)
        {
            s.CurrentCardIndex = 0;
            s.IsShowingAnswer = false;
            s.KnownCardIDs.Clear();
            s.StillLearningCardIDs.Clear();
            s.SkippedCardIDs.Clear();
        }
        UINavigator.Instance.ShowScreen(UINavigator.SCREEN_FLASHCARD);
    }

    public void OnTakeQuizClicked()
    {
        AppState.ActiveQuizSession = null;
        UINavigator.Instance.ShowScreen(UINavigator.SCREEN_QUIZ_Q);
    }
}