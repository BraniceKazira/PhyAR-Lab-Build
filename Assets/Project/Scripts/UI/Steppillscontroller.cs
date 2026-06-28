using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StepPillsController : MonoBehaviour
{
    [System.Serializable]
    public class PillUI
    {
        public Image background;
        public TMP_Text label;
    }

    public List<PillUI> pills; // 4 entries: Learn, AR, Flashcards, Quiz

    private readonly Color _activeColor   = new Color(0.722f, 0.451f, 0.200f); // Copper
    private readonly Color _doneColor     = new Color(0.180f, 0.769f, 0.710f); // Coil Green
    private readonly Color _inactiveColor = new Color(0.10f,  0.10f,  0.18f, 0.08f);
    private readonly Color _activeLabel   = Color.white;
    private readonly Color _doneLabel     = Color.white;
    private readonly Color _inactiveLabel = new Color(0.10f, 0.10f, 0.18f, 0.50f);

    /// <summary>
    /// activeStep: 0=Learn, 1=AR, 2=Flashcards, 3=Quiz
    /// Steps before activeStep show as done (Coil Green).
    /// Steps after show as inactive (grey).
    /// </summary>
    public void SetActiveStep(int activeStep)
    {
        for (int i = 0; i < pills.Count; i++)
        {
            if (i < activeStep)
            {
                pills[i].background.color = _doneColor;
                pills[i].label.color      = _doneLabel;
            }
            else if (i == activeStep)
            {
                pills[i].background.color = _activeColor;
                pills[i].label.color      = _activeLabel;
            }
            else
            {
                pills[i].background.color = _inactiveColor;
                pills[i].label.color      = _inactiveLabel;
            }
        }
    }
}