// EMUIController.cs
// Folder:  Assets/_Project/Scripts/UI/
// Attach:  EMControlPanel inside Screen_ARExperience

using UnityEngine;

public class EMUIController : MonoBehaviour
{
    EMInductionController _em;
    EMInductionController Em()
    {
        if (_em == null) _em = FindObjectOfType<EMInductionController>();
        return _em;
    }

    void OnEnable() { _em = FindObjectOfType<EMInductionController>(); }

    public void OnMagnetIn()  => Em()?.OnMagnetInClicked();
    public void OnMagnetOut() => Em()?.OnMagnetOutClicked();
    public void OnReset()     => Em()?.OnResetClicked();
}