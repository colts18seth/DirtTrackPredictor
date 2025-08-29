using System.Collections.Generic;

public enum RaceType { Qualifying, Heat, BMain, Feature }
public enum GameMode { SinglePick, Top3 } // Expand later as needed

[System.Serializable]
public class Player
{
    public string name;
}

[System.Serializable]
public class PlayerScore
{
    public string playerName;
    public int points;
}

[System.Serializable]
public class Race
{
    public string displayName;
    public RaceType type;
    public int invertCount;

    public List<PlayerPicks> picks = new();
    public List<int> results = new();

    // Persisted, per-race points (one entry per player)
    public List<PlayerScore> scores = new();

    // Optional: remove this or mark non-serialized to avoid saving runtime-only data
    [System.NonSerialized] public Dictionary<string, int> _totals = new();

    public int qualifyingWinnerCarNumber = -1;
    public bool picksLocked; // New
}

[System.Serializable]
public class PlayerPicks
{
    public string playerName; 
    public List<int> positions = new();

    public PlayerPicks(string name, IEnumerable<int> picks)
    {
        playerName = name;
        positions = new List<int>(picks);
    }

}

[System.Serializable]
public class RaceNight
{
    public int nightIndex;             // 1-based for display
    public List<Race> races = new();
    public bool finished;
}


[System.Serializable]
public class PlayerTotal
{
    public string playerName;
    public int points;
}

[System.Serializable]
public class EventState
{
    public string eventName;
    public int raceNightCount;
    public List<Player> players = new();
    public List<RaceNight> nights = new();
    public int currentNightIndex = 0;  // 0-based
    public int? selectedRaceIndex = null;
    public string selectedPlayerName = null;
    public GameMode mode = GameMode.SinglePick;

    // Overall totals across the event (all nights & races)
    public List<PlayerTotal> overallTotals = new();
}