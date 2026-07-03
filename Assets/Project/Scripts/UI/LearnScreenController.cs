using UnityEngine;
using TMPro;

public class LearnScreenController : MonoBehaviour
{
    [Header("Nav Bar")]
    public TMP_Text navTitleText;

    [Header("Step Pills")]
    public StepPillsController stepPills;

    [Header("Content")]
    public TMP_Text headingText;
    public TMP_Text definitionText;

    // Concept cards are found automatically via GetComponentsInChildren<ConceptCardUI>
    // No need to drag them here — just make sure each ConceptCard_X has ConceptCardUI.

    void OnEnable()
    {
        Debug.Log("[LearnScreen] OnEnable fired. CurrentTopicID = '"
                  + AppState.CurrentTopicID + "'");

        if (string.IsNullOrEmpty(AppState.CurrentTopicID))
        {
            Debug.LogWarning("[LearnScreen] No topic set in AppState. " +
                             "Make sure TopicSelector.SelectXxx() is called before showing this screen.");
            return;
        }

        LearnData data = ModuleLoader.Instance.LoadLearnData(AppState.CurrentTopicID);

        if (data == null)
        {
            Debug.LogError("[LearnScreen] LoadLearnData returned null for topic: "
                           + AppState.CurrentTopicID +
                           "\nCheck that the file exists at: " +
                           "Assets/_Project/Data/Resources/Data/Learn/learn_"
                           + AppState.CurrentTopicID + ".json");
            return;
        }

        Debug.Log("[LearnScreen] Loaded data for: " + data.topicID +
                  " | Heading: " + data.heading);

        PopulateScreen(data);
    }

    void PopulateScreen(LearnData data)
    {
        // Nav title
        if (navTitleText != null)
        {
            TopicListWrapper topics = ModuleLoader.Instance.LoadTopicList();
            string title = data.topicID;
            if (topics != null)
            {
                TopicEntry entry = topics.topics.Find(t => t.id == data.topicID);
                if (entry != null) title = entry.displayName;
            }
            navTitleText.text = title;
        }

        // Step pills
        if (stepPills != null)
            stepPills.SetActiveStep(0);

        // Heading and definition
        if (headingText != null)    headingText.text    = data.heading;
        if (definitionText != null) definitionText.text = data.definition;

        // Concept cards — find all ConceptCardUI children and update them
        ConceptCardUI[] cards = GetComponentsInChildren<ConceptCardUI>(true);

        Debug.Log("[LearnScreen] Found " + cards.Length + " ConceptCardUI components. " +
                  "JSON has " + data.concepts.Count + " concepts.");

        for (int i = 0; i < cards.Length; i++)
        {
            if (i < data.concepts.Count)
            {
                cards[i].gameObject.SetActive(true);
                cards[i].Populate(
                    data.concepts[i].title,
                    data.concepts[i].description,
                    data.concepts[i].iconGlyph
                );
                Debug.Log("[LearnScreen] Card " + i + " updated to: " + data.concepts[i].title);
            }
            else
            {
                cards[i].gameObject.SetActive(false);
            }
        }
    }

    public void OnExploreARClicked()
    {
        UINavigator.Instance.ShowScreen(UINavigator.SCREEN_AR);
    }

    public void OnFlashcardsClicked()
    {
        UINavigator.Instance.ShowScreen(UINavigator.SCREEN_FLASHCARD);
    }
}