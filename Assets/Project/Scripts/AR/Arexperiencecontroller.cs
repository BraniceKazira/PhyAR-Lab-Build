// ARExperienceScreenController.cs
// Folder:  Assets/_Project/Scripts/UI/
// Attach:  Screen_ARExperience panel in Main.unity

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ARExperienceScreenController : MonoBehaviour
{
    [Header("Nav")]
    public TMP_Text navTitleText;

    [Header("Toolbar buttons")]
    public Button backButton;
    public Button animateButton;
    public Button labelsButton;
    public Button resetButton;

    [Header("Topic control panels")]
    public GameObject circuitControlPanel;
    public GameObject emControlPanel;
    public GameObject wavesControlPanel;

    [Header("Drag ARSessionManager from Hierarchy")]
    public ARSessionManager arSessionManager;

    private bool _labelsVisible = true;

    void OnEnable()
    {
        if (navTitleText != null) navTitleText.text = GetTopicName();
        HideAllPanels();
    }

    public void OnARStarted(string topicID)
    {
        if (navTitleText != null) navTitleText.text = GetTopicName();
        HideAllPanels();
    }

    public void OnModelPlaced(string topicID)
    {
        HideAllPanels();
        switch (topicID)
        {
            case "current_electricity":
                if (circuitControlPanel != null) circuitControlPanel.SetActive(true); break;
            case "em_induction":
                if (emControlPanel != null)      emControlPanel.SetActive(true);      break;
            case "waves_ii":
                if (wavesControlPanel != null)   wavesControlPanel.SetActive(true);   break;
        }
    }

    public void OnModelRemoved() => HideAllPanels();

    // Wire Back button OnClick to this
    public void OnBackClicked()
    {
        HideAllPanels();
        if (arSessionManager != null) arSessionManager.StopAR();
        else UINavigator.Instance.ShowScreen(UINavigator.SCREEN_LEARN);
    }

    // Wire Animate button OnClick to this
    public void OnAnimateClicked()
    {
        switch (AppState.CurrentTopicID)
        {
            case "em_induction":
                FindObjectOfType<EMInductionController>()?.OnMagnetInClicked();
                break;
            case "waves_ii":
                var w = FindObjectOfType<WaveGenerator>();
                if (w != null) w.enabled = !w.enabled;
                break;
            case "current_electricity":
                FindObjectOfType<CircuitSimulator>()?.ToggleMode();
                break;
        }
    }

    // Wire Labels button OnClick to this
    public void OnLabelsClicked()
    {
        _labelsVisible = !_labelsVisible;
        FindObjectOfType<ModelInteractionHandler>()?.OnToggleLabels(_labelsVisible);
    }

    // Wire Reset button OnClick to this
    public void OnResetClicked()
    {
        HideAllPanels();
        if (arSessionManager != null) arSessionManager.ResetPlacement();
    }

    void HideAllPanels()
    {
        if (circuitControlPanel != null) circuitControlPanel.SetActive(false);
        if (emControlPanel      != null) emControlPanel.SetActive(false);
        if (wavesControlPanel   != null) wavesControlPanel.SetActive(false);
    }

    string GetTopicName() =>
        AppState.CurrentTopicID switch
        {
            "current_electricity" => "Current Electricity II",
            "em_induction"        => "Electromagnetic Induction",
            "waves_ii"            => "Waves II",
            _                     => "AR Experience"
        };
}