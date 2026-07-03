// ModelInteractionHandler.cs
// Folder:  Assets/_Project/Scripts/AR/
// Attach:  Root of EVERY topic prefab (circuit, EM, waves)
//
// Single finger = rotate   |   Two fingers = scale

using UnityEngine;

public class ModelInteractionHandler : MonoBehaviour
{
    public float rotSpeed = 0.25f;
    public float minScale = 0.01f;
    public float maxScale = 0.8f;

    float   _startDist;
    Vector3 _startScale;
    bool    _pinching;

    void Update()
    {
        if (Input.touchCount == 1)       Rotate();
        else if (Input.touchCount == 2)  Scale();
        else                             _pinching = false;
    }

    void Rotate()
    {
        var t = Input.GetTouch(0);
        if (t.phase == TouchPhase.Moved)
            transform.Rotate(Vector3.up, -t.deltaPosition.x * rotSpeed, Space.World);
    }

    void Scale()
    {
        var t0 = Input.GetTouch(0);
        var t1 = Input.GetTouch(1);

        if (t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
        {
            _startDist  = Vector2.Distance(t0.position, t1.position);
            _startScale = transform.localScale;
            _pinching   = true;
        }
        else if (_pinching && (t0.phase == TouchPhase.Moved || t1.phase == TouchPhase.Moved))
        {
            float d = Vector2.Distance(t0.position, t1.position);
            if (Mathf.Approximately(_startDist, 0)) return;
            float f  = d / _startDist;
            var   ns = Vector3.one * Mathf.Clamp((_startScale * f).x, minScale, maxScale);
            transform.localScale = ns;
        }
    }

    // Called by ARExperienceScreenController.OnLabelsClicked
    public void OnToggleLabels(bool show)
    {
        foreach (var a in GetComponentsInChildren<AnnotationLabel>(true))
            a.gameObject.SetActive(show);
    }
}