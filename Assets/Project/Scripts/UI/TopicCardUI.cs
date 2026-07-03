using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TopicCardUI : MonoBehaviour
{
    [Header("Set this in Inspector for manually placed cards")]
    [Tooltip("Must match the id field in topics.json — e.g. em_induction")]
    public string topicID;

    [Header("Text fields — leave empty if not built yet")]
    public TMP_Text topicNameText;
    public TMP_Text formText;
    public TMP_Text descriptionText;
    public TMP_Text statusBadgeText;

    [Header("Visual elements — leave empty if not built yet")]
    public Image accentBar;
    public Image statusBadgeImage;
    public Image progressFill;

    [Header("Status badge colours")]
    public Color fullARColor    = new Color(0.180f, 0.769f, 0.710f); // Coil Green
    public Color comingSoonColor = new Color(0.10f, 0.10f, 0.18f, 0.12f);

    void Start()
    {
        // Wire the button automatically so you don't need to set OnClick in Inspector
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnCardClicked);
        }
    }

    // Called by HomeScreenController for instantiated cards
    public void Populate(TopicEntry entry)
    {
        topicID = entry.id;

        if (topicNameText)   topicNameText.text   = entry.displayName;
        if (formText)        formText.text         = entry.form;
        if (descriptionText) descriptionText.text  = entry.description;

        if (accentBar != null &&
            ColorUtility.TryParseHtmlString(entry.accentColorHex, out Color accent))
            accentBar.color = accent;

        bool isFull = entry.status == "full";

        if (statusBadgeText)
            statusBadgeText.text = isFull ? "Full AR" : "Coming soon";

        if (statusBadgeImage)
            statusBadgeImage.color = isFull ? fullARColor : comingSoonColor;

        if (progressFill != null)
            progressFill.rectTransform.sizeDelta =
                new Vector2(0, progressFill.rectTransform.sizeDelta.y);

        // Disable tapping for shell topics
        Button btn = GetComponent<Button>();
        if (btn != null) btn.interactable = isFull;

        if (!isFull && topicNameText)
            topicNameText.color = new Color(0.10f, 0.10f, 0.18f, 0.40f);
    }

    void OnCardClicked()
    {
        if (string.IsNullOrEmpty(topicID))
        {
            Debug.LogWarning("[TopicCardUI] topicID is empty — set it in the Inspector.");
            return;
        }
        UINavigator.Instance.OpenTopic(topicID);
    }
}