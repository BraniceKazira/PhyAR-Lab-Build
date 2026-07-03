using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChecklistRowUI : MonoBehaviour
{
    public TMP_Text subTopicLabel;  // SubtopicName TMP
    public Image statusCircle;      // "Status circle" Image

    private readonly Color _doneColor    = new Color(0.180f, 0.769f, 0.710f); // Coil Green
    private readonly Color _pendingColor = new Color(0.10f, 0.10f, 0.18f, 0.15f);

    public void Populate(string name, bool done)
    {
        if (subTopicLabel != null) subTopicLabel.text = name;
        if (statusCircle != null)  statusCircle.color = done ? _doneColor : _pendingColor;
    }
}