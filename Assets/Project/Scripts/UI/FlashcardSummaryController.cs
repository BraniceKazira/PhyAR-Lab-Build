using UnityEngine;
using TMPro;

public class FlashcardSummaryController : MonoBehaviour
{
    [Header("Nav")]
    public TMP_Text navTitleText;

    [Header("Optional heading — leave empty if you don't have this text object")]
    public TMP_Text completeHeadingText;

    [Header("Score card bars")]
    public TMP_Text knownLabelText;
    public TMP_Text stillLearningLabelText;
    public TMP_Text skippedLabelText;
    public RectTransform knownFillRect;
    public RectTransform stillLearningFillRect;
    public RectTransform skippedFillRect;
    public float barTrackWidth = 224f;

    // Checklist rows are found automatically — no field needed here.

    void OnEnable()
    {
        Debug.Log("[FlashcardSummary] OnEnable fired.");

        FlashcardSession session = AppState.ActiveFlashcardSession;
        if (session == null)
        {
            Debug.LogWarning("[FlashcardSummary] No active flashcard session found.");
            return;
        }

        PopulateScreen(session);
    }

    void PopulateScreen(FlashcardSession session)
    {
        // Nav title
        if (navTitleText != null)
        {
            TopicListWrapper topics = ModuleLoader.Instance.LoadTopicList();
            string title = "Flashcard Summary";
            if (topics != null)
            {
                TopicEntry entry = topics.topics.Find(t => t.id == session.TopicID);
                if (entry != null) title = "Flashcard Summary · " + entry.displayName;
            }
            navTitleText.text = title;
        }

        if (completeHeadingText != null)
            completeHeadingText.text = "Flashcards complete!";

        // Score card
        int known = session.KnownCardIDs.Count;
        int stillLearning = session.StillLearningCardIDs.Count;
        int skipped = session.SkippedCardIDs.Count;
        int total = Mathf.Max(known + stillLearning + skipped, 1);

        if (knownLabelText != null) knownLabelText.text = $"Known {known}";
        if (stillLearningLabelText != null) stillLearningLabelText.text = $"Still learning {stillLearning}";
        if (skippedLabelText != null) skippedLabelText.text = $"Skipped {skipped}";

        SetBarWidth(knownFillRect, known, total);
        SetBarWidth(stillLearningFillRect, stillLearning, total);
        SetBarWidth(skippedFillRect, skipped, total);

        // Checklist — find all rows automatically, fill in order
        BuildChecklist(session);
    }

    void SetBarWidth(RectTransform fill, int value, int total)
    {
        if (fill == null) return;
        float width = (float)value / total * barTrackWidth;
        fill.sizeDelta = new Vector2(width, fill.sizeDelta.y);
    }

    void BuildChecklist(FlashcardSession session)
    {
        ChecklistRowUI[] rows = GetComponentsInChildren<ChecklistRowUI>(true);

        Debug.Log($"[FlashcardSummary] Found {rows.Length} checklist rows. " +
                  $"Topic has {session.DeckData.subTopics.Count} sub-topics.");

        for (int i = 0; i < rows.Length; i++)
        {
            if (i < session.DeckData.subTopics.Count)
            {
                var subTopic = session.DeckData.subTopics[i];

                // A sub-topic counts as "done" if every card in it is marked Known
                bool allKnown = true;
                foreach (var card in subTopic.cards)
                {
                    if (!session.KnownCardIDs.Contains(card.id))
                    {
                        allKnown = false;
                        break;
                    }
                }

                rows[i].gameObject.SetActive(true);
                rows[i].Populate(subTopic.fullName, allKnown);
            }
            else
            {
                // Hide extra rows if this topic has fewer sub-topics
                rows[i].gameObject.SetActive(false);
            }
        }
    }

    // ── Button callbacks ──────────────────────────────────────────────

    public void OnRetryUnknownClicked()
    {
        if (AppState.ActiveFlashcardSession != null)
        {
            AppState.ActiveFlashcardSession.CurrentSubTopicIndex = 0;
            AppState.ActiveFlashcardSession.CurrentCardIndex = 0;
            AppState.ActiveFlashcardSession.IsShowingAnswer = false;
        }
        UINavigator.Instance.ShowScreen(UINavigator.SCREEN_FLASHCARD);
    }

    public void OnTakeQuizClicked()
    {
        AppState.ActiveQuizSession = null;
        UINavigator.Instance.ShowScreen(UINavigator.SCREEN_QUIZ_Q);
    }
}