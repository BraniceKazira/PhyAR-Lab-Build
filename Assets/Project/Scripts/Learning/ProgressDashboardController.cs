// ProgressDashboardController.cs
// Place in: Assets/_Project/Scripts/UI/
// Attach to: Screen_Progress panel
//
// Matches this hierarchy:
//   HeaderBar > NavBar > AppTitle, ScreenTitle, SessionTotal
//   MetricsRow > Metric_TopicsDone, Metric_QuizzesTake, Metric_AvgScore, Metric_ARSessions
//                each has MetricLabel + MetricNumber
//   TopicContainer > Card_Electromagne, Card_Waves, Current Electricity
//                    each has TopicName, TopicDetails, TopicStatus

using UnityEngine;
using TMPro;

public class ProgressDashboardController : MonoBehaviour
{
    [Header("Header")]
    public TMP_Text sessionTotalText;   // HeaderBar/NavBar/SessionTotal

    [Header("Metrics — drag MetricNumber TMP from each card")]
    public TMP_Text topicsDoneNumber;    // Metric_TopicsDone/MetricNumber
    public TMP_Text quizzesTakenNumber;  // Metric_QuizzesTake/MetricNumber
    public TMP_Text avgScoreNumber;      // Metric_AvgScore/MetricNumber
    public TMP_Text arSessionsNumber;    // Metric_ARSessions/MetricNumber

    [Header("Topic cards — drag each card's 3 TMP children in order")]
    [Header("Card_Electromagne")]
    public TMP_Text emTopicName;
    public TMP_Text emTopicDetails;
    public TMP_Text emTopicStatus;

    [Header("Card_Waves")]
    public TMP_Text wavesTopicName;
    public TMP_Text wavesTopicDetails;
    public TMP_Text wavesTopicStatus;

    [Header("Card_CurrentElectricity")]
    public TMP_Text ceTopicName;
    public TMP_Text ceTopicDetails;
    public TMP_Text ceTopicStatus;

    [Header("Session logger — drag AppManagers")]
    public SessionLogger sessionLogger;

    void OnEnable()
    {
        PopulateScreen();
    }

    void PopulateScreen()
    {
        // ── Session summary from logger ───────────────────────────────
        if (sessionLogger != null)
        {
            var (topicsDone, quizzesTaken, avgScore, totalCards) =
                sessionLogger.GetProgressSummary();

            if (sessionTotalText != null)
                sessionTotalText.text = $"Session total: {quizzesTaken} quizzes taken";

            if (topicsDoneNumber  != null) topicsDoneNumber.text   = $"{topicsDone}/3";
            if (quizzesTakenNumber != null) quizzesTakenNumber.text = quizzesTaken.ToString();
            if (avgScoreNumber    != null) avgScoreNumber.text     = $"{Mathf.RoundToInt(avgScore * 100)}%";
            if (arSessionsNumber  != null) arSessionsNumber.text   = "0"; // AR sessions tracked later
        }

        // ── Topic cards from topics.json ──────────────────────────────
        TopicListWrapper topics = ModuleLoader.Instance?.LoadTopicList();
        if (topics == null) return;

        SessionLogList logs = sessionLogger?.LoadAllSessions() ?? new SessionLogList();

        foreach (TopicEntry topic in topics.topics)
        {
            // Find best quiz score for this topic from session logs
            float bestScore = -1f;
            int quizCount   = 0;
            foreach (SessionLog log in logs.sessions)
            {
                if (log.topicID == topic.id)
                {
                    quizCount++;
                    float score = (float)log.quizScore / Mathf.Max(log.quizTotal, 1);
                    if (score > bestScore) bestScore = score;
                }
            }

            string statusText = topic.status == "shell" ? "Coming soon" :
                                bestScore < 0           ? "Not started" :
                                bestScore >= 0.7f       ? $"Quiz: {Mathf.RoundToInt(bestScore * 100)}% ✓" :
                                                          $"Quiz: {Mathf.RoundToInt(bestScore * 100)}%";

            string detailText = topic.status == "shell"
                ? topic.description
                : $"Quiz: {quizCount} attempt{(quizCount == 1 ? "" : "s")} · {topic.description}";

            // Assign to correct card based on topic ID
            switch (topic.id)
            {
                case "em_induction":
                    SetCard(emTopicName, emTopicDetails, emTopicStatus,
                            topic.displayName, detailText, statusText, bestScore);
                    break;
                case "waves_ii":
                    SetCard(wavesTopicName, wavesTopicDetails, wavesTopicStatus,
                            topic.displayName, detailText, statusText, bestScore);
                    break;
                case "current_electricity":
                    SetCard(ceTopicName, ceTopicDetails, ceTopicStatus,
                            topic.displayName, detailText, statusText, bestScore);
                    break;
            }
        }
    }

    void SetCard(TMP_Text name, TMP_Text details, TMP_Text status,
                 string nameText, string detailText, string statusText, float score)
    {
        if (name    != null) name.text    = nameText;
        if (details != null) details.text = detailText;
        if (status  != null)
        {
            status.text  = statusText;
            // Colour the status green if passed, orange if attempted but below 70%
            if (score >= 0.7f)
                status.color = new Color(0.180f, 0.769f, 0.710f); // Coil Green
            else if (score >= 0f)
                status.color = new Color(1f, 0.42f, 0.21f);       // Ember Orange
            else
                status.color = new Color(0.10f, 0.10f, 0.18f, 0.50f); // Muted
        }
    }

    public void OnHomeClicked()
    {
        UINavigator.Instance.GoHome();
    }
}