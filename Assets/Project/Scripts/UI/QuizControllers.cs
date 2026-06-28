// QuizControllers.cs
// Place in: Assets/_Project/Scripts/UI/
// Contains: MCQOptionUI, QuizQuestionController,
//           QuizFeedbackController, QuizResultsController

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// ═══════════════════════════════════════════════════════════════════════════
//  MCQOptionUI
//  Attach to: each MCQOption prefab (Option_A, Option_B, Option_C, Option_D)
//  The QuizQuestionController calls Select() / Deselect() on each one.
// ═══════════════════════════════════════════════════════════════════════════

public class MCQOptionUI : MonoBehaviour
{
    public TMP_Text letterLabel;   // "A", "B", "C", "D"
    public TMP_Text optionText;    // the option content
    public Image cardBackground;
    public Image letterCircle;

    // Colours
    private Color _selectedCardColor  = new Color(0.722f, 0.451f, 0.200f, 0.10f); // Copper 10%
    private Color _unselectedCardColor = Color.white;
    private Color _selectedCircleColor = new Color(0.722f, 0.451f, 0.200f);       // Copper
    private Color _unselectedCircleColor = new Color(0.10f, 0.10f, 0.18f, 0.10f);
    private Color _selectedLetterColor = Color.white;
    private Color _unselectedLetterColor = new Color(0.10f, 0.10f, 0.18f, 0.50f);

    private int _optionIndex;
    private System.Action<int> _onSelectedCallback;

    public void Populate(int index, string letter, string text, System.Action<int> onSelected)
    {
        _optionIndex = index;
        _onSelectedCallback = onSelected;

        letterLabel.text = letter;
        optionText.text  = text;

        Deselect();

        // Wire button
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => _onSelectedCallback?.Invoke(_optionIndex));
        }
    }

    public void Select()
    {
        cardBackground.color  = _selectedCardColor;
        letterCircle.color    = _selectedCircleColor;
        letterLabel.color     = _selectedLetterColor;
    }

    public void Deselect()
    {
        cardBackground.color  = _unselectedCardColor;
        letterCircle.color    = _unselectedCircleColor;
        letterLabel.color     = _unselectedLetterColor;
    }

    // Called after submit to reveal correct/incorrect state
    public void ShowCorrect()
    {
        cardBackground.color = new Color(0.180f, 0.769f, 0.710f, 0.15f); // Coil Green tint
        letterCircle.color   = new Color(0.180f, 0.769f, 0.710f);
        letterLabel.color    = Color.white;
    }

    public void ShowIncorrect()
    {
        cardBackground.color = new Color(1f, 0.42f, 0.21f, 0.12f); // Ember Orange tint
        letterCircle.color   = new Color(1f, 0.42f, 0.21f);
        letterLabel.color    = Color.white;
    }
}


// ═══════════════════════════════════════════════════════════════════════════
//  QuizQuestionController
//  Attach to: Screen_QuizQuestion panel GameObject
// ═══════════════════════════════════════════════════════════════════════════

public class QuizQuestionController : MonoBehaviour
{
    [Header("Nav")]
    public TMP_Text navTitleText;

    [Header("Step pills")]
    public StepPillsController stepPills;

    [Header("Progress")]
    public TMP_Text progressCaption;     // "1/1 correct" badge
    public RectTransform progressFill;
    public float progressTrackWidth = 328f;

    [Header("Question card")]
    public TMP_Text questionNumberLabel; // "Q2" inside the left tab
    public TMP_Text questionText;

    [Header("Options — drag Option_A through Option_D here in order")]
    public List<MCQOptionUI> options;    // size 4

    [Header("Hint card")]
    public TMP_Text hintText;
    public GameObject hintCard;

    [Header("Buttons")]
    public Button submitButton;
    public Button skipButton;

    // Private state
    private int _selectedOptionIndex = -1; // -1 = nothing selected yet

    // ── Unity lifecycle ───────────────────────────────────────────────
    void OnEnable()
    {
        // Start a new quiz session if needed
        if (AppState.ActiveQuizSession == null ||
            AppState.ActiveQuizSession.TopicID != AppState.CurrentTopicID)
        {
            StartNewQuizSession();
        }

        _selectedOptionIndex = -1;
        RefreshUI();
    }

    void StartNewQuizSession()
    {
        QuizData quizData = ModuleLoader.Instance.LoadQuiz(AppState.CurrentTopicID);
        if (quizData == null)
        {
            Debug.LogError("[QuizQuestionController] Could not load quiz data.");
            return;
        }

        AppState.ActiveQuizSession = new QuizSession
        {
            TopicID = AppState.CurrentTopicID,
            QuizData = quizData
        };
    }

    // ── UI refresh ────────────────────────────────────────────────────
    void RefreshUI()
    {
        QuizSession session = AppState.ActiveQuizSession;
        if (session == null) return;

        QuizQuestion q = session.CurrentQuestion;
        int qNum       = session.CurrentQuestionIndex + 1;
        int total      = session.TotalQuestions;

        // Nav
        navTitleText.text = $"Topic Quiz · {AppState.CurrentTopicID.Replace("_"," ")} · Q{qNum} of {total}";

        // Step pills — Quiz is step index 3
        if (stepPills != null) stepPills.SetActiveStep(3);

        // Progress bar
        float fill = (float)session.CurrentQuestionIndex / total;
        progressFill.sizeDelta = new Vector2(progressTrackWidth * fill, progressFill.sizeDelta.y);

        // Correct count badge
        progressCaption.text = $"{session.Score}/{session.SelectedAnswers.Count} correct";

        // Question
        questionNumberLabel.text = $"Q{qNum}";
        questionText.text = q.questionText;

        // Options — deselect all, populate text
        string[] letters = { "A", "B", "C", "D" };
        for (int i = 0; i < options.Count && i < q.options.Count; i++)
        {
            options[i].gameObject.SetActive(true);
            options[i].Populate(i, letters[i], q.options[i], OnOptionSelected);
            options[i].Deselect();
        }

        // Hint
        if (!string.IsNullOrEmpty(q.hint))
        {
            hintCard.SetActive(true);
            hintText.text = q.hint;
        }
        else
        {
            hintCard.SetActive(false);
        }

        // Submit disabled until an option is tapped
        submitButton.interactable = false;
    }

    // ── Button callbacks ──────────────────────────────────────────────

    void OnOptionSelected(int index)
    {
        _selectedOptionIndex = index;

        // Visually update all options
        for (int i = 0; i < options.Count; i++)
        {
            if (i == index) options[i].Select();
            else options[i].Deselect();
        }

        submitButton.interactable = true;
    }

    public void OnSubmitClicked()
    {
        if (_selectedOptionIndex < 0) return;

        QuizSession session = AppState.ActiveQuizSession;
        if (session == null) return;

        // Record the answer
        bool correct = _selectedOptionIndex == session.CurrentQuestion.correctOptionIndex;
        session.SelectedAnswers.Add(_selectedOptionIndex);

        // Pass result to AppState for the feedback screen
        AppState.LastAnswerWasCorrect    = correct;
        AppState.LastSelectedOptionIndex = _selectedOptionIndex;

        // Briefly show correct/incorrect on the option before transitioning
        options[_selectedOptionIndex].ShowIncorrect(); // assume wrong first
        if (correct)
            options[_selectedOptionIndex].ShowCorrect();
        options[session.CurrentQuestion.correctOptionIndex].ShowCorrect();

        // Navigate to feedback
        UINavigator.Instance.ShowScreen(UINavigator.SCREEN_QUIZ_FB);
    }

    public void OnSkipClicked()
    {
        QuizSession session = AppState.ActiveQuizSession;
        if (session == null) return;

        // Record skip as -1
        session.SelectedAnswers.Add(-1);
        AppState.LastAnswerWasCorrect    = false;
        AppState.LastSelectedOptionIndex = -1;

        UINavigator.Instance.ShowScreen(UINavigator.SCREEN_QUIZ_FB);
    }
}


// ═══════════════════════════════════════════════════════════════════════════
//  QuizFeedbackController
//  Attach to: Screen_QuizFeedback panel GameObject
// ═══════════════════════════════════════════════════════════════════════════

public class QuizFeedbackController : MonoBehaviour
{
    [Header("Feedback banner")]
    public Image bannerBackground;
    public TMP_Text bannerHeading;
    public TMP_Text bannerSubtext;

    private Color _correctColor   = new Color(0.180f, 0.769f, 0.710f); // Coil Green
    private Color _incorrectColor = new Color(1f, 0.42f, 0.21f);       // Ember Orange

    [Header("Question recap")]
    public TMP_Text questionRecapText;

    [Header("Your answer")]
    public TMP_Text yourAnswerText;

    [Header("Explanation card")]
    public TMP_Text explanationText;
    public TMP_Text referenceText;

    [Header("Progress")]
    public RectTransform progressFill;
    public float progressTrackWidth = 328f;

    [Header("Next button")]
    public Button nextButton;
    public TMP_Text nextButtonText;

    void OnEnable()
    {
        QuizSession session = AppState.ActiveQuizSession;
        if (session == null) return;

        PopulateScreen(session);
    }

    void PopulateScreen(QuizSession session)
    {
        QuizQuestion q     = session.CurrentQuestion;
        bool correct       = AppState.LastAnswerWasCorrect;
        int selectedIndex  = AppState.LastSelectedOptionIndex;
        bool wasSkipped    = selectedIndex < 0;

        // Feedback banner
        bannerBackground.color = correct ? _correctColor : _incorrectColor;
        bannerHeading.text     = correct ? "Correct!" : (wasSkipped ? "Skipped" : "Not quite");
        bannerSubtext.text     = correct
            ? "Well done — keep going!"
            : "Review the explanation below.";

        // Question recap (shortened)
        string shortQ = q.questionText.Length > 80
            ? q.questionText.Substring(0, 80) + "…"
            : q.questionText;
        questionRecapText.text = shortQ;

        // Your answer
        if (wasSkipped)
        {
            yourAnswerText.text = "You skipped this question.";
        }
        else
        {
            string letter = ((char)('A' + selectedIndex)).ToString();
            string status = correct ? "✓" : "✗";
            yourAnswerText.text = $"Your answer: {letter} · {q.options[selectedIndex]} {status}";
        }

        // Explanation
        explanationText.text = q.explanation;
        referenceText.text   = q.reference;

        // Progress bar
        float fill = (float)(session.CurrentQuestionIndex + 1) / session.TotalQuestions;
        progressFill.sizeDelta = new Vector2(progressTrackWidth * fill, progressFill.sizeDelta.y);

        // Next button label
        nextButtonText.text = session.IsLastQuestion ? "See results →" : "Next question →";
    }

    public void OnNextClicked()
    {
        QuizSession session = AppState.ActiveQuizSession;
        if (session == null) return;

        if (session.IsLastQuestion)
        {
            UINavigator.Instance.ShowScreen(UINavigator.SCREEN_QUIZ_RESULTS);
        }
        else
        {
            // Advance to next question
            session.CurrentQuestionIndex++;
            UINavigator.Instance.ShowScreen(UINavigator.SCREEN_QUIZ_Q);
        }
    }
}


// ═══════════════════════════════════════════════════════════════════════════
//  QuizResultsController
//  Attach to: Screen_QuizResults panel GameObject
// ═══════════════════════════════════════════════════════════════════════════

public class QuizResultsController : MonoBehaviour
{
    [Header("Score ring")]
    public Image scoreRingImage;      // Filled, Radial360 image
    public TMP_Text scoreFractionText; // "5/6"
    public TMP_Text scorePercentText;  // "83%"

    [Header("Heading")]
    public TMP_Text headingText;

    [Header("Breakdown bars")]
    public TMP_Text correctCountText;
    public TMP_Text incorrectCountText;
    public TMP_Text skippedCountText;
    public RectTransform correctFillRect;
    public RectTransform incorrectFillRect;
    public RectTransform skippedFillRect;
    public float barTrackWidth = 200f;

    [Header("Per-question list")]
    public Transform perQuestionListContainer;
    public GameObject perQuestionRowPrefab;

    [Header("Session logger")]
    public SessionLogger sessionLogger;

    void OnEnable()
    {
        QuizSession session = AppState.ActiveQuizSession;
        if (session == null) return;

        PopulateScreen(session);
        LogSession(session);
    }

    void PopulateScreen(QuizSession session)
    {
        int total     = session.TotalQuestions;
        int correct   = session.Score;
        int skipped   = 0;
        int incorrect = 0;

        for (int i = 0; i < session.SelectedAnswers.Count; i++)
        {
            if (session.SelectedAnswers[i] < 0) skipped++;
            else if (session.SelectedAnswers[i] != session.QuizData.questions[i].correctOptionIndex)
                incorrect++;
        }

        float pct = (float)correct / Mathf.Max(total, 1);

        // Score ring
        scoreRingImage.fillAmount = pct;
        scoreFractionText.text    = $"{correct}/{total}";
        scorePercentText.text     = $"{Mathf.RoundToInt(pct * 100)}%";

        // Heading
        headingText.text = correct == total ? "Perfect score!" :
                           pct >= 0.7f      ? "Great work!"    :
                           pct >= 0.5f      ? "Good effort!"   :
                                             "Keep practising!";

        // Breakdown
        correctCountText.text   = $"Correct {correct}";
        incorrectCountText.text = $"Incorrect {incorrect}";
        skippedCountText.text   = $"Skipped {skipped}";

        SetBarWidth(correctFillRect,   correct,   total);
        SetBarWidth(incorrectFillRect, incorrect, total);
        SetBarWidth(skippedFillRect,   skipped,   total);

        // Per-question list
        BuildPerQuestionList(session);
    }

    void SetBarWidth(RectTransform fill, int value, int total)
    {
        float w = (float)value / Mathf.Max(total, 1) * barTrackWidth;
        fill.sizeDelta = new Vector2(w, fill.sizeDelta.y);
    }

    void BuildPerQuestionList(QuizSession session)
    {
        foreach (Transform child in perQuestionListContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < session.QuizData.questions.Count; i++)
        {
            int selected = i < session.SelectedAnswers.Count ? session.SelectedAnswers[i] : -1;
            bool correct = selected >= 0 && selected == session.QuizData.questions[i].correctOptionIndex;
            bool skipped = selected < 0;

            GameObject row = Instantiate(perQuestionRowPrefab, perQuestionListContainer);
            PerQuestionRowUI rowUI = row.GetComponent<PerQuestionRowUI>();
            if (rowUI != null)
            {
                string shortQ = session.QuizData.questions[i].questionText;
                if (shortQ.Length > 55) shortQ = shortQ.Substring(0, 55) + "…";

                string selectedLabel = skipped ? "Skipped"
                    : ((char)('A' + selected)).ToString() + " · " + session.QuizData.questions[i].options[selected];

                rowUI.Populate(i + 1, shortQ, selectedLabel, correct, skipped);
            }
        }
    }

    void LogSession(QuizSession session)
    {
        if (sessionLogger == null) return;

        FlashcardSession fc = AppState.ActiveFlashcardSession;
        sessionLogger.LogSession(
            topicID: session.TopicID,
            fcKnown: fc?.KnownCardIDs.Count ?? 0,
            fcStillLearning: fc?.StillLearningCardIDs.Count ?? 0,
            fcSkipped: fc?.SkippedCardIDs.Count ?? 0,
            quizScore: session.Score,
            quizTotal: session.TotalQuestions
        );
    }

    // ── Buttons ───────────────────────────────────────────────────────

    public void OnRetryQuizClicked()
    {
        // Clear quiz session so it restarts from Q1
        AppState.ActiveQuizSession = null;
        UINavigator.Instance.ShowScreen(UINavigator.SCREEN_QUIZ_Q);
    }

    public void OnNextTopicClicked()
    {
        UINavigator.Instance.GoHome();
    }
}


// ═══════════════════════════════════════════════════════════════════════════
//  PerQuestionRowUI
//  Attach to: the per-question result row prefab
// ═══════════════════════════════════════════════════════════════════════════

public class PerQuestionRowUI : MonoBehaviour
{
    public Image statusIcon;
    public TMP_Text questionLabel;
    public TMP_Text selectionLabel;

    private Color _correctColor   = new Color(0.180f, 0.769f, 0.710f);
    private Color _incorrectColor = new Color(1f, 0.42f, 0.21f);
    private Color _skippedColor   = new Color(0.10f, 0.10f, 0.18f, 0.25f);

    public void Populate(int number, string question, string selection, bool correct, bool skipped)
    {
        questionLabel.text  = $"Q{number}. {question}";
        selectionLabel.text = selection;
        statusIcon.color    = skipped ? _skippedColor :
                              correct ? _correctColor  : _incorrectColor;
    }
}