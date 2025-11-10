using UnityEngine;
using TMPro;

public class TerminalDisplay : MonoBehaviour
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

