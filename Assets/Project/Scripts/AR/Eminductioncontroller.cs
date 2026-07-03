// EMInductionController.cs
// Folder:  Assets/_Project/Scripts/AR/
// Attach:  EMInductionPrefab root

using System.Collections;
using UnityEngine;
using TMPro;

public class EMInductionController : MonoBehaviour
{
    [Header("Scene objects")]
    public Transform magnet;
    public Transform galvanometerNeedle;
    public ParticleSystem fieldLines;

    [Header("Positions (local space)")]
    public Vector3 idlePos   = new Vector3(0,  0.15f, 0);
    public Vector3 insidePos = new Vector3(0,  0.01f, 0);
    public float   speed     = 0.06f;

    [Header("Galvanometer")]
    public float maxAngle = 50f;

    [Header("Labels")]
    public TMP_Text stateLabel;
    public TMP_Text emfLabel;

    [Header("Coil properties")]
    public int   turns    = 200;
    public float coilArea = 0.004f;

    Coroutine _move;

    void Start()
    {
        if (magnet != null) magnet.localPosition = idlePos;
        SetIdle();
    }

    // Wire to "Magnet In" button via EMUIController
    public void OnMagnetInClicked()
    {
        StopMove();
        _move = StartCoroutine(Animate(insidePos, approaching: true));
    }

    // Wire to "Magnet Out" button via EMUIController
    public void OnMagnetOutClicked()
    {
        StopMove();
        _move = StartCoroutine(Animate(idlePos, approaching: false));
    }

    // Wire to "Reset" button via EMUIController
    public void OnResetClicked()
    {
        StopMove();
        if (magnet != null) magnet.localPosition = idlePos;
        Deflect(0);
        SetIdle();
        if (fieldLines != null) fieldLines.Stop();
    }

    IEnumerator Animate(Vector3 target, bool approaching)
    {
        if (fieldLines != null && !fieldLines.isPlaying) fieldLines.Play();

        if (stateLabel != null)
            stateLabel.text = approaching
                ? "Magnet approaching\nFlux increasing\nInduced EMF opposes entry\n(Lenz's Law)"
                : "Magnet withdrawing\nFlux decreasing\nInduced EMF reverses\n(Lenz's Law)";

        while (magnet != null &&
               Vector3.Distance(magnet.localPosition, target) > 0.002f)
        {
            magnet.localPosition = Vector3.MoveTowards(
                magnet.localPosition, target, speed * Time.deltaTime);

            float dist   = Mathf.Abs(magnet.localPosition.y);
            float prox   = Mathf.Clamp01(1f - dist / 0.15f);
            float emf    = turns * coilArea * prox * 3f;
            Deflect(approaching ? prox : -prox);

            if (emfLabel != null) emfLabel.text = $"EMF \u2248 {emf:F2} V";
            yield return null;
        }

        if (fieldLines != null) fieldLines.Stop();
        SetIdle();
    }

    void StopMove()
    {
        if (_move != null) StopCoroutine(_move);
    }

    void Deflect(float d)
    {
        if (galvanometerNeedle == null) return;
        galvanometerNeedle.localRotation = Quaternion.Lerp(
            galvanometerNeedle.localRotation,
            Quaternion.Euler(0, 0, d * maxAngle),
            Time.deltaTime * 8f);
    }

    void SetIdle()
    {
        if (stateLabel != null)
            stateLabel.text = "Magnet stationary\nNo flux change\nEMF = 0\n(Faraday's Law)";
        if (emfLabel != null) emfLabel.text = "EMF = 0 V";
        Deflect(0);
    }
}