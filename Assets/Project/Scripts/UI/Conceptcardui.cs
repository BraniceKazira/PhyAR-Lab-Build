using UnityEngine;
using TMPro;

public class ConceptCardUI : MonoBehaviour
{
    public TMP_Text iconGlyphText;
    public TMP_Text titleText;
    public TMP_Text descriptionText;

    public void Populate(string title, string description, string glyph)
    {
        if (titleText)       titleText.text       = title;
        if (descriptionText) descriptionText.text = description;
        if (iconGlyphText)   iconGlyphText.text   = glyph;
    }
}