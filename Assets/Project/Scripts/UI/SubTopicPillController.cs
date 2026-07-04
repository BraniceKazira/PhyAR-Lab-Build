using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Small data class that describes one sub-topic pill entry.
// Used by FlashcardDeckController and ARExperienceController.
[System.Serializable]
public class SubTopicEntry
{
    public string id;        // "faraday"
    public string shortName; // "Faraday"
}

/// Attach to the SubTopicPillRow GameObject (the one with Horizontal Layout Group).
/// Controls the colour state of each pill based on which sub-topic is active.
///
/// HOW TO WIRE IN INSPECTOR:
///   Pills array size = number of sub-topics for your longest topic (5).
///   For each pill slot, drag:
///     - toggle   → the Toggle component on that pill
///     - background → the Image component on that pill
///     - label    → the TMP_Text child inside that pill

public class SubTopicPillController : MonoBehaviour
{
    [System.Serializable]
    public class PillData
    {
        public UnityEngine.UI.Toggle toggle;
        public Image background;
        public TMP_Text label;
        [HideInInspector] public string subTopicID;
    }

    public List<PillData> pills;

    // Colours for the AR Experience screen (dark background variant)
    [Header("Active pill colours")]
    public Color activeBackground = new Color(0.722f, 0.451f, 0.200f); // Copper
    public Color activeLabel      = Color.white;

    [Header("Inactive pill colours — change to white variants for dark screens")]
    public Color inactiveBackground = new Color(0.10f, 0.10f, 0.18f, 0.08f);
    public Color inactiveLabel      = new Color(0.10f, 0.10f, 0.18f, 0.50f);

    // Optional callback — FlashcardDeckController can subscribe to know
    // when the student taps a different sub-topic
    public System.Action<string> OnSubTopicSelected;

    void Start()
    {
        // ✅ Guard against null pills list
        if (pills == null)
        {
            Debug.LogError("[SubTopicPills] pills list is NULL! Assign in Inspector.");
            return;
        }

        for (int i = 0; i < pills.Count; i++)
        {
            // ✅ Guard against null toggle
            if (pills[i].toggle == null)
            {
                Debug.LogWarning($"[SubTopicPills] Pill {i} has no Toggle assigned. Skipping.");
                continue;
            }

            int captured = i; // capture for lambda
            pills[i].toggle.onValueChanged.AddListener(
                isOn => OnPillToggled(captured, isOn));
        }
        RefreshAllVisuals();
    }

    // ── Called by screen controllers to populate pills from JSON data ──

    /// Populates the pill row with sub-topic data from the loaded JSON.
    /// Extra pills (if topic has fewer sub-topics than pill slots) are hidden.
    public void SetSubTopics(SubTopicEntry[] subTopics)
    {
        // ✅ Guard against null pills list
        if (pills == null)
        {
            Debug.LogError("[SubTopicPills] pills list is NULL! Cannot set sub-topics.");
            return;
        }

        // ✅ Guard against null or empty subTopics
        if (subTopics == null || subTopics.Length == 0)
        {
            Debug.LogWarning("[SubTopicPills] No sub-topics provided. Hiding all pills.");
            foreach (var pill in pills)
            {
                if (pill != null && pill.toggle != null)
                    pill.toggle.gameObject.SetActive(false);
            }
            return;
        }

        for (int i = 0; i < pills.Count; i++)
        {
            // ✅ Guard against null pill data
            if (pills[i] == null)
            {
                Debug.LogWarning($"[SubTopicPills] Pill {i} is NULL. Skipping.");
                continue;
            }

            if (i < subTopics.Length)
            {
                if (pills[i].toggle != null)
                    pills[i].toggle.gameObject.SetActive(true);
                if (pills[i].label != null)
                    pills[i].label.text = subTopics[i].shortName;
                pills[i].subTopicID = subTopics[i].id;
            }
            else
            {
                // Hide unused pills for topics with fewer sub-topics
                if (pills[i].toggle != null)
                    pills[i].toggle.gameObject.SetActive(false);
            }
        }

        // Select the first pill by default
        if (pills.Count > 0 && subTopics.Length > 0 && pills[0] != null && pills[0].toggle != null)
        {
            pills[0].toggle.isOn = true;
            SetActiveByIndex(0);
        }
    }

    /// Directly activate a pill by index (used when resuming a session
    /// mid-sub-topic so the correct pill is highlighted).
    public void SetActiveByIndex(int index)
    {
        // ✅ Guard against null pills list
        if (pills == null)
        {
            Debug.LogError("[SubTopicPills] pills list is NULL! Cannot set active.");
            return;
        }

        for (int i = 0; i < pills.Count; i++)
        {
            // ✅ Guard against null pill data
            if (pills[i] == null) continue;
            UpdatePillVisual(pills[i], i == index);
        }
    }

    // ── Private ───────────────────────────────────────────────────────

    void OnPillToggled(int index, bool isOn)
    {
        // ✅ Guard against index out of range
        if (pills == null || index < 0 || index >= pills.Count || pills[index] == null)
        {
            Debug.LogWarning($"[SubTopicPills] Invalid pill index: {index}");
            return;
        }

        UpdatePillVisual(pills[index], isOn);
        if (isOn)
            OnSubTopicSelected?.Invoke(pills[index].subTopicID);
    }

    void UpdatePillVisual(PillData pill, bool isActive)
    {
        // ✅ Guard against null pill
        if (pill == null) return;

        if (pill.background != null)
            pill.background.color = isActive ? activeBackground : inactiveBackground;
        if (pill.label != null)
            pill.label.color = isActive ? activeLabel : inactiveLabel;
    }

    void RefreshAllVisuals()
    {
        // ✅ Guard against null pills list
        if (pills == null)
        {
            Debug.LogError("[SubTopicPills] pills list is NULL! Cannot refresh visuals.");
            return;
        }

        for (int i = 0; i < pills.Count; i++)
        {
            if (pills[i] != null && pills[i].toggle != null)
                UpdatePillVisual(pills[i], pills[i].toggle.isOn);
        }
    }
}