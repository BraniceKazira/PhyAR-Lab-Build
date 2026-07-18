// WaveLabels.cs — Priority 2.3
// Attach to: WavesPrefab root
// Adds floating labels anchored to specific points on the wave.

using UnityEngine;
using TMPro;

public class WaveUIcontroller: MonoBehaviour
{
    [Header("Drag the WaveGenerator from the same GameObject")]
    public WaveGenerator waveGenerator;

    [Header("Label positions (set in Inspector or auto-calculated)")]
    public Transform amplitudeLabel;   // floats at wave crest
    public Transform troughLabel;      // floats at wave trough
    public Transform wavelengthLabel;  // floats between two crests
    public Transform crestLabel;       // marks the highest point

    [Header("Label TMP texts")]
    public TMP_Text amplitudeText;
    public TMP_Text troughText;
    public TMP_Text wavelengthText;
    public TMP_Text crestText;

    private LineRenderer _line;
    private bool         _labelsVisible = true;

    void Start()
    {
        _line = GetComponent<LineRenderer>();
        if (_line == null && waveGenerator != null)
            _line = waveGenerator.GetComponent<LineRenderer>();
    }

    void LateUpdate()
    {
        if (!_labelsVisible || _line == null || _line.positionCount < 2) return;

        // Find crest (highest Y) and trough (lowest Y) positions on the wave
        Vector3 crestPos  = _line.GetPosition(0);
        Vector3 troughPos = _line.GetPosition(0);

        for (int i = 1; i < _line.positionCount; i++)
        {
            Vector3 p = _line.GetPosition(i);
            if (p.y > crestPos.y)  crestPos  = p;
            if (p.y < troughPos.y) troughPos = p;
        }

        // Convert from local to world space
        crestPos  = transform.TransformPoint(crestPos);
        troughPos = transform.TransformPoint(troughPos);

        // Position labels
        if (amplitudeLabel != null)
        {
            amplitudeLabel.position = crestPos + Vector3.up * 0.02f;
            if (amplitudeText != null && waveGenerator != null)
                amplitudeText.text = $"Amplitude\n{waveGenerator.amplitude * 100f:F1} cm";
        }

        if (crestLabel != null)
        {
            crestLabel.position = crestPos;
            if (crestText != null) crestText.text = "Crest";
        }

        if (troughLabel != null)
        {
            troughLabel.position = troughPos;
            if (troughText != null) troughText.text = "Trough";
        }

        if (wavelengthLabel != null && waveGenerator != null)
        {
            // Place wavelength label at mid-point of wave
            Vector3 mid = transform.TransformPoint(_line.GetPosition(_line.positionCount / 2));
            wavelengthLabel.position = mid + Vector3.up * 0.04f;
            if (wavelengthText != null)
                wavelengthText.text = $"\u03bb = {waveGenerator.wavelength * 100f:F1} cm";
        }
    }

    public void ToggleLabels(bool show)
    {
        _labelsVisible = show;
        if (amplitudeLabel  != null) amplitudeLabel.gameObject.SetActive(show);
        if (troughLabel     != null) troughLabel.gameObject.SetActive(show);
        if (wavelengthLabel != null) wavelengthLabel.gameObject.SetActive(show);
        if (crestLabel      != null) crestLabel.gameObject.SetActive(show);
    }
}