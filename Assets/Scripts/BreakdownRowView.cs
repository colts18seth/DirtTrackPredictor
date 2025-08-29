using TMPro;
using UnityEngine;

public class BreakdownRowView : MonoBehaviour
{
    [SerializeField] private TMP_Text raceNameText;
    [SerializeField] private TMP_Text pointsText;

    public void Set(string raceName, int points)
    {
        if (raceNameText) raceNameText.text = raceName;
        if (pointsText) pointsText.text = points.ToString();
    }
}