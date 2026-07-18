using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FlashcardDeckController : MonoBehaviour
{
    [Header("Nav")]
    public TMP_Text navTitleText;

    [Header("Step pills")]
    public StepPillsController stepPills;

    [Header("Progress")]
    public TMP_Text progressCaptionText;
    public RectTransform progressFillRect;
    public float progressTrackWidth = 328f;

    [Header("Flashcard")]
    public Button cardPanelButton;
    public TMP_Text cardText;
    public TMP_Text flipHintText;

    [Header("Answer buttons")]
    public Button gotItButton;
    public Button stillLearningButton;
    public Button skipButton;

    [Header("Counter - Icon based ()")]
    public TMP_Text knownCountText;
    public TMP_Text stillLearningCountText;
    public TMP_Text skippedCountText;

    void OnEnable()
    {
        if (ModuleLoader.Instance == null) return;

        if (AppState.ActiveFlashcardSession == null ||
            AppState.ActiveFlashcardSession.TopicID != AppState.CurrentTopicID)
            StartNewSession();

        // Clear all listeners
        if (cardPanelButton != null)
        {
            cardPanelButton.onClick.RemoveAllListeners();
            cardPanelButton.onClick.AddListener(OnCardFlipped);
        }
        if (gotItButton != null)
        {
            gotItButton.onClick.RemoveAllListeners();
            gotItButton.onClick.AddListener(OnGotItClicked);
        }
        if (stillLearningButton != null)
        {
            stillLearningButton.onClick.RemoveAllListeners();
            stillLearningButton.onClick.AddListener(OnStillLearningClicked);
        }
        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(OnSkipClicked);
        }

        RefreshUI();
    }

    void StartNewSession()
    {
        FlashcardDeckData deck = ModuleLoader.Instance.LoadFlashcards(AppState.CurrentTopicID);
        if (deck == null)
        {
            Debug.LogError("[FC] No deck for: " + AppState.CurrentTopicID);
            return;
        }
        AppState.ActiveFlashcardSession = new FlashcardSession
        {
            TopicID = AppState.CurrentTopicID,
            DeckData = deck
        };
        Debug.Log($"[FC] Session started: {AppState.CurrentTopicID} | {AppState.ActiveFlashcardSession.TotalCards} cards");
    }

    void RefreshUI()
    {
        FlashcardSession s = AppState.ActiveFlashcardSession;
        if (s == null) return;

        // Nav title
        if (navTitleText != null)
        {
            string displayName = AppState.CurrentTopicID;
            TopicListWrapper topics = ModuleLoader.Instance.LoadTopicList();
            if (topics != null)
            {
                TopicEntry entry = topics.topics.Find(t => t.id == AppState.CurrentTopicID);
                if (entry != null) displayName = entry.displayName;
            }
            navTitleText.text = "Flashcards · " + displayName;
        }

        // Step pills
        if (stepPills != null) stepPills.SetActiveStep(2);

        // Progress bar and counter
        int total = s.TotalCards;
        int done = s.CurrentCardIndex;
        if (progressCaptionText != null)
            progressCaptionText.text = $"{done + 1} of {total}";
        if (progressFillRect != null && total > 0)
            progressFillRect.sizeDelta = new Vector2(
                progressTrackWidth * ((float)done / total),
                progressFillRect.sizeDelta.y);

        // Card text
        if (cardText != null)
            cardText.text = s.IsShowingAnswer ? s.CurrentCard.answer : s.CurrentCard.question;

        // Flip hint
        if (flipHintText != null)
            flipHintText.gameObject.SetActive(!s.IsShowingAnswer);

        // Answer buttons
        if (gotItButton != null)
            gotItButton.interactable = s.IsShowingAnswer;
        if (stillLearningButton != null)
            stillLearningButton.interactable = s.IsShowingAnswer;
        if (skipButton != null)
            skipButton.interactable = true;

        // Icon counters (compact)
        if (knownCountText != null)
            knownCountText.text = $" {s.KnownCardIDs.Count}";
        if (stillLearningCountText != null)
            stillLearningCountText.text = $"{s.StillLearningCardIDs.Count}";
        if (skippedCountText != null)
            skippedCountText.text = $" {s.SkippedCardIDs.Count}";
    }

    // ── Button Callbacks ──────────────────────────────────────────────

    public void OnCardFlipped()
    {
        var s = AppState.ActiveFlashcardSession;
        if (s == null || s.IsShowingAnswer) return;
        s.IsShowingAnswer = true;
        RefreshUI();
    }

    public void OnGotItClicked()
    {
        var s = AppState.ActiveFlashcardSession;
        if (s == null || !s.IsShowingAnswer) return;
        string id = s.CurrentCard.id;

        // Move to Known, remove from other lists
        if (!s.KnownCardIDs.Contains(id)) s.KnownCardIDs.Add(id);
        if (s.StillLearningCardIDs.Contains(id)) s.StillLearningCardIDs.Remove(id);
        if (s.SkippedCardIDs.Contains(id)) s.SkippedCardIDs.Remove(id);

        Advance(s);
    }

    public void OnStillLearningClicked()
    {
        var s = AppState.ActiveFlashcardSession;
        if (s == null || !s.IsShowingAnswer) return;
        string id = s.CurrentCard.id;

        // Move to StillLearning, remove from other lists
        if (!s.StillLearningCardIDs.Contains(id)) s.StillLearningCardIDs.Add(id);
        if (s.KnownCardIDs.Contains(id)) s.KnownCardIDs.Remove(id);
        if (s.SkippedCardIDs.Contains(id)) s.SkippedCardIDs.Remove(id);

        Advance(s);
    }

    public void OnSkipClicked()
    {
        var s = AppState.ActiveFlashcardSession;
        if (s == null) return;
        string id = s.CurrentCard.id;

        // Move to Skipped, remove from other lists
        if (!s.SkippedCardIDs.Contains(id)) s.SkippedCardIDs.Add(id);
        if (s.KnownCardIDs.Contains(id)) s.KnownCardIDs.Remove(id);
        if (s.StillLearningCardIDs.Contains(id)) s.StillLearningCardIDs.Remove(id);

        Advance(s);
    }

    void Advance(FlashcardSession s)
    {
        bool more = s.AdvanceCard();
        if (!more)
        {
            Debug.Log("[FC] All cards complete → Summary");
            UINavigator.Instance.ShowScreen(UINavigator.SCREEN_FC_SUMMARY);
        }
        else
        {
            RefreshUI();
        }
    }
}