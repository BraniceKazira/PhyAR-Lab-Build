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
    public TMP_Text progressCaptionText;     // "Card 3 of 7"
    public RectTransform progressFillRect;   // leave empty if no fill child yet
    public float progressTrackWidth = 328f;

    [Header("Flashcard panel")]
    public Button cardPanelButton;   // Button component on FlashCardPanel itself
    public TMP_Text cardText;        // QuestionText — shows question, then answer
    public TMP_Text flipHintText;    // "Tap to reveal answer"

    [Header("Answer buttons")]
    public Button gotItButton;
    public Button stillLearningButton;

    [Header("Counter strip")]
    public TMP_Text knownCountText;
    public TMP_Text stillLearningCountText;
    public TMP_Text skippedCountText;

    [Header("Sub-topic pills")]
    public SubTopicPillController subTopicPills;

    [Header("Continue button")]
    public Button continueButton;
    public TMP_Text continueButtonText;

    void OnEnable()
    {
        Debug.Log("[FlashcardDeck] OnEnable fired.");

        if (ModuleLoader.Instance == null)
        {
            Debug.LogError("[FlashcardDeck] ModuleLoader.Instance is NULL.");
            return;
        }

        if (AppState.ActiveFlashcardSession == null ||
            AppState.ActiveFlashcardSession.TopicID != AppState.CurrentTopicID)
        {
            StartNewSession();
        }

        // Check field wiring
        if (cardPanelButton == null)
            Debug.LogError("[FlashcardDeck] cardPanelButton is NOT WIRED in the Inspector.");
        else
            Debug.Log("[FlashcardDeck] cardPanelButton IS wired to: " + cardPanelButton.gameObject.name);

        if (cardText == null)
            Debug.LogError("[FlashcardDeck] cardText is NOT WIRED in the Inspector.");

        if (cardPanelButton != null)
        {
            cardPanelButton.onClick.RemoveAllListeners();
            cardPanelButton.onClick.AddListener(OnCardFlipped);
            Debug.Log("[FlashcardDeck] OnClick listener attached to cardPanelButton.");
        }

        RefreshUI();
    }

    void StartNewSession()
    {
        FlashcardDeckData deck = ModuleLoader.Instance.LoadFlashcards(AppState.CurrentTopicID);
        if (deck == null)
        {
            Debug.LogError("[FlashcardDeck] Could not load flashcards for: " + AppState.CurrentTopicID);
            return;
        }

        AppState.ActiveFlashcardSession = new FlashcardSession
        {
            TopicID  = AppState.CurrentTopicID,
            DeckData = deck
        };
    }

    void RefreshUI()
    {
        FlashcardSession session = AppState.ActiveFlashcardSession;
        if (session == null) return;

        // Nav title
        if (navTitleText != null)
            navTitleText.text = "Flashcards · " + session.CurrentSubTopic.fullName;

        // Step pills — Flashcards = step 2
        if (stepPills != null) stepPills.SetActiveStep(2);

        // Progress
        int cardNumber = session.CompletedCards + 1;
        int totalCards = session.TotalCards;
        if (progressCaptionText != null)
            progressCaptionText.text = $"Card {cardNumber} of {totalCards}";

        if (progressFillRect != null)
        {
            float pct = (float)session.CompletedCards / totalCards;
            progressFillRect.sizeDelta = new Vector2(progressTrackWidth * pct, progressFillRect.sizeDelta.y);
        }

        // Card content — show question or answer in the SAME text field
        if (cardText != null)
        {
            cardText.text = session.IsShowingAnswer
                ? session.CurrentCard.answer
                : session.CurrentCard.question;
            Debug.Log("[FlashcardDeck] cardText.text set to: " + cardText.text);
        }
        else
        {
            Debug.LogError("[FlashcardDeck] cardText is null — cannot display question/answer.");
        }

        // Flip hint — hide once flipped, show "Tap to reveal answer" before flip
        if (flipHintText != null)
            flipHintText.gameObject.SetActive(!session.IsShowingAnswer);

        // Buttons only usable after flip
        if (gotItButton != null) gotItButton.interactable = session.IsShowingAnswer;
        if (stillLearningButton != null) stillLearningButton.interactable = session.IsShowingAnswer;

        // Counter strip
        if (knownCountText != null)
            knownCountText.text = session.KnownCardIDs.Count + " known";
        if (stillLearningCountText != null)
            stillLearningCountText.text = session.StillLearningCardIDs.Count + " still learning";
        if (skippedCountText != null)
            skippedCountText.text = session.SkippedCardIDs.Count + " skipped";

        // Sub-topic pills
        if (subTopicPills != null)
        {
            SubTopicEntry[] entries = new SubTopicEntry[session.DeckData.subTopics.Count];
            for (int i = 0; i < session.DeckData.subTopics.Count; i++)
            {
                entries[i] = new SubTopicEntry
                {
                    id = session.DeckData.subTopics[i].id,
                    shortName = session.DeckData.subTopics[i].shortName
                };
            }
            subTopicPills.SetSubTopics(entries);
            subTopicPills.SetActiveByIndex(session.CurrentSubTopicIndex);
        }

        // Continue button — only visible when current sub-topic is fully answered
        bool subTopicDone = CheckCurrentSubTopicDone(session);
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(subTopicDone);
            if (subTopicDone && continueButtonText != null)
                continueButtonText.text = session.HasNextCard()
                    ? "Continue to next sub-topic"
                    : "See your results";
        }
    }

    bool CheckCurrentSubTopicDone(FlashcardSession session)
    {
        foreach (var card in session.CurrentSubTopic.cards)
        {
            bool answered = session.KnownCardIDs.Contains(card.id) ||
                            session.StillLearningCardIDs.Contains(card.id) ||
                            session.SkippedCardIDs.Contains(card.id);
            if (!answered) return false;
        }
        return true;
    }

    // ── Button callbacks ──────────────────────────────────────────────

    public void OnCardFlipped()
    {
        Debug.Log("[FlashcardDeck] OnCardFlipped() was called — tap registered!");

        if (AppState.ActiveFlashcardSession == null)
        {
            Debug.LogError("[FlashcardDeck] No active session — cannot flip.");
            return;
        }
        if (AppState.ActiveFlashcardSession.IsShowingAnswer)
        {
            Debug.Log("[FlashcardDeck] Already showing answer, ignoring tap.");
            return;
        }
        AppState.ActiveFlashcardSession.IsShowingAnswer = true;
        Debug.Log("[FlashcardDeck] IsShowingAnswer set to TRUE. Answer text should be: "
                  + AppState.ActiveFlashcardSession.CurrentCard.answer);
        RefreshUI();
    }

    public void OnGotItClicked()
    {
        FlashcardSession session = AppState.ActiveFlashcardSession;
        if (session == null || !session.IsShowingAnswer) return;

        string cardID = session.CurrentCard.id;
        if (!session.KnownCardIDs.Contains(cardID))
            session.KnownCardIDs.Add(cardID);

        AdvanceOrComplete(session);
    }

    public void OnStillLearningClicked()
    {
        FlashcardSession session = AppState.ActiveFlashcardSession;
        if (session == null || !session.IsShowingAnswer) return;

        string cardID = session.CurrentCard.id;
        if (!session.StillLearningCardIDs.Contains(cardID))
            session.StillLearningCardIDs.Add(cardID);

        AdvanceOrComplete(session);
    }

    public void OnContinueClicked()
    {
        FlashcardSession session = AppState.ActiveFlashcardSession;
        if (session == null) return;

        if (session.HasNextCard())
        {
            session.AdvanceCard();
            RefreshUI();
        }
        else
        {
            UINavigator.Instance.ShowScreen(UINavigator.SCREEN_FC_SUMMARY);
        }
    }

    void AdvanceOrComplete(FlashcardSession session)
    {
        bool hasNext = session.AdvanceCard();
        if (!hasNext)
            UINavigator.Instance.ShowScreen(UINavigator.SCREEN_FC_SUMMARY);
        else
            RefreshUI();
    }
}