// TopicCardUI.cs
// Place in: Assets/_Project/Scripts/UI/
// Attach to: the TopicCard prefab root GameObject
//
// This script holds references to all the UI elements inside one TopicCard.
// HomeScreenController instantiates TopicCard prefabs and then calls
// Populate() on each one to fill in the topic-specific content.

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TopicCardUI : MonoBehaviour
{
    [Header("Text fields")]
    public TMP_Text topicNameText;    // "Electromagnetic Induction"
    public TMP_Text formText;         // "Form 4"
    public TMP_Text descriptionText;  // "Faraday, Lenz, generators..."
    public TMP_Text statusBadgeText;  // "Full AR" or "Coming soon"

    [Header("Visual elements")]
    public Image accentBar;           // left-edge colour strip
    public Image statusBadgeImage;    // background of the status badge
    public Image progressFill;        // the coloured fill inside progress bar

    [Header("Colours for status badge")]
    public Color fullARColor   = new Color(0.18f, 0.77f, 0.71f); // #2EC4B6 Coil Green
    public Color comingSoonColor = new Color(0.10f, 0.10f, 0.18f, 0.12f); // Deep Indigo faint

    // Stored so the button click handler can pass it back
    private string _topicID;

    /// <summary>
    /// Called by HomeScreenController after instantiating this prefab.
    /// Fills all UI fields from the TopicEntry data.
    /// </summary>
    public void Populate(TopicEntry entry)
    {
        _topicID = entry.id;

        topicNameText.text = entry.displayName;
        formText.text = entry.form;
        descriptionText.text = entry.description;

        // Accent bar colour
        if (ColorUtility.TryParseHtmlString(entry.accentColorHex, out Color accent))
            accentBar.color = accent;

        // Status badge
        bool isFull = entry.status == "full";
        statusBadgeText.text = isFull ? "Full AR" : "Coming soon";
        statusBadgeImage.color = isFull ? fullARColor : comingSoonColor;
        statusBadgeText.color = isFull
            ? Color.white
            : new Color(0.10f, 0.10f, 0.18f, 0.50f);

        // Progress bar — zero for now, updated by LearningProgressTracker later
        if (progressFill != null)
            progressFill.rectTransform.sizeDelta = new Vector2(0, progressFill.rectTransform.sizeDelta.y);

        // Wire the card's Button component to OnCardClicked
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnCardClicked);
        }

        // Disable interaction for shell topics (Coming soon)
        if (btn != null) btn.interactable = isFull;
        if (!isFull) topicNameText.color = new Color(0.10f, 0.10f, 0.18f, 0.40f);
    }

    private void OnCardClicked()
    {
        UINavigator.Instance.OpenTopic(_topicID);
    }
}