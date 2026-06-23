// DataModels.cs
// Place in: Assets/_Project/Scripts/Data/
//
// Every class in this file maps 1-to-1 to a JSON structure.
// Unity's JsonUtility reads these — field names must exactly match
// the JSON key names (case-sensitive).
//
// Also contains the runtime session state classes (FlashcardSession,
// QuizSession) which are NOT serialized to JSON — they only live in
// memory while the app is running.

using System;
using System.Collections.Generic;

// ════════════════════════════════════════════════════════════════════
//  TOPICS  (Resources/Data/topics.json)
// ════════════════════════════════════════════════════════════════════

[Serializable]
public class TopicListWrapper
{
    public List<TopicEntry> topics;
}

[Serializable]
public class TopicEntry
{
    public string id;              // "em_induction"
    public string displayName;     // "Electromagnetic Induction"
    public string form;            // "Form 4"
    public string chapter;         // "Chapter 5"
    // "full" = complete content built, "shell" = placeholder only
    public string status;
    public string accentColorHex;  // "#B87333"
    public string description;     // short subtitle shown on card
    public int subTopicCount;
}

// ════════════════════════════════════════════════════════════════════
//  LEARN CONTENT  (Resources/Data/Learn/learn_{topicid}.json)
// ════════════════════════════════════════════════════════════════════

[Serializable]
public class LearnData
{
    public string topicID;
    public string heading;       // "What is electromagnetic induction?"
    public string definition;    // paragraph shown in the definition card
    public List<ConceptEntry> concepts;
    public string formula;       // "EMF = -N × (ΔΦ / Δt)"
    public string variableDefs;  // multi-line string, \n for line breaks
    public string source;        // "KLB Physics Book 4, Chapter 5"
}

[Serializable]
public class ConceptEntry
{
    public string title;         // "Faraday's Law"
    public string description;   // one-line description
    public string iconGlyph;     // "F" — letter shown inside the icon circle
}

// ════════════════════════════════════════════════════════════════════
//  FLASHCARD DATA  (Resources/Data/Flashcards/flashcards_{topicid}.json)
// ════════════════════════════════════════════════════════════════════

[Serializable]
public class FlashcardDeckData
{
    public string topicID;
    public List<FlashcardSubTopic> subTopics;
}

[Serializable]
public class FlashcardSubTopic
{
    public string id;        // "faraday"
    public string shortName; // "Faraday" — used in pill label
    public string fullName;  // "4.2 Faraday's Law" — used in nav subtitle
    public List<FlashcardEntry> cards;
}

[Serializable]
public class FlashcardEntry
{
    public string id;        // "em_faraday_001" — unique across all topics
    public string question;
    public string answer;
}

// ════════════════════════════════════════════════════════════════════
//  QUIZ DATA  (Resources/Data/Quiz/quiz_{topicid}.json)
// ════════════════════════════════════════════════════════════════════

[Serializable]
public class QuizData
{
    public string topicID;
    public List<QuizQuestion> questions;
}

[Serializable]
public class QuizQuestion
{
    public string id;                   // "em_q01"
    public string questionText;
    public List<string> options;        // exactly 4 options
    public int correctOptionIndex;      // 0–3
    public string hint;
    public string explanation;          // worked solution shown on feedback screen
    public string reference;            // "KLB Physics Book 4, p.142"
}

// ════════════════════════════════════════════════════════════════════
//  RUNTIME SESSION STATE  (in-memory only, not saved to JSON directly)
// ════════════════════════════════════════════════════════════════════

public class FlashcardSession
{
    public string TopicID;
    public FlashcardDeckData DeckData;

    // Where we are right now
    public int CurrentSubTopicIndex = 0;
    public int CurrentCardIndex = 0;
    public bool IsShowingAnswer = false;

    // Results tracking
    public List<string> KnownCardIDs         = new List<string>();
    public List<string> StillLearningCardIDs = new List<string>();
    public List<string> SkippedCardIDs       = new List<string>();

    // ── Convenience properties ────────────────────────────────────────
    public FlashcardSubTopic CurrentSubTopic =>
        DeckData.subTopics[CurrentSubTopicIndex];

    public FlashcardEntry CurrentCard =>
        CurrentSubTopic.cards[CurrentCardIndex];

    public int TotalCards
    {
        get
        {
            int count = 0;
            foreach (var st in DeckData.subTopics) count += st.cards.Count;
            return count;
        }
    }

    public int CompletedCards =>
        KnownCardIDs.Count + StillLearningCardIDs.Count + SkippedCardIDs.Count;

    // Returns true if there is a next card (in this sub-topic or the next)
    public bool HasNextCard()
    {
        if (CurrentCardIndex < CurrentSubTopic.cards.Count - 1) return true;
        if (CurrentSubTopicIndex < DeckData.subTopics.Count - 1) return true;
        return false;
    }

    // Advance to the next card. Returns false if the deck is complete.
    public bool AdvanceCard()
    {
        IsShowingAnswer = false;
        if (CurrentCardIndex < CurrentSubTopic.cards.Count - 1)
        {
            CurrentCardIndex++;
            return true;
        }
        if (CurrentSubTopicIndex < DeckData.subTopics.Count - 1)
        {
            CurrentSubTopicIndex++;
            CurrentCardIndex = 0;
            return true;
        }
        return false; // deck complete
    }
}

public class QuizSession
{
    public string TopicID;
    public QuizData QuizData;
    public int CurrentQuestionIndex = 0;

    // One entry per question answered. -1 means skipped.
    public List<int> SelectedAnswers = new List<int>();

    // ── Convenience properties ────────────────────────────────────────
    public QuizQuestion CurrentQuestion =>
        QuizData.questions[CurrentQuestionIndex];

    public int TotalQuestions => QuizData.questions.Count;

    public bool IsLastQuestion =>
        CurrentQuestionIndex >= TotalQuestions - 1;

    public int Score
    {
        get
        {
            int score = 0;
            for (int i = 0; i < SelectedAnswers.Count; i++)
            {
                if (i < QuizData.questions.Count &&
                    SelectedAnswers[i] == QuizData.questions[i].correctOptionIndex)
                    score++;
            }
            return score;
        }
    }

    public bool LastAnswerCorrect =>
        SelectedAnswers.Count > 0 &&
        SelectedAnswers[SelectedAnswers.Count - 1] ==
        QuizData.questions[CurrentQuestionIndex].correctOptionIndex;
}

// ════════════════════════════════════════════════════════════════════
//  SESSION LOG  (saved to device storage as JSON)
// ════════════════════════════════════════════════════════════════════

[Serializable]
public class SessionLog
{
    public string sessionID;          // GUID
    public string deviceID;           // SystemInfo.deviceUniqueIdentifier
    public string topicID;
    public string dateTime;           // ISO 8601
    public int durationSeconds;
    public int flashcardsKnown;
    public int flashcardsStillLearning;
    public int flashcardsSkipped;
    public int quizScore;
    public int quizTotal;
}

[Serializable]
public class SessionLogList
{
    public List<SessionLog> sessions = new List<SessionLog>();
}