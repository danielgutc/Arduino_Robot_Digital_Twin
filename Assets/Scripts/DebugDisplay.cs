using UnityEngine;
using TMPro;

public class DebugDisplay : MonoBehaviour
{
    public TextMeshProUGUI displayText;

    public void UpdateDisplay(string log)
    {
        if (displayText != null)
        {
            displayText.text = log;
        }
    }
}

