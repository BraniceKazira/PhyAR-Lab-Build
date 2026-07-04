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

    [Header("Counter")]
    public TMP_Text knownCountText;
    public TMP_Text stillLearningCountText;
    public TMP_Text skippedCountText;

    [Header("Sub-topic pills")]
    public SubTopicPillController subTopicPills;

    [Header("Continue")]
    public Button continueButton;
    public TMP_Text continueButtonText;

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
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnContinueClicked);
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
    }

    void RefreshUI()
    {
        FlashcardSession s = AppState.ActiveFlashcardSession;
        if (s == null) return;

        if (navTitleText != null)
            navTitleText.text = "Flashcards · " + s.CurrentSubTopic.fullName;

        if (stepPills != null) stepPills.SetActiveStep(2);

        int total = s.TotalCards;
        int done = s.CompletedCards;
        if (progressCaptionText != null)
            progressCaptionText.text = $"Card {done + 1} of {total}";
        if (progressFillRect != null && total > 0)
            progressFillRect.sizeDelta = new Vector2(
                progressTrackWidth * ((float)done / total),
                progressFillRect.sizeDelta.y);

        if (cardText != null)
            cardText.text = s.IsShowingAnswer ? s.CurrentCard.answer : s.CurrentCard.question;

        if (flipHintText != null)
            flipHintText.gameObject.SetActive(!s.IsShowingAnswer);

        if (gotItButton != null)
            gotItButton.interactable = s.IsShowingAnswer;
        if (stillLearningButton != null)
            stillLearningButton.interactable = s.IsShowingAnswer;
        if (skipButton != null)
            skipButton.interactable = true;

        // COUNTERS - FIXED
        if (knownCountText != null)
            knownCountText.text = s.KnownCardIDs.Count + " known";
        if (stillLearningCountText != null)
            stillLearningCountText.text = s.StillLearningCardIDs.Count + " still learning";
        if (skippedCountText != null)
        {
            skippedCountText.text = s.SkippedCardIDs.Count + " skipped";
            skippedCountText.gameObject.SetActive(true);
        }

        if (subTopicPills != null)
        {
            var entries = new SubTopicEntry[s.DeckData.subTopics.Count];
            for (int i = 0; i < s.DeckData.subTopics.Count; i++)
                entries[i] = new SubTopicEntry
                {
                    id = s.DeckData.subTopics[i].id,
                    shortName = s.DeckData.subTopics[i].shortName
                };
            subTopicPills.SetSubTopics(entries);
            subTopicPills.SetActiveByIndex(s.CurrentSubTopicIndex);
        }

        bool done2 = CheckSubTopicDone(s);
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(done2);
            if (done2 && continueButtonText != null)
                continueButtonText.text = s.HasNextCard() ? "Continue to next sub-topic" : "See your results";
        }
    }

    bool CheckSubTopicDone(FlashcardSession s)
    {
        foreach (var card in s.CurrentSubTopic.cards)
        {
            if (!s.KnownCardIDs.Contains(card.id) &&
                !s.StillLearningCardIDs.Contains(card.id) &&
                !s.SkippedCardIDs.Contains(card.id))
                return false;
        }
        return true;
    }

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
        if (!s.KnownCardIDs.Contains(id)) s.KnownCardIDs.Add(id);
        Advance(s);
    }

    public void OnStillLearningClicked()
    {
        var s = AppState.ActiveFlashcardSession;
        if (s == null || !s.IsShowingAnswer) return;
        string id = s.CurrentCard.id;
        if (!s.StillLearningCardIDs.Contains(id)) s.StillLearningCardIDs.Add(id);
        Advance(s);
    }

    public void OnSkipClicked()
    {
        var s = AppState.ActiveFlashcardSession;
        if (s == null) return;
        string id = s.CurrentCard.id;
        if (!s.SkippedCardIDs.Contains(id) &&
            !s.KnownCardIDs.Contains(id) &&
            !s.StillLearningCardIDs.Contains(id))
            s.SkippedCardIDs.Add(id);
        Advance(s);
    }

    public void OnContinueClicked()
    {
        var s = AppState.ActiveFlashcardSession;
        if (s == null) return;
        if (s.HasNextCard())
        {
            s.AdvanceCard();
            RefreshUI();
        }
        else
        {
            UINavigator.Instance.ShowScreen(UINavigator.SCREEN_FC_SUMMARY);
        }
    }

    void Advance(FlashcardSession s)
    {
        bool more = s.AdvanceCard();
        if (!more)
            UINavigator.Instance.ShowScreen(UINavigator.SCREEN_FC_SUMMARY);
        else
            RefreshUI();
    }
}