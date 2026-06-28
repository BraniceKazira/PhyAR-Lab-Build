// HomeScreenController.cs
// Place in: Assets/_Project/Scripts/UI/
// Attach to: Screen_Home panel GameObject
//
// Runs when Screen_Home becomes active.
// Loads topics.json and instantiates one TopicCard prefab per topic.

using UnityEngine;
using UnityEngine.UI;

public class HomeScreenController : MonoBehaviour
{
    [Header("Drag the Content child of your TopicScrollView here")]
    public Transform topicListContainer;

    [Header("Drag the TopicCard prefab from Assets/_Project/Prefabs/UI/")]
    public GameObject topicCardPrefab;

    // Tracks whether cards are already built (avoids rebuilding on every visit)
    private bool _cardsBuilt = false;

    void OnEnable()
    {
        if (!_cardsBuilt) BuildTopicCards();
    }

    void BuildTopicCards()
    {
        TopicListWrapper data = ModuleLoader.Instance.LoadTopicList();
        if (data == null || data.topics == null)
        {
            Debug.LogError("[HomeScreenController] Could not load topics.json");
            return;
        }

        // Clear any placeholder cards placed in the editor
        foreach (Transform child in topicListContainer)
            Destroy(child.gameObject);

        // Instantiate one card per topic
        foreach (TopicEntry topic in data.topics)
        {
            GameObject card = Instantiate(topicCardPrefab, topicListContainer);
            TopicCardUI cardUI = card.GetComponent<TopicCardUI>();
            if (cardUI != null)
                cardUI.Populate(topic);
        }

        _cardsBuilt = true;
    }
}


// ═══════════════════════════════════════════════════════════════════════════
// LearnScreenController.cs
// Place in: Assets/_Project/Scripts/UI/
// Attach to: Screen_Learn panel GameObject
//
// Runs every time Screen_Learn becomes active (including when returning
// from AR back to Learn for a different topic).
// Loads learn_{topicID}.json and populates all text fields and concept cards.
// ═══════════════════════════════════════════════════════════════════════════

using TMPro;
using System.Collections.Generic;

public class LearnScreenController : MonoBehaviour
{
    [Header("Nav Bar")]
    public TMP_Text navTitleText;

    [Header("Step Pills — drag StepPillsRow's SubTopicPillController here")]
    public StepPillsController stepPills;

    [Header("Learn content fields")]
    public TMP_Text headingText;
    public TMP_Text definitionText;
    public TMP_Text formulaText;
    public TMP_Text variableDefsText;
    public TMP_Text sourceCaptionText;

    [Header("Concept cards")]
    public Transform conceptCardsContainer;  // the Vertical Layout Group container
    public GameObject conceptCardPrefab;     // ConceptCard prefab

    void OnEnable()
    {
        LearnData data = ModuleLoader.Instance.LoadLearnData(AppState.CurrentTopicID);
        if (data == null) return;

        PopulateScreen(data);
    }

    void PopulateScreen(LearnData data)
    {
        // Nav bar title (show topic name + chapter)
        TopicListWrapper topics = ModuleLoader.Instance.LoadTopicList();
        string chapter = "";
        if (topics != null)
        {
            TopicEntry topic = topics.topics.Find(t => t.id == data.topicID);
            if (topic != null)
                chapter = " · " + topic.form + " " + topic.chapter;
        }
        navTitleText.text = data.topicID.Replace("_", " ").ToUpper() + chapter;

        // Step pills — Learn is always the active step on this screen
        if (stepPills != null) stepPills.SetActiveStep(0); // 0 = Learn

        // Text content
        headingText.text     = data.heading;
        definitionText.text  = data.definition;
        formulaText.text     = data.formula;
        variableDefsText.text = data.variableDefs.Replace("\\n", "\n");
        sourceCaptionText.text = "Source: " + data.source;

        // Concept cards — clear old ones, instantiate fresh
        foreach (Transform child in conceptCardsContainer)
            Destroy(child.gameObject);

        foreach (ConceptEntry concept in data.concepts)
        {
            GameObject card = Instantiate(conceptCardPrefab, conceptCardsContainer);
            ConceptCardUI cardUI = card.GetComponent<ConceptCardUI>();
            if (cardUI != null)
                cardUI.Populate(concept.title, concept.description, concept.iconGlyph);
        }
    }

    // Called by the "Explore in AR →" button on this screen
    public void OnExploreARClicked()
    {
        UINavigator.Instance.ShowScreen(UINavigator.SCREEN_AR);
    }

    // Called by the "Flashcards" step pill or a shortcut button
    public void OnGoToFlashcardsClicked()
    {
        UINavigator.Instance.ShowScreen(UINavigator.SCREEN_FLASHCARD);
    }
}


// ═══════════════════════════════════════════════════════════════════════════
// ConceptCardUI.cs
// Place in: Assets/_Project/Scripts/UI/
// Attach to: ConceptCard prefab
// ═══════════════════════════════════════════════════════════════════════════

public class ConceptCardUI : MonoBehaviour
{
    public TMP_Text iconGlyphText;   // the letter inside the copper circle
    public TMP_Text titleText;       // "Faraday's Law"
    public TMP_Text descriptionText; // one-line description

    public void Populate(string title, string description, string glyph)
    {
        titleText.text       = title;
        descriptionText.text = description;
        iconGlyphText.text   = glyph;
    }
}


// ═══════════════════════════════════════════════════════════════════════════
// StepPillsController.cs
// Place in: Assets/_Project/Scripts/UI/
// Attach to: StepPillsRow GameObject (the one with the Horizontal Layout Group)
//
// Controls the 4-step progress pills: Learn / AR / Flashcards / Quiz
// Call SetActiveStep(int step) from each screen's controller:
//   0 = Learn active, AR/Flashcards/Quiz inactive
//   1 = Learn done, AR active
//   2 = Learn+AR done, Flashcards active
//   3 = Learn+AR+Flashcards done, Quiz active
// ═══════════════════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class StepPillsController : MonoBehaviour
{
    [System.Serializable]
    public class PillUI
    {
        public Image background;
        public TMP_Text label;
    }

    public List<PillUI> pills; // size 4: [Learn, AR, Flashcards, Quiz]

    // Colours
    private Color _activeColor   = new Color(0.722f, 0.451f, 0.200f); // #B87333 Copper
    private Color _doneColor     = new Color(0.180f, 0.769f, 0.710f); // #2EC4B6 Coil Green
    private Color _inactiveColor = new Color(0.10f, 0.10f, 0.18f, 0.08f);
    private Color _activeLbl     = Color.white;
    private Color _doneLbl       = Color.white;
    private Color _inactiveLbl   = new Color(0.10f, 0.10f, 0.18f, 0.50f);

    /// <summary>
    /// activeStep: 0=Learn, 1=AR, 2=Flashcards, 3=Quiz
    /// Steps before activeStep are shown as "done" (Coil Green).
    /// Steps after are shown as "inactive" (grey).
    /// </summary>
    public void SetActiveStep(int activeStep)
    {
        for (int i = 0; i < pills.Count; i++)
        {
            if (i < activeStep)
            {
                // Done
                pills[i].background.color = _doneColor;
                pills[i].label.color = _doneLbl;
            }
            else if (i == activeStep)
            {
                // Active
                pills[i].background.color = _activeColor;
                pills[i].label.color = _activeLbl;
            }
            else
            {
                // Inactive
                pills[i].background.color = _inactiveColor;
                pills[i].label.color = _inactiveLbl;
            }
        }
    }
}
