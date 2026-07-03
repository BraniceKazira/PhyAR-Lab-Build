// WavesUIController.cs
// Folder:  Assets/_Project/Scripts/UI/
// Attach:  WavesControlPanel inside Screen_ARExperience

using UnityEngine;
using TMPro;

public class WavesUIController : MonoBehaviour
{
    public TMP_Text amplitudeLabel;
    public TMP_Text frequencyLabel;
    public TMP_Text wavelengthLabel;

    WaveGenerator _wave;
    WaveGenerator Wave()
    {
        if (_wave == null) _wave = FindObjectOfType<WaveGenerator>();
        return _wave;
    }

    void OnEnable() { _wave = FindObjectOfType<WaveGenerator>(); }

    public void OnAmplitudeChanged(float a)
    {
        Wave()?.SetAmplitude(a);
        if (amplitudeLabel  != null) amplitudeLabel.text  = $"{a * 100f:F1} cm";
    }

    public void OnFrequencyChanged(float f)
    {
        Wave()?.SetFrequency(f);
        if (frequencyLabel  != null) frequencyLabel.text  = $"{f:F1} Hz";
    }

    public void OnWavelengthChanged(float w)
    {
        Wave()?.SetWavelength(w);
        if (wavelengthLabel != null) wavelengthLabel.text = $"{w * 100f:F1} cm";
    }

    public void OnToggleWaveType() => Wave()?.ToggleWaveType();
}