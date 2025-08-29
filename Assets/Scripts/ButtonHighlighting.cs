using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public static class UIHighlightHelper
{
    // Gold, Silver, Bronze colors
    private static readonly Color Gold = Color.yellow;
    private static readonly Color Silver = Color.gray;
    private static readonly Color Bronze = new Color(0.8f, 0.5f, 0.2f);
    private static readonly Color Default = Color.white;

    /// <summary>
    /// Highlights a button based on the order it was picked.
    /// </summary>
    public static void ApplyPickHighlight(Button btn, int position, List<int> currentSelection)
    {
        var colors = btn.colors;

        int pickIndex = currentSelection.IndexOf(position); // 0-based index in pick order
        Color targetColor = Default;

        if (pickIndex == 0)
            targetColor = Gold;
        else if (pickIndex == 1)
            targetColor = Silver;
        else if (pickIndex == 2)
            targetColor = Bronze;

        // Apply to all relevant states so it shows immediately
        colors.normalColor = targetColor;
        colors.highlightedColor = targetColor;
        colors.selectedColor = targetColor;

        btn.colors = colors;

        // Force immediate redraw
        if (btn.targetGraphic != null)
            btn.targetGraphic.SetAllDirty();
    }

    /// <summary>
    /// Highlights a TMP_Text label based on pick order (optional for results display).
    /// </summary>
    public static void ApplyResultHighlight(TMP_Text label, int position, List<int> currentSelection)
    {
        int pickIndex = currentSelection.IndexOf(position);

        if (pickIndex == 0)
            label.color = Gold;
        else if (pickIndex == 1)
            label.color = Silver;
        else if (pickIndex == 2)
            label.color = Bronze;
        else
            label.color = Default;
    }
}