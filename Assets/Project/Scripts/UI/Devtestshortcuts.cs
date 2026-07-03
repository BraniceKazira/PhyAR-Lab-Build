// DevTestShortcuts.cs
// TEMPORARY — delete this file before your final build.
// Attach to: AppManagers GameObject
//
// Lets you jump straight to any screen while testing, using number keys.
// Saves you from clicking Splash → Home → Learn → AR → Flashcards every
// single time you press Play.
//
// CONTROLS (press while in Play mode):
//   1 = Home
//   2 = Learn (EM Induction)
//   3 = Flashcards (EM Induction)
//   4 = Flashcard Summary
//   5 = Quiz Question (EM Induction)
//   6 = Quiz Results
//   7 = Progress

using UnityEngine;

public class DevTestShortcuts : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            UINavigator.Instance.ShowScreen(UINavigator.SCREEN_HOME);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            AppState.CurrentTopicID = "em_induction";
            UINavigator.Instance.ShowScreen(UINavigator.SCREEN_LEARN);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            AppState.CurrentTopicID = "em_induction";
            AppState.ActiveFlashcardSession = null; // force fresh session
            UINavigator.Instance.ShowScreen(UINavigator.SCREEN_FLASHCARD);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            UINavigator.Instance.ShowScreen(UINavigator.SCREEN_FC_SUMMARY);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            AppState.CurrentTopicID = "em_induction";
            AppState.ActiveQuizSession = null; // force fresh session
            UINavigator.Instance.ShowScreen(UINavigator.SCREEN_QUIZ_Q);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            UINavigator.Instance.ShowScreen(UINavigator.SCREEN_QUIZ_RESULTS);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            UINavigator.Instance.ShowScreen(UINavigator.SCREEN_PROGRESS);
        }
    }
}