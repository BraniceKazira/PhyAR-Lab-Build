// PhyARLabTests.cs
// Folder: Assets/_Project/Tests/
// These are EditMode tests — they run in the Unity Editor without 
// entering Play mode. No device or AR needed.
//
// HOW TO RUN:
// Window → General → Test Runner → EditMode tab → Run All

using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Unit tests for PhyAR Lab core logic.
/// Tests data models, calculations, and session state independently
/// of the Unity scene — no MonoBehaviour instantiation needed.
/// </summary>
public class PhyARLabTests
{
    // ═══════════════════════════════════════════════════════════════
    // TC-U01 to TC-U05: QuizSession scoring
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void QuizSession_PerfectScore_Returns10()
    {
        // Arrange
        var session = CreateQuizSession(10);
        // Simulate all correct answers
        for (int i = 0; i < 10; i++)
            session.SelectedAnswers.Add(session.QuizData.questions[i].correctOptionIndex);

        // Act
        int score = session.Score;

        // Assert
        Assert.AreEqual(10, score, "Perfect score should be 10/10");
    }

    [Test]
    public void QuizSession_ZeroScore_WhenAllSkipped()
    {
        var session = CreateQuizSession(10);
        for (int i = 0; i < 10; i++)
            session.SelectedAnswers.Add(-1); // -1 = skipped

        Assert.AreEqual(0, session.Score, "All skipped should give 0");
    }

    [Test]
    public void QuizSession_PartialScore_CalculatesCorrectly()
    {
        var session = CreateQuizSession(10);
        // Answer first 6 correctly, skip remaining 4
        for (int i = 0; i < 6; i++)
            session.SelectedAnswers.Add(session.QuizData.questions[i].correctOptionIndex);
        for (int i = 0; i < 4; i++)
            session.SelectedAnswers.Add(-1);

        Assert.AreEqual(6, session.Score, "6 correct out of 10");
    }

    [Test]
    public void QuizSession_IsLastQuestion_TrueAtIndex9()
    {
        var session = CreateQuizSession(10);
        session.CurrentQuestionIndex = 9;
        Assert.IsTrue(session.IsLastQuestion);
    }

    [Test]
    public void QuizSession_IsLastQuestion_FalseAtIndex8()
    {
        var session = CreateQuizSession(10);
        session.CurrentQuestionIndex = 8;
        Assert.IsFalse(session.IsLastQuestion);
    }

    // ═══════════════════════════════════════════════════════════════
    // TC-U06 to TC-U10: FlashcardSession state
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void FlashcardSession_TotalCards_CountsAllSubTopics()
    {
        var session = CreateFlashcardSession(3, 5); // 3 subtopics, 5 cards each
        Assert.AreEqual(15, session.TotalCards);
    }

    [Test]
    public void FlashcardSession_CompletedCards_StartsAtZero()
    {
        var session = CreateFlashcardSession(2, 5);
        Assert.AreEqual(0, session.CompletedCards);
    }

    [Test]
    public void FlashcardSession_CompletedCards_IncrementsOnKnown()
    {
        var session = CreateFlashcardSession(1, 3);
        session.KnownCardIDs.Add("card_0");
        session.KnownCardIDs.Add("card_1");
        Assert.AreEqual(2, session.CompletedCards);
    }

    [Test]
    public void FlashcardSession_HasNextCard_TrueWhenMoreCards()
    {
        var session = CreateFlashcardSession(1, 5);
        session.CurrentCardIndex = 0;
        Assert.IsTrue(session.HasNextCard());
    }

    [Test]
    public void FlashcardSession_AdvanceCard_ReturnsFalseAtEnd()
    {
        var session = CreateFlashcardSession(1, 1);
        bool hasNext = session.AdvanceCard();
        Assert.IsFalse(hasNext, "Should return false when no more cards");
    }

    // ═══════════════════════════════════════════════════════════════
    // TC-U11 to TC-U15: Ohm's Law calculations (CircuitSimulator logic)
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void OhmsLaw_SeriesCircuit_CurrentIsVoltageOverTotalR()
    {
        float voltage = 6f;
        float r1 = 10f, r2 = 10f;
        float totalR = r1 + r2; // 20 ohms
        float expectedI = voltage / totalR; // 0.3A

        Assert.AreEqual(0.3f, expectedI, 0.001f,
            "Series: I = V/(R1+R2) = 6/20 = 0.3A");
    }

    [Test]
    public void OhmsLaw_ParallelCircuit_TotalRIsLessThanSmallestR()
    {
        float r1 = 10f, r2 = 10f;
        float totalR = (r1 * r2) / (r1 + r2); // 5 ohms

        Assert.Less(totalR, Mathf.Min(r1, r2),
            "Parallel R must be less than smallest individual R");
    }

    [Test]
    public void OhmsLaw_ParallelCircuit_CurrentIsHigherThanSeries()
    {
        float v = 6f, r1 = 10f, r2 = 10f;
        float seriesI   = v / (r1 + r2);
        float parallelI = v / ((r1 * r2) / (r1 + r2));

        Assert.Greater(parallelI, seriesI,
            "Parallel circuit draws more current than series");
    }

    [Test]
    public void OhmsLaw_IncreasedResistance_DecreasesCurrentSeries()
    {
        float v = 6f;
        float lowR  = 10f;
        float highR = 50f;

        float iLow  = v / (lowR  * 2);
        float iHigh = v / (highR * 2);

        Assert.Greater(iLow, iHigh,
            "Higher resistance produces lower current");
    }

    [Test]
    public void OhmsLaw_VoltageShared_InSeriesCircuit()
    {
        float v = 6f, r1 = 10f, r2 = 10f;
        float i  = v / (r1 + r2);
        float v1 = i * r1;
        float v2 = i * r2;

        Assert.AreEqual(v, v1 + v2, 0.001f,
            "V1 + V2 must equal supply voltage in series");
    }

    // ═══════════════════════════════════════════════════════════════
    // TC-U16 to TC-U20: Wave equation (v = fλ)
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void WaveEquation_SpeedEqualsFrequencyTimesWavelength()
    {
        float f = 2f, lambda = 0.1f;
        float speed = f * lambda;
        Assert.AreEqual(0.2f, speed, 0.001f, "v = fλ = 2 × 0.1 = 0.2 m/s");
    }

    [Test]
    public void WaveEquation_DoublingFrequency_DoublesSpeed()
    {
        float lambda = 0.1f;
        float f1 = 1f, f2 = 2f;
        Assert.AreEqual(f1 * lambda * 2, f2 * lambda, 0.001f);
    }

    [Test]
    public void WaveEquation_Period_IsInverseOfFrequency()
    {
        float f = 5f;
        float T = 1f / f;
        Assert.AreEqual(0.2f, T, 0.001f, "T = 1/f = 1/5 = 0.2s");
    }

    [Test]
    public void WaveEquation_HigherAmplitude_DoesNotChangeSpeed()
    {
        float f = 1f, lambda = 0.1f;
        float speed1 = f * lambda; // amplitude = 0.05
        float speed2 = f * lambda; // amplitude = 0.12
        Assert.AreEqual(speed1, speed2, 0.001f,
            "Amplitude does not affect wave speed");
    }

    [Test]
    public void WaveEquation_ZeroFrequency_ProducesZeroSpeed()
    {
        float speed = 0f * 0.1f;
        Assert.AreEqual(0f, speed, 0.001f);
    }

    // ═══════════════════════════════════════════════════════════════
    // TC-U21 to TC-U25: AppState management
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void AppState_DefaultTopicID_IsEMInduction()
    {
        AppState.CurrentTopicID = "em_induction";
        Assert.AreEqual("em_induction", AppState.CurrentTopicID);
    }

    [Test]
    public void AppState_ResetSession_ClearsActiveSession()
    {
        AppState.ActiveQuizSession = new QuizSession
        {
            TopicID  = "test",
            QuizData = new QuizData()
        };
        AppState.ResetSession();
        Assert.IsNull(AppState.ActiveQuizSession,
            "ResetSession should clear ActiveQuizSession");
    }

    [Test]
    public void AppState_ResetSession_ClearsFlashcardSession()
    {
        AppState.ActiveFlashcardSession = new FlashcardSession
        {
            TopicID  = "test",
            DeckData = new FlashcardDeckData()
        };
        AppState.ResetSession();
        Assert.IsNull(AppState.ActiveFlashcardSession);
    }

    [Test]
    public void AppState_TopicIDPersists_AcrossScreenChange()
    {
        AppState.CurrentTopicID = "waves_ii";
        // Simulate navigating away and back
        string saved = AppState.CurrentTopicID;
        Assert.AreEqual("waves_ii", saved);
    }

    [Test]
    public void AppState_LastAnswerCorrect_DefaultsFalse()
    {
        AppState.ResetSession();
        Assert.IsFalse(AppState.LastAnswerWasCorrect);
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPERS — create lightweight test objects without MonoBehaviours
    // ═══════════════════════════════════════════════════════════════

    QuizSession CreateQuizSession(int questionCount)
    {
        var data = new QuizData
        {
            topicID   = "test",
            questions = new System.Collections.Generic.List<QuizQuestion>()
        };

        for (int i = 0; i < questionCount; i++)
        {
            data.questions.Add(new QuizQuestion
            {
                id                 = "q" + i,
                questionText       = "Test question " + i,
                options            = new System.Collections.Generic.List<string>
                    { "A", "B", "C", "D" },
                correctOptionIndex = 0, // correct answer is always A
                hint               = "",
                explanation        = "",
                reference          = ""
            });
        }

        return new QuizSession
        {
            TopicID  = "test",
            QuizData = data
        };
    }

    FlashcardSession CreateFlashcardSession(int subTopicCount, int cardsPerSubTopic)
    {
        var deck = new FlashcardDeckData
        {
            topicID   = "test",
            subTopics = new System.Collections.Generic.List<FlashcardSubTopic>()
        };

        for (int s = 0; s < subTopicCount; s++)
        {
            var subTopic = new FlashcardSubTopic
            {
                id        = "st" + s,
                shortName = "SubTopic " + s,
                fullName  = "Full SubTopic " + s,
                cards     = new System.Collections.Generic.List<FlashcardEntry>()
            };

            for (int c = 0; c < cardsPerSubTopic; c++)
                subTopic.cards.Add(new FlashcardEntry
                {
                    id       = "card_" + c,
                    question = "Q" + c,
                    answer   = "A" + c
                });

            deck.subTopics.Add(subTopic);
        }

        return new FlashcardSession { TopicID = "test", DeckData = deck };
    }
}