using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class PodiumPanelController : MonoBehaviour
{
    [SerializeField] private TMP_Text firstPlaceText;
    [SerializeField] private TMP_Text secondPlaceText;
    [SerializeField] private TMP_Text thirdPlaceText;

    private void OnEnable()
    {
        ShowPodium();
    }


    public void ShowPodium()
    {

        // Get sorted totals from GameManager
        List<PlayerTotal> sortedTotals = GameManager.I.GetEventTotalsSorted();


        // Clear first
        firstPlaceText.text = "";
        secondPlaceText.text = "";
        thirdPlaceText.text = "";

        if (sortedTotals.Count > 0)
            firstPlaceText.text = $"{sortedTotals[0].playerName} : {sortedTotals[0].points}";

        if (sortedTotals.Count > 1)
            secondPlaceText.text = $"{sortedTotals[1].playerName} : {sortedTotals[1].points}";

        if (sortedTotals.Count > 2)
            thirdPlaceText.text = $"{sortedTotals[2].playerName} : {sortedTotals[2].points}";
    }
}