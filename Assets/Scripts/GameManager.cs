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

    public Race GetSelectedRace()
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

    public void SaveActivePlayerPicks(IEnumerable<int> picks)
    {
        var race = GetSelectedRace();
        var player = State.selectedPlayerName;
        if (race == null || string.IsNullOrEmpty(player)) return;

        var existing = race.picks.FirstOrDefault(p => p.playerName == player);
        if (existing != null)
            existing.positions = new List<int>(picks);
        else
            race.picks.Add(new PlayerPicks(player, picks));
    }

    public List<int> GetActivePlayerPicks()
    {
        var race = GetSelectedRace();
        var player = State.selectedPlayerName;
        if (race == null || string.IsNullOrEmpty(player)) return new List<int>();

        var existing = race.picks.FirstOrDefault(p => p.playerName == player);
        return existing != null ? new List<int>(existing.positions) : new List<int>();
    }

    public void SaveRaceResultsForSelected(List<int> positions, int invertCount)
    {
        var race = GetSelectedRace();
        if (race == null) return;
        race.results = new List<int>(positions);
        race.invertCount = Mathf.Max(0, invertCount);
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
        foreach (var night in State.nights)
        {
            foreach (var race in night.races)
            {
                race.RebuildTotals();
            }
        }

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

    public void LockPicksForSelectedRace()
    {
        var race = GetSelectedRace();
        if (race != null)
            race.picksLocked = true;
    }

    public bool ArePicksLocked()
    {
        var race = GetSelectedRace();
        return race != null && race.picksLocked;
    }


    // Call this after you’ve set race.results + race.invertCount + saved player picks.
    public void RecomputeAndPersistScoresForSelectedRace()
    {
        var s = State;
        if (s == null || s.selectedRaceIndex == null) return;

        var night = s.nights[s.currentNightIndex];
        var race = night.races[s.selectedRaceIndex.Value];
        if (race == null) return;

        // Compute points per player
        var dict = ScoreManager.I.ScoreCurrentRace(s);

        // Save them onto the race for persistence
        race.scores = dict.Select(kv => new PlayerScore
        {
            playerName = kv.Key,
            points = kv.Value
        }).ToList();

        GameManager.I.UpdateEventTotals(dict);
        foreach (var kv in dict)
            AddToEventTotal(kv.Key, kv.Value);


        UnityEngine.Debug.Log($"Scored {race.displayName}:\n");
        foreach (var kv in dict) UnityEngine.Debug.Log($"{kv.Key}: {kv.Value}");

        //Save();
    }

    private void AddToEventTotal(string player, int delta)
    {
        var race = GameManager.I.GetSelectedRace();
        if (race == null) return;

        // Store totals in a serializable way if you want persistence (e.g., a list on EventState).
        // For simplicity here, we’ll keep it transient on GameManager; adapt to your save system as needed.
        if (!race._totals.TryGetValue(player, out int cur))
            cur = 0;
        race._totals[player] = cur + delta;

    }

    // Aggregate totals for a single night (sum all races in that night)
    public List<PlayerScore> GetNightTotals(int nightIndex, bool onlyScoredRaces = true)
    {
        var scores = new Dictionary<string, int>();
        if (State == null || State.nights == null || nightIndex < 0 || nightIndex >= State.nights.Count)
            return new List<PlayerScore>();

        var night = State.nights[nightIndex];
        foreach (var race in night.races)
        {
            // Optionally skip races without saved scores
            if (onlyScoredRaces && (race.scores == null || race.scores.Count == 0)) continue;

            if (race.scores == null) continue;
            foreach (var ps in race.scores)
            {
                if (!scores.ContainsKey(ps.playerName)) scores[ps.playerName] = 0;
                scores[ps.playerName] += ps.points;
            }
        }

        return scores
            .Select(kv => new PlayerScore { playerName = kv.Key, points = kv.Value })
            .OrderByDescending(ps => ps.points)
            .ThenBy(ps => ps.playerName)
            .ToList();
    }

    // Aggregate totals across all nights in the event
    public List<PlayerScore> GetEventTotals(bool onlyScoredRaces = true)
    {
        var scores = new Dictionary<string, int>();
        if (State == null || State.nights == null) return new List<PlayerScore>();

        foreach (var night in State.nights)
        {
            foreach (var race in night.races)
            {
                if (onlyScoredRaces && (race.scores == null || race.scores.Count == 0)) continue;

                if (race.scores == null) continue;
                foreach (var ps in race.scores)
                {
                    if (!scores.ContainsKey(ps.playerName)) scores[ps.playerName] = 0;
                    scores[ps.playerName] += ps.points;
                }
            }
        }

        return scores
            .Select(kv => new PlayerScore { playerName = kv.Key, points = kv.Value })
            .OrderByDescending(ps => ps.points)
            .ThenBy(ps => ps.playerName)
            .ToList();
    }

    public void UpdateEventTotals(Dictionary<string, int> latestRaceScores)
    {
        if (State == null) return;

        // Ensure list exists
        if (State.overallTotals == null)
            State.overallTotals = new List<PlayerTotal>();

        foreach (var kv in latestRaceScores)
        {
            var entry = State.overallTotals.FirstOrDefault(pt => pt.playerName == kv.Key);
            if (entry == null)
            {
                entry = new PlayerTotal { playerName = kv.Key, points = kv.Value };
                State.overallTotals.Add(entry);
            }
            else
            {
                entry.points += kv.Value;
            }
        }
    }

    public List<PlayerTotal> GetEventTotalsSorted()
    {
        return State.overallTotals
            .OrderByDescending(pt => pt.points)
            .ThenBy(pt => pt.playerName)
            .ToList();
    }

    public List<PlayerTotal> GetNightTotalsSorted(int nightIndex, bool onlyScoredRaces = true)
    {
        var totals = new Dictionary<string, int>();
        if (State == null || State.nights == null || nightIndex < 0 || nightIndex >= State.nights.Count)
            return new List<PlayerTotal>();

        var night = State.nights[nightIndex];
        foreach (var race in night.races)
        {
            if (onlyScoredRaces && (race.scores == null || race.scores.Count == 0)) continue;

            if (race.scores == null) continue;
            foreach (var ps in race.scores)
            {
                if (!totals.ContainsKey(ps.playerName)) totals[ps.playerName] = 0;
                totals[ps.playerName] += ps.points;
            }
        }

        return totals
            .Select(kv => new PlayerTotal { playerName = kv.Key, points = kv.Value })
            .OrderByDescending(pt => pt.points)
            .ThenBy(pt => pt.playerName)
            .ToList();
    }

    public int GetPlayerNightTotal(string playerName, bool onlyScoredRaces = true)
    {
        if (State == null || State.nights == null || State.currentNightIndex < 0 || State.currentNightIndex >= State.nights.Count)
            return 0;

        int total = 0;
        var night = State.nights[State.currentNightIndex];
        foreach (var race in night.races)
        {
            if (onlyScoredRaces && (race.scores == null || race.scores.Count == 0)) continue;
            if (race.scores == null) continue;

            var score = race.scores.FirstOrDefault(ps => ps.playerName == playerName);
            if (score != null)
                total += score.points;
        }
        return total;
    }
}