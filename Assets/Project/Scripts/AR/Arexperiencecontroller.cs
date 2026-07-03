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

    [Header("Drag ARSessionManager from Hierarchy")]
    public ARSessionManager arSessionManager;

    [Header("InteractionCaption — repurposed as value display")]
    public TMP_Text captionText;

    [Header("HelpIcon — repurposed as value label on nav bar")]
    public TMP_Text valueLabelText;

    private bool _labelsVisible = true;

    // ── Preset values ─────────────────────────────────────────────────
    private float[] _resistancePresets = { 5f, 10f, 20f, 50f, 100f };
    private int     _resistanceIndex   = 1;

    private float[] _amplitudePresets  = { 0.02f, 0.05f, 0.08f, 0.12f };
    private int     _amplitudeIndex    = 1;

    // EM Induction cycles through 3 states
    private int _emState = 0; // 0=idle, 1=magnetIn, 2=magnetOut

    void OnEnable()
    {
        if (navTitleText != null) navTitleText.text = GetTopicName();
        SetCaption("Point phone at a flat surface to begin");
    }

    public void OnARStarted(string topicID)
    {
        if (navTitleText != null) navTitleText.text = GetTopicName();
        _emState = 0;
        _resistanceIndex = 1;
        _amplitudeIndex  = 1;
        SetCaption("Move phone slowly over a flat surface");
        UpdateValueLabel();
    }

    public void OnModelPlaced(string topicID)
    {
        SetCaption("Pinch to scale  ·  Drag to rotate");
        UpdateValueLabel();
    }

    public void OnModelRemoved()
    {
        SetCaption("Move phone slowly over a flat surface");
        if (valueLabelText != null) valueLabelText.text = "";
    }

    // ── Tool_Rotate → Increment (+) ──────────────────────────────────
    public void OnIncrementClicked()
    {
        switch (AppState.CurrentTopicID)
        {
            case "current_electricity":
                _resistanceIndex = Mathf.Min(_resistanceIndex + 1, _resistancePresets.Length - 1);
                ApplyResistance();
                break;
            case "waves_ii":
                _amplitudeIndex = Mathf.Min(_amplitudeIndex + 1, _amplitudePresets.Length - 1);
                ApplyAmplitude();
                break;
        }
        UpdateValueLabel();
    }

    // ── Tool_Scale → Decrement (−) ────────────────────────────────────
    public void OnDecrementClicked()
    {
        switch (AppState.CurrentTopicID)
        {
            case "current_electricity":
                _resistanceIndex = Mathf.Max(_resistanceIndex - 1, 0);
                ApplyResistance();
                break;
            case "waves_ii":
                _amplitudeIndex = Mathf.Max(_amplitudeIndex - 1, 0);
                ApplyAmplitude();
                break;
        }
        UpdateValueLabel();
    }

    // ── Tool_Animate → Main physics action ───────────────────────────
    public void OnAnimateClicked()
    {
        switch (AppState.CurrentTopicID)
        {
            case "current_electricity":
                FindObjectOfType<CircuitSimulator>()?.ToggleMode();
                CircuitSimulator sim = FindObjectOfType<CircuitSimulator>();
                if (sim != null)
                    SetCaption(sim.mode == CircuitSimulator.CircuitMode.Series
                        ? "Series circuit — same current through all" 
                        : "Parallel circuit — same voltage across all");
                break;

            case "em_induction":
                EMInductionController em = FindObjectOfType<EMInductionController>();
                if (em != null)
                {
                    _emState = (_emState + 1) % 3;
                    if (_emState == 1)
                    {
                        em.OnMagnetInClicked();
                        SetCaption("Flux increasing → EMF induced (Faraday)");
                    }
                    else if (_emState == 2)
                    {
                        em.OnMagnetOutClicked();
                        SetCaption("Flux decreasing → EMF reverses (Lenz)");
                    }
                    else
                    {
                        em.OnResetClicked();
                        SetCaption("Magnet stationary → no EMF (Faraday)");
                    }
                }
                break;

            case "waves_ii":
                WaveGenerator wave = FindObjectOfType<WaveGenerator>();
                if (wave != null)
                {
                    wave.enabled = !wave.enabled;
                    SetCaption(wave.enabled ? "Wave propagating →" : "Wave paused");
                }
                break;
        }
    }

    // ── Tool_Labels → show/hide floating label ────────────────────────
    public void OnLabelsClicked()
    {
        _labelsVisible = !_labelsVisible;
        FindObjectOfType<ModelInteractionHandler>()?.OnToggleLabels(_labelsVisible);
        SetCaption(_labelsVisible ? "Labels shown" : "Labels hidden");
    }

    // ── Tool_Summary → reset placement ───────────────────────────────
    public void OnResetClicked()
    {
        _emState = 0;
        if (arSessionManager != null) arSessionManager.ResetPlacement();
    }

    // ── Back button ───────────────────────────────────────────────────
    public void OnBackClicked()
    {
        if (arSessionManager != null) arSessionManager.StopAR();
        else UINavigator.Instance.ShowScreen(UINavigator.SCREEN_LEARN);
    }

    // ── Apply values to AR scripts ────────────────────────────────────
    void ApplyResistance()
    {
        float r = _resistancePresets[_resistanceIndex];
        CircuitSimulator sim = FindObjectOfType<CircuitSimulator>();
        if (sim != null) { sim.SetResistance1(r); sim.SetResistance2(r); }
    }

    void ApplyAmplitude()
    {
        FindObjectOfType<WaveGenerator>()?.SetAmplitude(_amplitudePresets[_amplitudeIndex]);
    }

    void UpdateValueLabel()
    {
        if (valueLabelText == null) return;

        switch (AppState.CurrentTopicID)
        {
            case "current_electricity":
                float r = _resistancePresets[_resistanceIndex];
                float i = 6f / (r * 2f);
                valueLabelText.text = $"R={r:F0}\u03a9  I={i:F3}A";
                break;
            case "waves_ii":
                valueLabelText.text = $"A={_amplitudePresets[_amplitudeIndex]*100f:F0}cm";
                break;
            case "em_induction":
                valueLabelText.text = "↓ Animate to move magnet";
                break;
            default:
                valueLabelText.text = "";
                break;
        }
    }

    void SetCaption(string text)
    {
        if (captionText != null) captionText.text = text;
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