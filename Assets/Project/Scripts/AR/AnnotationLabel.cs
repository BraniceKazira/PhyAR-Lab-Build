// AnnotationLabel.cs
// Folder:  Assets/_Project/Scripts/AR/
// Attach:  Each World Space Canvas that floats above a component
//
// Makes the label always face the AR camera (billboard).

using UnityEngine;
using TMPro;

public class AnnotationLabel : MonoBehaviour
{
    public string defaultText;
    Camera _cam;

    void Start()
    {
        _cam = Camera.main;
        if (!string.IsNullOrEmpty(defaultText))
        {
            var tmp = GetComponentInChildren<TMP_Text>();
            if (tmp != null) tmp.text = defaultText;
        }
    }

    void LateUpdate()
    {
        if (_cam == null) _cam = Camera.main;
        if (_cam == null) return;
        transform.LookAt(
            transform.position + _cam.transform.rotation * Vector3.forward,
            _cam.transform.rotation * Vector3.up);
    }
}