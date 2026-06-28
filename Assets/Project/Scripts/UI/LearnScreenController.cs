using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LearnScreenController : MonoBehaviour
{
    [Header("Nav Bar")]
    public TMP_Text navTitleText;

    [Header("Step Pills")]
    public StepPillsController stepPills;

    [Header("Learn content text fields")]
    public TMP_Text headingText;
    public TMP_Text definitionText;
    public TMP_Text formulaText;
    public TMP_Text variableDefsText;
    public TMP_Text sourceCaptionText;

    [Header("Concept cards")]
    public Transform conceptCardsContainer;
    public GameObject conceptCardPrefab;

    void OnEnable()
    {
        LearnData data = ModuleLoader.Instance.LoadLearnData(AppState.CurrentTopicID);
        if (data == null)
        {
            Debug.LogError("[LearnScreenController] No learn data for: " + AppState.CurrentTopicID);
            return;
        }
        PopulateScreen(data);
    }

    void PopulateScreen(LearnData data)
    {
        // Nav title — topic name from topics list
        TopicListWrapper topics = ModuleLoader.Instance.LoadTopicList();
        string navTitle = data.topicID;
        if (topics != null)
        {
            TopicEntry entry = topics.topics.Find(t => t.id == data.topicID);
            if (entry != null)
                navTitle = entry.displayName + " · " + entry.form + " " + entry.chapter;
        }
        if (navTitleText) navTitleText.text = navTitle;

        // Step pills — Learn is always step 0 on this screen
        if (stepPills != null) stepPills.SetActiveStep(0);

        // Text fields
        if (headingText)      headingText.text      = data.heading;
        if (definitionText)   definitionText.text   = data.definition;
        if (formulaText)      formulaText.text       = data.formula;
        if (variableDefsText) variableDefsText.text  = data.variableDefs.Replace("\\n", "\n");
        if (sourceCaptionText) sourceCaptionText.text = "Source: " + data.source;

        // Concept cards — clear then rebuild
        if (conceptCardsContainer == null || conceptCardPrefab == null) return;

        foreach (Transform child in conceptCardsContainer)
            Destroy(child.gameObject);

        foreach (ConceptEntry concept in data.concepts)
        {
            GameObject card = Instantiate(conceptCardPrefab, conceptCardsContainer);
            ConceptCardUI ui = card.GetComponent<ConceptCardUI>();
            if (ui != null)
                ui.Populate(concept.title, concept.description, concept.iconGlyph);
        }
    }

    // ── Button callbacks — wire these in the Inspector ─────────────────
    public void OnExploreARClicked()
    {
        UINavigator.Instance.ShowScreen(UINavigator.SCREEN_AR);
    }

    public void OnGoToFlashcardsClicked()
    {
        UINavigator.Instance.ShowScreen(UINavigator.SCREEN_FLASHCARD);
    }
}