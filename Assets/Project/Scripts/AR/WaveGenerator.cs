// WaveGenerator.cs
// Folder:  Assets/_Project/Scripts/AR/
// Attach:  WavesPrefab root

using UnityEngine;
using TMPro;

public class WaveGenerator : MonoBehaviour
{
    public enum WaveType { Transverse, Longitudinal }

    [Header("Line Renderer — add this component to the same GameObject")]
    public LineRenderer waveLine;
    public int   points      = 80;
    public float displayLen  = 0.4f;

    [Header("Wave properties — changed by sliders")]
    [Range(0.01f, 0.15f)] public float amplitude  = 0.05f;
    [Range(0.5f,  5f)]    public float frequency  = 1f;
    [Range(0.05f, 0.3f)]  public float wavelength = 0.1f;

    [Header("Wave type")]
    public WaveType waveType = WaveType.Transverse;

    [Header("Longitudinal dots — small sphere GameObjects as children")]
    public Transform[] dots;

    [Header("Info labels")]
    public TMP_Text amplitudeLabel;
    public TMP_Text frequencyLabel;
    public TMP_Text wavelengthLabel;
    public TMP_Text speedLabel;
    public TMP_Text typeLabel;

    float _t;

    void Start()
    {
        if (waveLine != null) waveLine.positionCount = points;
        ApplyType();
    }

    void Update()
    {
        _t += Time.deltaTime;
        if (waveType == WaveType.Transverse) DrawTransverse();
        else AnimateLongitudinal();
        UpdateLabels();
    }

    void DrawTransverse()
    {
        if (waveLine == null) return;
        float wn = 2f * Mathf.PI / wavelength;
        float af = 2f * Mathf.PI * frequency;
        for (int i = 0; i < points; i++)
        {
            float x = Mathf.Lerp(-displayLen * 0.5f, displayLen * 0.5f, (float)i / (points - 1));
            float y = amplitude * Mathf.Sin(wn * x - af * _t);
            waveLine.SetPosition(i, new Vector3(x, y, 0));
        }
    }

    void AnimateLongitudinal()
    {
        if (dots == null) return;
        float wn  = 2f * Mathf.PI / wavelength;
        float af  = 2f * Mathf.PI * frequency;
        float gap = displayLen / dots.Length;
        for (int i = 0; i < dots.Length; i++)
        {
            if (dots[i] == null) continue;
            float bx   = (i - dots.Length * 0.5f) * gap;
            float disp = amplitude * Mathf.Sin(wn * bx - af * _t);
            dots[i].localPosition = new Vector3(bx + disp, 0, 0);
        }
    }

    void UpdateLabels()
    {
        float speed = frequency * wavelength;
        if (amplitudeLabel  != null) amplitudeLabel.text  = $"A = {amplitude * 100f:F1} cm";
        if (frequencyLabel  != null) frequencyLabel.text  = $"f = {frequency:F1} Hz";
        if (wavelengthLabel != null) wavelengthLabel.text = $"\u03bb = {wavelength * 100f:F1} cm";
        if (speedLabel      != null) speedLabel.text      = $"v = f\u03bb = {speed:F3} m/s";
        if (typeLabel       != null) typeLabel.text       = waveType + " Wave";
    }

    void ApplyType()
    {
        bool t = waveType == WaveType.Transverse;
        if (waveLine != null) waveLine.gameObject.SetActive(t);
        if (dots != null) foreach (var d in dots) if (d != null) d.gameObject.SetActive(!t);
    }

    // Called by WavesUIController
    public void SetAmplitude(float a)  { amplitude  = a; }
    public void SetFrequency(float f)  { frequency  = f; }
    public void SetWavelength(float w) { wavelength = w; }
    public void ToggleWaveType()
    {
        waveType = waveType == WaveType.Transverse ? WaveType.Longitudinal : WaveType.Transverse;
        ApplyType();
    }
}