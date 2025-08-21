using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static GameManager;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Canvas References")]
    public GameObject setupCanvas;
    public GameObject nightSelectionCanvas;
    public GameObject raceCanvas;
    public GameObject resultsCanvas;

    [HideInInspector] public int currentNightIndex;
    [HideInInspector] public int currentRaceIndex;

    public GameSessionData sessionData;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ShowCanvas(GameObject canvasToShow)
    {
        setupCanvas.SetActive(false);
        nightSelectionCanvas.SetActive(false);
        raceCanvas.SetActive(false);
        resultsCanvas.SetActive(false);

        canvasToShow.SetActive(true);
    }

    public void OnSetupComplete(int totalNights, string[] playerNames, GameMode mode)
    {
        sessionData.totalNights = totalNights;
        sessionData.players.Clear();
        foreach (var name in playerNames)
            sessionData.players.Add(new Player { playerName = name });

        sessionData.selectedGameMode = mode;
        ShowCanvas(nightSelectionCanvas);
        NightSelectionUI.Instance.BuildNightButtons(totalNights);
    }

    public void OnNightSelected(int nightIndex)
    {
        currentNightIndex = nightIndex;
        RaceListUI.Instance.LoadNight(nightIndex);
        ShowCanvas(raceCanvas);
    }

    public void OnRaceSelected(int raceIndex)
    {
        currentRaceIndex = raceIndex;
        RacePickUI.Instance.LoadRace(currentNightIndex, raceIndex);
    }

    public void ShowResultsForRace()
    {
        ResultsUI.Instance.ShowRaceResults(currentNightIndex, currentRaceIndex);
        ShowCanvas(resultsCanvas);
    }

    public void ShowNightLeaderboard()
    {
        ResultsUI.Instance.ShowNightLeaderboard(currentNightIndex);
        ShowCanvas(resultsCanvas);
    }
}

