// TopicSelector.cs
// Attach to: Screen_Home panel
//
// The simplest and most reliable way to select a topic.
// Wire each card's button DIRECTLY to these methods in the Inspector.
// No topicID guesswork, no Start() timing issues, no indirect calls.
//
// HOW TO WIRE:
//   Card_Waves button         → On Click() → TopicSelector → SelectWavesII()
//   Card_Electromagnetism button → On Click() → TopicSelector → SelectEMInduction()
//   Card_CurrentElectricity button → On Click() → TopicSelector → SelectCurrentElectricity()

using UnityEngine;

public class TopicSelector : MonoBehaviour
{
    public void SelectEMInduction()
    {
        Debug.Log("[TopicSelector] Selected: em_induction");
        AppState.CurrentTopicID = "em_induction";
        UINavigator.Instance.ShowScreen(UINavigator.SCREEN_LEARN);
    }

    public void SelectCurrentElectricity()
    {
        Debug.Log("[TopicSelector] Selected: current_electricity");
        AppState.CurrentTopicID = "current_electricity";
        UINavigator.Instance.ShowScreen(UINavigator.SCREEN_LEARN);
    }

    public void SelectWavesII()
    {
        Debug.Log("[TopicSelector] Selected: waves_ii");
        AppState.CurrentTopicID = "waves_ii";
        UINavigator.Instance.ShowScreen(UINavigator.SCREEN_LEARN);
    }
}