// QuizControllers.cs
// Place in: Assets/_Project/Scripts/UI/
// Contains: MCQOptionUI only.
// PerQuestionRowUI removed — question review dropped from Quiz Results screen.

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MCQOptionUI : MonoBehaviour
{
    public TMP_Text letterLabel;
    public TMP_Text optionText;
    public Image    cardBackground;
    public Image    letterCircle;

    private readonly Color _selCard     = new Color(0.722f, 0.451f, 0.200f, 0.10f);
    private readonly Color _unselCard   = Color.white;
    private readonly Color _selCircle   = new Color(0.722f, 0.451f, 0.200f);
    private readonly Color _unselCircle = new Color(0.10f, 0.10f, 0.18f, 0.10f);
    private readonly Color _selLetter   = Color.white;
    private readonly Color _unselLetter = new Color(0.10f, 0.10f, 0.18f, 0.50f);

    private int _index;
    private System.Action<int> _callback;

    public void Populate(int index, string letter, string text, System.Action<int> onSelected)
    {
        _index    = index;
        _callback = onSelected;
        if (letterLabel != null) letterLabel.text = letter;
        if (optionText  != null) optionText.text  = text;
        Deselect();

        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => _callback?.Invoke(_index));
        }
    }

    public void Select()
    {
        if (cardBackground != null) cardBackground.color = _selCard;
        if (letterCircle   != null) letterCircle.color   = _selCircle;
        if (letterLabel    != null) letterLabel.color     = _selLetter;
    }

    public void Deselect()
    {
        if (cardBackground != null) cardBackground.color = _unselCard;
        if (letterCircle   != null) letterCircle.color   = _unselCircle;
        if (letterLabel    != null) letterLabel.color     = _unselLetter;
    }

    public void ShowCorrect()
    {
        if (cardBackground != null) cardBackground.color = new Color(0.180f, 0.769f, 0.710f, 0.15f);
        if (letterCircle   != null) letterCircle.color   = new Color(0.180f, 0.769f, 0.710f);
        if (letterLabel    != null) letterLabel.color     = Color.white;
    }

    public void ShowIncorrect()
    {
        if (cardBackground != null) cardBackground.color = new Color(1f, 0.42f, 0.21f, 0.12f);
        if (letterCircle   != null) letterCircle.color   = new Color(1f, 0.42f, 0.21f);
        if (letterLabel    != null) letterLabel.color     = Color.white;
    }
}