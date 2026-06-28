using UnityEngine;

public class HomeScreenController : MonoBehaviour
{
    [Header("Drag the Content child of your TopicScrollView here")]
    public Transform topicListContainer;

    [Header("Drag the TopicCard prefab from Assets/_Project/Prefabs/UI/")]
    public GameObject topicCardPrefab;

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

        foreach (Transform child in topicListContainer)
            Destroy(child.gameObject);

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