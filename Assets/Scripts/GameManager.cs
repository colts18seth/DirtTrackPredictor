using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }

    public EventState State { get; private set; } = new();

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    // New event (called from Setup after user inputs)
    public void CreateNewEvent(string eventName, int nights, List<string> playerNames, GameMode mode)
    {
        State = new EventState
        {
            eventName = eventName,
            raceNightCount = nights,
            mode = mode
        };
        foreach (var n in playerNames) State.players.Add(new Player { name = n });

        State.nights = new List<RaceNight>();
        for (int i = 0; i < nights; i++)
            State.nights.Add(new RaceNight { nightIndex = i + 1 });

        State.currentNightIndex = 0;
        State.selectedRaceIndex = null;
        State.selectedPlayerName = null;
    }

    public void SetCurrentNight(int nightIndex0Based)
    {
        State.currentNightIndex = Mathf.Clamp(nightIndex0Based, 0, State.nights.Count - 1);
        State.selectedRaceIndex = null;
        State.selectedPlayerName = null;
    }

    public Race AddRaceToCurrentNight(RaceType type, string displayName)
    {
        var night = State.nights[State.currentNightIndex];
        var race = new Race { type = type, displayName = displayName };
        night.races.Add(race);
        State.selectedRaceIndex = night.races.Count - 1;
        return race;
    }

    public Race? GetSelectedRace()
    {
        var idx = State.selectedRaceIndex;
        if (idx == null) return null;
        var night = State.nights[State.currentNightIndex];
        if (idx.Value < 0 || idx.Value >= night.races.Count) return null;
        return night.races[idx.Value];
    }

    public void SelectRace(int raceIndex)
    {
        var night = State.nights[State.currentNightIndex];
        State.selectedRaceIndex = Mathf.Clamp(raceIndex, 0, night.races.Count - 1);
    }

    public void SelectPlayer(string playerName) => State.selectedPlayerName = playerName;

    public void SavePicksForSelected(string playerName, List<int> positions)
    {
        var race = GetSelectedRace();
        if (race == null || string.IsNullOrEmpty(playerName))
            return;

        // Find existing entry
        var playerPicks = race.picks.FirstOrDefault(pp => pp.playerName == playerName);

        if (playerPicks == null)
        {
            // Create a new entry if not found
            playerPicks = new PlayerPicks { playerName = playerName };
            race.picks.Add(playerPicks);
        }

        // Overwrite the picks list with the new positions
        playerPicks.positions = new List<int>(positions);
    }

    public PlayerPicks GetOrCreatePicks(string playerName)
    {
        var race = GetSelectedRace();
        var existing = race.picks.FirstOrDefault(pp => pp.playerName == playerName);
        if (existing == null)
        {
            existing = new PlayerPicks { playerName = playerName };
            race.picks.Add(existing);
        }
        return existing;
    }

    public void SaveRaceResultsForSelected(List<int> positions )
    {
        var race = GetSelectedRace();
        if (race == null) return;
        race.results = new List<int>(positions);
    }


    // Simple persistence for Resume (swap to proper save later)
    private const string SaveKey = "RacePredictor_EventState";

    public void Save()
    {
        var json = JsonUtility.ToJson(State);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public bool Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return false;
        var json = PlayerPrefs.GetString(SaveKey);
        var loaded = JsonUtility.FromJson<EventState>(json);
        if (loaded == null) return false;
        State = loaded;
        return true;
    }

    public void ClearSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
    }

    internal bool HasPlayerPicked(Race race, Player p)
    {
        if (race == null || p == null || string.IsNullOrEmpty(p.name))
            return false;

        var playerPicks = race.picks.FirstOrDefault(pp => pp.playerName == p.name);
        return playerPicks != null && playerPicks.positions != null && playerPicks.positions.Count > 0;
    }

}