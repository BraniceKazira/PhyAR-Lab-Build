using UnityEngine;
using TMPro;
using UnityEngine.UI; // For Image support

public class ProgressDashboardController : MonoBehaviour
{
    // ── Header ──────────────────────────────────────────────────────────
    // Shows the total session summary at the top

    [Header("Header")]
    public TMP_Text sessionTotalText;   // e.g. "Session total: 3 quizzes taken"

    // ── Metrics Row ──────────────────────────────────────────────────
    // Four small cards showing overall stats

    [Header("Metrics — drag MetricNumber TMP from each card")]
    public TMP_Text topicsDoneNumber;    // "2/3"
    public TMP_Text quizzesTakenNumber;  // "2"
    public TMP_Text avgScoreNumber;      // "80%"
    public TMP_Text arSessionsNumber;    // "7"

    // ── Topic Cards ──────────────────────────────────────────────────
    // Three cards showing progress per topic

    [Header("Topic card texts — drag each card's 3 TMP children")]
    [Header("Card_Electromagne")]
    public TMP_Text emTopicName;       // "Electromagnetic Induction"
    public TMP_Text emTopicDetails;    // "Quiz: 2 attempts · Faraday, Lenz..."
    public TMP_Text emTopicStatus;     // "Quiz: 90% ✓"

    [Header("Card_Waves")]
    public TMP_Text wavesTopicName;
    public TMP_Text wavesTopicDetails;
    public TMP_Text wavesTopicStatus;

    [Header("Card_CurrentElectricity")]
    public TMP_Text ceTopicName;
    public TMP_Text ceTopicDetails;
    public TMP_Text ceTopicStatus;

    // ── Topic Icons ──────────────────────────────────────────────────
    // Small icons next to each topic name

    [Header("Topic Icons — drag the Image component from each topic card")]
    public Image emTopicIcon;          // Icon for Electromagnetic Induction
    public Image wavesTopicIcon;       // Icon for Waves II
    public Image ceTopicIcon;          // Icon for Current Electricity II

    [Header("Session logger — drag AppManagers")]
    public SessionLogger sessionLogger;

    // ── Called when the screen becomes visible ──────────────────────

    void OnEnable()
    {
        PopulateScreen();
    }

    // ── Populate everything on the screen ───────────────────────────

    void PopulateScreen()
    {
        // Load session data from the logger
        if (sessionLogger != null)
        {
            var (topicsDone, quizzesTaken, avgScore, totalCards) =
                sessionLogger.GetProgressSummary();

            // Update the session total text
            if (sessionTotalText != null)
                sessionTotalText.text = $"Session total: {quizzesTaken} quizzes taken";

            // Update the four metric numbers
            if (topicsDoneNumber   != null) topicsDoneNumber.text   = $"{topicsDone}/3";
            if (quizzesTakenNumber != null) quizzesTakenNumber.text = quizzesTaken.ToString();
            if (avgScoreNumber     != null) avgScoreNumber.text     = $"{Mathf.RoundToInt(avgScore * 100)}%";
            if (arSessionsNumber   != null) arSessionsNumber.text   = "0"; // AR sessions tracked later
        }

        // Load topic data from the JSON file
        TopicListWrapper topics = ModuleLoader.Instance?.LoadTopicList();
        if (topics == null) return;

        // Load all saved session logs
        SessionLogList logs = sessionLogger?.LoadAllSessions() ?? new SessionLogList();

        // Loop through each topic and update its card
        foreach (TopicEntry topic in topics.topics)
        {
            // Find the best quiz score and count attempts for this topic
            float bestScore = -1f;
            int quizCount = 0;
            foreach (SessionLog log in logs.sessions)
            {
                if (log.topicID == topic.id)
                {
                    quizCount++;
                    float score = (float)log.quizScore / Mathf.Max(log.quizTotal, 1);
                    if (score > bestScore) bestScore = score;
                }
            }

            // Work out what status text to show
            // - "Coming soon" for shell topics (not fully built)
            // - "Not started" if no quiz attempts
            // - "Quiz: 90% ✓" if passed (70%+)
            // - "Quiz: 60%" if attempted but below 70%
            string statusText = topic.status == "shell" ? "Coming soon" :
                                bestScore < 0           ? "Not started" :
                                bestScore >= 0.7f       ? $"Quiz: {Mathf.RoundToInt(bestScore * 100)}% ✓" :
                                                          $"Quiz: {Mathf.RoundToInt(bestScore * 100)}%";

            // Build the details text
            string detailText = topic.status == "shell"
                ? topic.description
                : $"Quiz: {quizCount} attempt{(quizCount == 1 ? "" : "s")} · {topic.description}";

            // Send the data to the correct card
            switch (topic.id)
            {
                case "em_induction":
                    SetCard(emTopicName, emTopicDetails, emTopicStatus, emTopicIcon,
                            topic.displayName, detailText, statusText, bestScore);
                    break;
                case "waves_ii":
                    SetCard(wavesTopicName, wavesTopicDetails, wavesTopicStatus, wavesTopicIcon,
                            topic.displayName, detailText, statusText, bestScore);
                    break;
                case "current_electricity":
                    SetCard(ceTopicName, ceTopicDetails, ceTopicStatus, ceTopicIcon,
                            topic.displayName, detailText, statusText, bestScore);
                    break;
            }
        }
    }

    // ── Update a single topic card ──────────────────────────────────

    void SetCard(TMP_Text name, TMP_Text details, TMP_Text status, Image icon,
                 string nameText, string detailText, string statusText, float score)
    {
        // Update the text fields
        if (name    != null) name.text    = nameText;
        if (details != null) details.text = detailText;

        if (status != null)
        {
            status.text = statusText;

            // Colour the status based on performance:
            // Green = passed (70%+)
            // Orange = attempted but below 70%
            // Grey = not attempted or shell topic
            if (score >= 0.7f)
                status.color = new Color(0.180f, 0.769f, 0.710f); // Coil Green
            else if (score >= 0f)
                status.color = new Color(1f, 0.42f, 0.21f);       // Ember Orange
            else
                status.color = new Color(0.10f, 0.10f, 0.18f, 0.50f); // Muted
        }

        // ── Set the topic icon ──────────────────────────────────────
        // The icon will be loaded from the Sprite set in the Inspector
        // or from Resources/Icons/ if you prefer.
        // If you want to load from Resources, uncomment the code below.
        /*
        if (icon != null && !string.IsNullOrEmpty(topicIconName))
        {
            Sprite loadedIcon = Resources.Load<Sprite>("Icons/" + topicIconName);
            if (loadedIcon != null)
            {
                icon.sprite = loadedIcon;
                icon.enabled = true;
            }
            else
            {
                icon.enabled = false;
            }
        }
        */
        // For now, the icon sprite can be set manually in the Inspector.
    }

    // ── Button callbacks ─────────────────────────────────────────────

    public void OnHomeClicked()
    {
        UINavigator.Instance.GoHome();
    }
}