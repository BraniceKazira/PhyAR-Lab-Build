// TopicCardUI.cs — Updated with icon support (Priority 2.1)
// Attach to: each topic card prefab/GameObject

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TopicCardUI : MonoBehaviour
{
    [Header("Set topicID in Inspector for manually placed cards")]
    public string topicID;

    [Header("Text fields")]
    public TMP_Text topicNameText;
    public TMP_Text formText;
    public TMP_Text descriptionText;
    public TMP_Text statusBadgeText;

    [Header("Icon (Priority 2.1)")]
    public Image topicIcon;  // drag the Image component for the icon

    [Header("Visual elements")]
    public Image accentBar;
    public Image statusBadgeImage;
    public Image progressFill;

    private readonly Color _fullARColor    = new Color(0.180f, 0.769f, 0.710f);
    private readonly Color _comingSoonColor= new Color(0.10f, 0.10f, 0.18f, 0.12f);

    void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnCardClicked);
        }
    }

    public void Populate(TopicEntry entry)
    {
        topicID = entry.id;

        if (topicNameText   != null) topicNameText.text   = entry.displayName;
        if (formText        != null) formText.text         = entry.form;
        if (descriptionText != null) descriptionText.text  = entry.description;

        // Accent bar colour
        if (accentBar != null &&
            ColorUtility.TryParseHtmlString(entry.accentColorHex, out Color c))
            accentBar.color = c;

        bool isFull = entry.status == "full";
        if (statusBadgeText  != null)
            statusBadgeText.text  = isFull ? "Full AR" : "Coming soon";
        if (statusBadgeImage != null)
            statusBadgeImage.color = isFull ? _fullARColor : _comingSoonColor;

        // Priority 2.1 — load icon from Resources/Icons/
        if (topicIcon != null && !string.IsNullOrEmpty(entry.iconName))
        {
            Sprite icon = Resources.Load<Sprite>("Icons/" + entry.iconName);
            if (icon != null)
                topicIcon.sprite = icon;
            else
                Debug.LogWarning($"[TopicCard] Icon not found: Icons/{entry.iconName}");
        }

        Button btn = GetComponent<Button>();
        if (btn != null) btn.interactable = isFull;
        if (!isFull && topicNameText != null)
            topicNameText.color = new Color(0.10f, 0.10f, 0.18f, 0.40f);
    }

    void OnCardClicked()
    {
        if (string.IsNullOrEmpty(topicID))
        {
            Debug.LogWarning("[TopicCard] topicID empty.");
            return;
        }
        UINavigator.Instance.OpenTopic(topicID);
    }
}