using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static GameManager;

public class ResultsUI : MonoBehaviour
{
    public static ResultsUI Instance;
    public Transform resultContainer;
    public ResultRowUI resultRowPrefab;
    public TMP_Text headerText;

    private void Awake() => Instance = this;

    public void ShowRaceResults(int nightIndex, int raceIndex)
    {
        ClearContainer();
        var race = GameSessionData.Instance.nights[nightIndex].races[raceIndex];
        headerText.text = $"Results – {race.raceName}";

        // Sort by points awarded in that race
        var sorted = new List<(string playerName, int points)>();
        foreach (var kvp in race.pointsAwarded)
            sorted.Add((kvp.Key, kvp.Value));
        sorted.Sort((a, b) => b.points.CompareTo(a.points));

        for (int i = 0; i < Mathf.Min(3, sorted.Count); i++)
        {
            var row = Instantiate(resultRowPrefab, resultContainer);
            row.Init(i + 1, sorted[i].playerName, sorted[i].points);
        }
    }

    public void ShowNightLeaderboard(int nightIndex)
    {
        ClearContainer();
        headerText.text = $"Night {nightIndex + 1} Leaderboard";

        var topThree = GameSessionData.Instance.GetTopThreeForNight(nightIndex);
        for (int i = 0; i < topThree.Count; i++)
        {
            var row = Instantiate(resultRowPrefab, resultContainer);
            row.Init(i + 1, topThree[i].playerName, topThree[i].points);
        }
    }

    private void ClearContainer()
    {
        foreach (Transform child in resultContainer)
            Destroy(child.gameObject);
    }
}