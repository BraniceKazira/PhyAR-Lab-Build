// FlashcardDeckController.cs
// Place in: Assets/_Project/Scripts/UI/
// Attach to: Screen_FlashcardDeck panel GameObject
//
// Controls the flashcard deck screen. Manages the flip interaction,
// Got it / Still learning buttons, progress bar, sub-topic pills,
// and counter strip. Reads from AppState.ActiveFlashcardSession.

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
    public TMP_Text progressCaptionText;   // "Card 3 of 7"
    public RectTransform progressFillRect; // the fill Image inside the progress bar track
    public float progressTrackWidth = 328f;

    [Header("Flashcard panel")]
    public GameObject questionStateGroup;  // shown when IsShowingAnswer = false
    public GameObject answerStateGroup;    // shown when IsShowingAnswer = true
    public TMP_Text questionText;
    public TMP_Text answerText;
    public TMP_Text flipHintText;          // "Tap to reveal answer"

    [Header("Buttons")]
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

    // ── Unity lifecycle ───────────────────────────────────────────────
    void OnEnable()
    {
        // Start a new session if one doesn't exist for the current topic
        if (AppState.ActiveFlashcardSession == null ||
            AppState.ActiveFlashcardSession.TopicID != AppState.CurrentTopicID)
        {
            StartNewSession();
        }

        RefreshUI();
    }

    // ── Session management ────────────────────────────────────────────
    void StartNewSession()
    {
        FlashcardDeckData deck = ModuleLoader.Instance.LoadFlashcards(AppState.CurrentTopicID);
        if (deck == null)
        {
            Debug.LogError("[FlashcardDeckController] Could not load flashcard data.");
            return;
        }

        AppState.ActiveFlashcardSession = new FlashcardSession
        {
            TopicID = AppState.CurrentTopicID,
            DeckData = deck
        };
    }

    // ── UI refresh ────────────────────────────────────────────────────
    void RefreshUI()
    {
        FlashcardSession session = AppState.ActiveFlashcardSession;
        if (session == null) return;

        // Nav title
        navTitleText.text = "Flashcards · " + session.CurrentSubTopic.fullName;

        // Step pills — Flashcards is step index 2
        if (stepPills != null) stepPills.SetActiveStep(2);

        // Progress caption + bar
        int cardNumber = session.CompletedCards + 1;
        int totalCards = session.TotalCards;
        progressCaptionText.text = $"Card {cardNumber} of {totalCards}";

        float fillPercent = (float)session.CompletedCards / totalCards;
        progressFillRect.sizeDelta = new Vector2(progressTrackWidth * fillPercent,
                                                  progressFillRect.sizeDelta.y);

        // Card content — show question or answer depending on flip state
        questionText.text = session.CurrentCard.question;
        answerText.text   = session.CurrentCard.answer;

        bool showingAnswer = session.IsShowingAnswer;
        questionStateGroup.SetActive(!showingAnswer);
        answerStateGroup.SetActive(showingAnswer);

        // Buttons — only enable Got it / Still learning when answer is showing
        gotItButton.interactable        = showingAnswer;
        stillLearningButton.interactable = showingAnswer;

        // Counter strip
        knownCountText.text        = session.KnownCardIDs.Count.ToString() + " known";
        stillLearningCountText.text = session.StillLearningCardIDs.Count.ToString() + " still learning";
        skippedCountText.text      = session.SkippedCardIDs.Count.ToString() + " skipped";

        // Sub-topic pills
        if (subTopicPills != null)
        {
            var subTopicDataArray = new FlashcardSubTopic[session.DeckData.subTopics.Count];
            session.DeckData.subTopics.CopyTo(subTopicDataArray);

            // Convert to the SubTopicEntry format SubTopicPillController expects
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
        }

        // Continue button — only show when the current sub-topic is fully done
        bool subTopicDone = CheckCurrentSubTopicDone(session);
        continueButton.gameObject.SetActive(subTopicDone);
        if (subTopicDone)
            continueButtonText.text = session.HasNextCard()
                ? "Continue to next sub-topic"
                : "See your results";
    }

    bool CheckCurrentSubTopicDone(FlashcardSession session)
    {
        // All cards in the current sub-topic have been answered
        var subTopic = session.CurrentSubTopic;
        foreach (var card in subTopic.cards)
        {
            bool answered = session.KnownCardIDs.Contains(card.id) ||
                            session.StillLearningCardIDs.Contains(card.id) ||
                            session.SkippedCardIDs.Contains(card.id);
            if (!answered) return false;
        }
        return true;
    }

    // ── Button callbacks (wire these in Inspector via On Click) ───────

    // Called by tapping the flashcard panel itself
    public void OnCardFlipped()
    {
        if (AppState.ActiveFlashcardSession == null) return;
        AppState.ActiveFlashcardSession.IsShowingAnswer = true;
        RefreshUI();
    }

    public void OnGotItClicked()
    {
        FlashcardSession session = AppState.ActiveFlashcardSession;
        if (session == null) return;

        string cardID = session.CurrentCard.id;
        if (!session.KnownCardIDs.Contains(cardID))
            session.KnownCardIDs.Add(cardID);

        AdvanceOrComplete(session);
    }

    public void OnStillLearningClicked()
    {
        FlashcardSession session = AppState.ActiveFlashcardSession;
        if (session == null) return;

        string cardID = session.CurrentCard.id;
        if (!session.StillLearningCardIDs.Contains(cardID))
            session.StillLearningCardIDs.Add(cardID);

        AdvanceOrComplete(session);
    }

    // Called by the "Continue to next sub-topic" / "See your results" button
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
            // All cards done — go to summary
            UINavigator.Instance.ShowScreen(UINavigator.SCREEN_FC_SUMMARY);
        }
    }

    void AdvanceOrComplete(FlashcardSession session)
    {
        bool hasNext = session.AdvanceCard();
        if (!hasNext)
        {
            // Deck complete
            UINavigator.Instance.ShowScreen(UINavigator.SCREEN_FC_SUMMARY);
        }
        else
        {
            RefreshUI();
        }
    }
}


// ═══════════════════════════════════════════════════════════════════════════
// FlashcardSummaryController.cs
// Place in: Assets/_Project/Scripts/UI/
// Attach to: Screen_FlashcardSummary panel GameObject
//
// Shows the results of a completed flashcard session.
// Reads AppState.ActiveFlashcardSession.
// ═══════════════════════════════════════════════════════════════════════════

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
    public float barTrackWidth = 200f;

    [Header("Sub-topic checklist")]
    // Drag in each ChecklistRow's TMP label and status circle Image here
    // (one entry per sub-topic — matches the topic with the most sub-topics, which is 5)
    public Transform checklistContainer;
    public GameObject checklistRowPrefab;

    void OnEnable()
    {
        FlashcardSession session = AppState.ActiveFlashcardSession;
        if (session == null) return;

        PopulateScreen(session);
    }

    void PopulateScreen(FlashcardSession session)
    {
        // Nav title
        navTitleText.text = "Flashcard Summary · " +
            session.DeckData.topicID.Replace("_", " ");

        // Score card
        int known = session.KnownCardIDs.Count;
        int stillLearning = session.StillLearningCardIDs.Count;
        int skipped = session.SkippedCardIDs.Count;
        int total = Mathf.Max(known + stillLearning + skipped, 1); // avoid div/0

        knownLabelText.text = $"Known {known}";
        stillLearningLabelText.text = $"Still learning {stillLearning}";
        skippedLabelText.text = $"Skipped {skipped}";

        SetBarWidth(knownFillRect, known, total);
        SetBarWidth(stillLearningFillRect, stillLearning, total);
        SetBarWidth(skippedFillRect, skipped, total);

        // Sub-topic checklist
        BuildChecklist(session);
    }

    void SetBarWidth(RectTransform fill, int value, int total)
    {
        float width = (float)value / total * barTrackWidth;
        fill.sizeDelta = new Vector2(width, fill.sizeDelta.y);
    }

    void BuildChecklist(FlashcardSession session)
    {
        // Clear old rows
        foreach (Transform child in checklistContainer)
            Destroy(child.gameObject);

        // One row per sub-topic
        foreach (var subTopic in session.DeckData.subTopics)
        {
            // Check if all cards in this sub-topic are "known"
            bool allKnown = true;
            foreach (var card in subTopic.cards)
            {
                if (!session.KnownCardIDs.Contains(card.id))
                {
                    allKnown = false;
                    break;
                }
            }

            GameObject row = Instantiate(checklistRowPrefab, checklistContainer);
            ChecklistRowUI rowUI = row.GetComponent<ChecklistRowUI>();
            if (rowUI != null)
                rowUI.Populate(subTopic.fullName, allKnown);
        }
    }

    // ── Button callbacks ──────────────────────────────────────────────

    public void OnRetryUnknownClicked()
    {
        // Restart the session — the deck controller will pick up the same session
        if (AppState.ActiveFlashcardSession != null)
        {
            // Reset card position to start (keep result lists for context)
            AppState.ActiveFlashcardSession.CurrentSubTopicIndex = 0;
            AppState.ActiveFlashcardSession.CurrentCardIndex = 0;
            AppState.ActiveFlashcardSession.IsShowingAnswer = false;
            // Clear known so only "still learning" cards are retried
            // (a full retry would also clear StillLearningCardIDs)
        }
        UINavigator.Instance.ShowScreen(UINavigator.SCREEN_FLASHCARD);
    }

    public void OnTakeQuizClicked()
    {
        // Clear any previous quiz session for this topic
        AppState.ActiveQuizSession = null;
        UINavigator.Instance.ShowScreen(UINavigator.SCREEN_QUIZ_Q);
    }
}


// ═══════════════════════════════════════════════════════════════════════════
// ChecklistRowUI.cs
// Attach to: the checklist row prefab
// ═══════════════════════════════════════════════════════════════════════════

public class ChecklistRowUI : MonoBehaviour
{
    public TMP_Text subTopicLabel;
    public Image statusCircle;

    // Colours
    private Color _doneColor   = new Color(0.180f, 0.769f, 0.710f); // Coil Green
    private Color _pendingColor = new Color(0.10f, 0.10f, 0.18f, 0.15f);

    public void Populate(string name, bool done)
    {
        subTopicLabel.text = name;
        statusCircle.color = done ? _doneColor : _pendingColor;
    }
}