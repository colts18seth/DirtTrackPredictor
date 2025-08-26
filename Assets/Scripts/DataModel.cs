using System.Collections.Generic;

public enum RaceType { Heat, BMain, Feature }
public enum GameMode { SinglePick, Top3 } // Expand later as needed

[System.Serializable]
public class Player
{
    public string name;
}

[System.Serializable]
public class Race
{
    public string displayName;         // e.g., "Heat 1"
    public RaceType type;
    public Dictionary<string, List<int>> picksByPlayer = new();
    // key: playerName; value: selected starting positions (SinglePick: 1 entry; Top3: 3 entries)
}

[System.Serializable]
public class RaceNight
{
    public int nightIndex;             // 1-based for display
    public List<Race> races = new();
    public bool finished;
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
    public string? selectedPlayerName = null;
    public GameMode mode = GameMode.SinglePick;
}