// QuizQuestionController.cs
// No feedback screen — goes directly to results after the last question.

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class QuizQuestionController : MonoBehaviour
{
    [Header("Nav")]
    public TMP_Text navTitleText;

    [Header("Step pills")]
    public StepPillsController stepPills;

    [Header("Progress")]
    public RectTransform progressFillRect;
    public float progressTrackWidth = 328f;
    public TMP_Text correctCountText;

    [Header("Question card")]
    public TMP_Text questionNumberLabel;
    public TMP_Text questionText;

    [Header("Options — drag Option_A through Option_D in order")]
    public List<MCQOptionUI> options;

    [Header("Hint")]
    public GameObject hintCard;
    public TMP_Text hintText;

    [Header("Buttons")]
    public Button submitButton;
    public Button skipButton;

    private int _selectedOptionIndex = -1;

    void OnEnable()
    {
        Debug.Log("[QuizQuestion] OnEnable. Topic = " + AppState.CurrentTopicID);
        if (ModuleLoader.Instance == null) return;

        if (AppState.ActiveQuizSession == null ||
            AppState.ActiveQuizSession.TopicID != AppState.CurrentTopicID)
            StartNewQuizSession();

        _selectedOptionIndex = -1;

        if (submitButton != null)
        {
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(OnSubmitClicked);
        }
        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(OnSkipClicked);
        }

        RefreshUI();
    }

    void StartNewQuizSession()
    {
        QuizData quizData = ModuleLoader.Instance.LoadQuiz(AppState.CurrentTopicID);
        if (quizData == null) { Debug.LogError("[QuizQuestion] No quiz data for: " + AppState.CurrentTopicID); return; }
        AppState.ActiveQuizSession = new QuizSession { TopicID = AppState.CurrentTopicID, QuizData = quizData };
        Debug.Log("[QuizQuestion] Session started. " + quizData.questions.Count + " questions.");
    }

    void RefreshUI()
    {
        QuizSession session = AppState.ActiveQuizSession;
        if (session == null) return;

        int qNum  = session.CurrentQuestionIndex + 1;
        int total = session.TotalQuestions;
        QuizQuestion q = session.CurrentQuestion;

        if (navTitleText != null)      navTitleText.text      = $"Topic Quiz · Q{qNum} of {total}";
        if (questionNumberLabel != null) questionNumberLabel.text = $"Q{qNum}";
        if (questionText != null)      questionText.text       = q.questionText;
        if (correctCountText != null)  correctCountText.text   = $"{session.Score}/{session.SelectedAnswers.Count} correct";

        if (stepPills != null) stepPills.SetActiveStep(2);

        if (progressFillRect != null)
        {
            float fill = (float)session.CurrentQuestionIndex / total;
            progressFillRect.sizeDelta = new Vector2(progressTrackWidth * fill, progressFillRect.sizeDelta.y);
        }

        string[] letters = { "A", "B", "C", "D" };
        for (int i = 0; i < options.Count && i < q.options.Count; i++)
        {
            options[i].gameObject.SetActive(true);
            options[i].Populate(i, letters[i], q.options[i], OnOptionSelected);
            options[i].Deselect();
        }

        if (hintCard != null) hintCard.SetActive(!string.IsNullOrEmpty(q.hint));
        if (hintText != null && !string.IsNullOrEmpty(q.hint)) hintText.text = q.hint;

        if (submitButton != null) submitButton.interactable = false;
        _selectedOptionIndex = -1;
    }

    void OnOptionSelected(int index)
    {
        _selectedOptionIndex = index;
        for (int i = 0; i < options.Count; i++)
        {
            if (i == index) options[i].Select();
            else            options[i].Deselect();
        }
        if (submitButton != null) submitButton.interactable = true;
    }

    void GoToNextOrResults()
    {
        QuizSession session = AppState.ActiveQuizSession;
        if (session.IsLastQuestion)
            UINavigator.Instance.ShowScreen(UINavigator.SCREEN_QUIZ_RESULTS);
        else
        {
            session.CurrentQuestionIndex++;
            RefreshUI(); // stay on same screen, just load next question
        }
    }

    public void OnSubmitClicked()
    {
        if (_selectedOptionIndex < 0) return;
        QuizSession session = AppState.ActiveQuizSession;
        if (session == null) return;

        session.SelectedAnswers.Add(_selectedOptionIndex);
        AppState.LastAnswerWasCorrect    = _selectedOptionIndex == session.CurrentQuestion.correctOptionIndex;
        AppState.LastSelectedOptionIndex = _selectedOptionIndex;

        GoToNextOrResults();
    }

    public void OnSkipClicked()
    {
        QuizSession session = AppState.ActiveQuizSession;
        if (session == null) return;

        session.SelectedAnswers.Add(-1);
        AppState.LastAnswerWasCorrect    = false;
        AppState.LastSelectedOptionIndex = -1;

        GoToNextOrResults();
    }
}