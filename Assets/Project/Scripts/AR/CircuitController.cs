// CircuitUIController.cs
// Folder:  Assets/_Project/Scripts/UI/
// Attach:  CircuitControlPanel inside Screen_ARExperience

using UnityEngine;
using TMPro;

public class CircuitUIController : MonoBehaviour
{
    public TMP_Text voltageLabel;
    public TMP_Text r1Label;
    public TMP_Text r2Label;
    public TMP_Text modeLabel;

    CircuitSimulator _sim;
    CircuitSimulator Sim()
    {
        if (_sim == null) _sim = FindObjectOfType<CircuitSimulator>();
        return _sim;
    }

    void OnEnable() { _sim = FindObjectOfType<CircuitSimulator>(); }

    public void OnVoltageChanged(float v)
    {
        Sim()?.SetVoltage(v);
        if (voltageLabel != null) voltageLabel.text = $"{v:F1} V";
    }

    public void OnR1Changed(float r)
    {
        Sim()?.SetResistance1(r);
        if (r1Label != null) r1Label.text = $"{r:F0} \u03a9";
    }

    public void OnR2Changed(float r)
    {
        Sim()?.SetResistance2(r);
        if (r2Label != null) r2Label.text = $"{r:F0} \u03a9";
    }

    public void OnToggleMode()
    {
        Sim()?.ToggleMode();
        if (modeLabel != null && Sim() != null)
            modeLabel.text = Sim().mode == CircuitSimulator.CircuitMode.Series
                ? "Series" : "Parallel";
    }
}