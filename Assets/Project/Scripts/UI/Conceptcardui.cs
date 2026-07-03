using UnityEngine;
using TMPro;

public class ConceptCardUI : MonoBehaviour
{
    [Tooltip("The bold title TMP — e.g. 'Faraday's Law'")]
    public TMP_Text titleText;

    [Tooltip("The description TMP below the title")]
    public TMP_Text descriptionText;

    [Tooltip("Optional — the letter glyph inside an icon circle. Leave empty if unused.")]
    public TMP_Text iconGlyphText;

    public void Populate(string title, string description, string glyph)
    {
        if (titleText != null)       titleText.text       = title;
        if (descriptionText != null) descriptionText.text = description;
        if (iconGlyphText != null)   iconGlyphText.text   = glyph;
    }
}