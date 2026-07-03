using UnityEngine;

public class HomeScreenController : MonoBehaviour
{
    void OnEnable()
    {
        // Guard against running before AppManagers has initialised
        if (ModuleLoader.Instance == null)
        {
            Debug.LogWarning("[HomeScreen] ModuleLoader not ready yet — skipping populate.");
            return;
        }

        TopicCardUI[] cards = GetComponentsInChildren<TopicCardUI>(includeInactive: true);
        if (cards.Length == 0) return;

        TopicListWrapper data = ModuleLoader.Instance.LoadTopicList();
        if (data == null) return;

        foreach (TopicCardUI card in cards)
        {
            if (string.IsNullOrEmpty(card.topicID)) continue;
            TopicEntry entry = data.topics.Find(t => t.id == card.topicID);
            if (entry != null) card.Populate(entry);
        }
    }
}