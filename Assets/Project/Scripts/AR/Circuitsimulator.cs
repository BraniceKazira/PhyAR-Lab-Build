// CircuitSimulator.cs — Updated with Voltage display (Priority 1.5)
// Attach to: CircuitPrefab root

using UnityEngine;
using TMPro;

public class CircuitSimulator : MonoBehaviour
{
    public enum CircuitMode { Series, Parallel }

    [Header("Circuit values")]
    public CircuitMode mode        = CircuitMode.Series;
    [Range(1f,  12f)]  public float voltage     = 6f;
    [Range(1f, 100f)]  public float resistance1 = 10f;
    [Range(1f, 100f)]  public float resistance2 = 10f;

    [Header("Layout GameObjects")]
    public GameObject seriesLayout;
    public GameObject parallelLayout;

    [Header("Annotation labels")]
    public TMP_Text batteryLabel;
    public TMP_Text resistor1Label;
    public TMP_Text resistor2Label;
    public TMP_Text amperemeterLabel;
    public TMP_Text voltmeterLabel;
    public TMP_Text formulaLabel;

    [Header("Visuals (optional)")]
    public ParticleSystem currentParticles;
    public Light          bulb1Light;
    public Light          bulb2Light;
    [Range(0.5f, 5f)] public float maxBulbIntensity = 2f;

    float _totalR, _totalI, _i1, _i2, _v1, _v2;

    void Update()
    {
        Calculate();
        UpdateLabels();
        UpdateVisuals();
    }

    void Calculate()
    {
        if (mode == CircuitMode.Series)
        {
            _totalR = resistance1 + resistance2;
            _totalI = voltage / _totalR;
            _i1 = _totalI; _i2 = _totalI;
            _v1 = _i1 * resistance1;
            _v2 = _i2 * resistance2;
        }
        else
        {
            _totalR = (resistance1 * resistance2) / (resistance1 + resistance2);
            _totalI = voltage / _totalR;
            _i1 = voltage / resistance1;
            _i2 = voltage / resistance2;
            _v1 = voltage; _v2 = voltage;
        }
    }

    void UpdateLabels()
    {
        // FIX 1.5: All three V, I, R values shown on battery label
        if (batteryLabel != null)
            batteryLabel.text = $"Battery\nV = {voltage:F1} V\nI = {_totalI:F3} A\nR = {_totalR:F1} \u03a9";

        if (resistor1Label != null)
            resistor1Label.text =
                $"R\u2081 = {resistance1:F0}\u03a9\nV = {_v1:F2}V\nI = {_i1:F3}A";

        if (resistor2Label != null)
            resistor2Label.text =
                $"R\u2082 = {resistance2:F0}\u03a9\nV = {_v2:F2}V\nI = {_i2:F3}A";

        if (amperemeterLabel != null)
            amperemeterLabel.text = $"A\n{_totalI:F3} A";

        // FIX 1.5: Voltmeter now shows actual voltage
        if (voltmeterLabel != null)
            voltmeterLabel.text = $"V\n{voltage:F1} V";

        if (formulaLabel != null)
        {
            string modeStr = mode == CircuitMode.Series
                ? "Series" : "Parallel";
            // FIX 1.5: All three values in formula label
            formulaLabel.text =
                $"{modeStr}: V={voltage:F1}V  R={_totalR:F1}\u03a9  I={_totalI:F3}A";
        }
    }

    void UpdateVisuals()
    {
        if (currentParticles != null)
        {
            var m = currentParticles.main;
            m.simulationSpeed = Mathf.Lerp(0.2f, 4f, _totalI / 2f);
            if (_totalI > 0.01f && !currentParticles.isPlaying) currentParticles.Play();
            else if (_totalI <= 0.01f && currentParticles.isPlaying) currentParticles.Pause();
        }

        float b = Mathf.Clamp01(_totalI / 2f);
        if (bulb1Light != null) bulb1Light.intensity = b * maxBulbIntensity;
        if (bulb2Light != null)
            bulb2Light.intensity = (mode == CircuitMode.Series
                ? b : Mathf.Clamp01(_i2 / 2f)) * maxBulbIntensity;
    }

    public void SetVoltage(float v)     { voltage     = v; }
    public void SetResistance1(float r) { resistance1 = r; }
    public void SetResistance2(float r) { resistance2 = r; }

    public void ToggleMode()
    {
        mode = mode == CircuitMode.Series
            ? CircuitMode.Parallel : CircuitMode.Series;
        if (seriesLayout   != null) seriesLayout.SetActive(mode   == CircuitMode.Series);
        if (parallelLayout != null) parallelLayout.SetActive(mode == CircuitMode.Parallel);
    }

    // Public getters for ARExperienceScreenController caption
    public float GetVoltage()   => voltage;
    public float GetTotalR()    => _totalR;
    public float GetTotalI()    => _totalI;
}