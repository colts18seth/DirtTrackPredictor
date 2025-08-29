using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class UIHighlightHelper
{
    public static void ApplyPickHighlight(Button btn, bool picked)
    {
        var colors = btn.colors;
        colors.normalColor = picked ? Color.green : Color.white;
        btn.colors = colors;
        btn.interactable = !picked; // Disable if selected
    }

    public static void ApplyResultHighlight(TMP_Text label, int position)
    {
        switch (position)
        {
            case 1: label.color = Color.yellow; break; // Gold
            case 2: label.color = Color.gray; break;   // Silver
            case 3: label.color = new Color(0.8f, 0.5f, 0.2f); break; // Bronze
            default: label.color = Color.white; break;
        }
    }
}