using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameManager;

public class RacePickUI : MonoBehaviour
{
    public static RacePickUI Instance;
    public TMP_Text raceNameText;
    public Transform pickContainer;
    public Button confirmButton;
    public TMP_Dropdown racerDropdown; // Could also be a list of buttons

    private int currentNight;
    private int currentRace;

    private void Awake() => Instance = this;

    public void LoadRace(int nightIndex, int raceIndex)
    {
        currentNight = nightIndex;
        currentRace = raceIndex;

        var race = GameSessionData.Instance.nights[nightIndex].races[raceIndex];
        raceNameText.text = race.raceName;
    }

    public void OnConfirmPick()
    {
        List<RacePick> picks = new();

        // Example for SinglePick:
        picks.Add(new RacePick
        {
            racerName = racerDropdown.options[racerDropdown.value].text,
            position = 1
        });

        GameSessionData.Instance.RecordPick(currentNight, currentRace,
            "PlayerNameHere", picks); // Replace with active player logic

        UIManager.Instance.ShowResultsForRace();
    }
}