using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum GameMode
    {
        SinglePick,   // 1 pick per race
        TopThree      // Top 3 picks per race
    }

    [Serializable]
    public class Player
    {
        public string playerName;
        public Dictionary<int, int> totalPointsPerNight = new(); // nightIndex -> points
    }

    [Serializable]
    public class RacePick
    {
        public string racerName;
        public int position; // 1 = first, 2 = second, 3 = third
    }

    [Serializable]
    public class RaceResult
    {
        public string raceName;
        public List<RacePick> picks = new(); // Player's picks
        public List<string> actualTopThree = new(); // Actual race finishers
        public Dictionary<string, int> pointsAwarded = new(); // playerName -> points
    }

    [Serializable]
    public class NightData
    {
        public int nightIndex;
        public List<RaceResult> races = new();
    }

    public class GameSessionData : MonoBehaviour
    {
        public static GameSessionData Instance; // Singleton

        [Header("Game Setup")]
        public int totalNights;
        public List<Player> players = new();
        public GameMode selectedGameMode;

        [Header("Gameplay Data")]
        public List<NightData> nights = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(gameObject);
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        // === SETUP METHODS ===
        public void InitializeSession(int nightsCount, List<string> playerNames, GameMode mode)
        {
            totalNights = nightsCount;
            selectedGameMode = mode;

            players.Clear();
            foreach (var name in playerNames)
                players.Add(new Player { playerName = name });

            nights.Clear();
            for (int i = 0; i < nightsCount; i++)
                nights.Add(new NightData { nightIndex = i });
        }

        public void AddRace(int nightIndex, string raceName)
        {
            if (nightIndex < 0 || nightIndex >= nights.Count) return;
            nights[nightIndex].races.Add(new RaceResult { raceName = raceName });
        }

        // === GAMEPLAY METHODS ===
        public void RecordPick(int nightIndex, int raceIndex, string playerName, List<RacePick> picks)
        {
            var race = GetRace(nightIndex, raceIndex);
            if (race == null) return;

            // Overwrite existing picks for this player
            race.picks.RemoveAll(p => p.racerName == playerName);
            race.picks.AddRange(picks);
        }

        public void SetActualResults(int nightIndex, int raceIndex, List<string> results)
        {
            var race = GetRace(nightIndex, raceIndex);
            if (race == null) return;

            race.actualTopThree = new List<string>(results);
            CalculatePointsForRace(race, raceIndex);
        }

        private void CalculatePointsForRace(RaceResult race, int raceIndex)
        {
            race.pointsAwarded.Clear();

            foreach (var pick in race.picks)
            {
                int points = CalculatePoints(selectedGameMode, pick, race.actualTopThree);
                if (!race.pointsAwarded.ContainsKey(pick.racerName))
                    race.pointsAwarded[pick.racerName] = 0;

                race.pointsAwarded[pick.racerName] += points;
            }

            // Also update player's night totals
            foreach (var kvp in race.pointsAwarded)
            {
                var player = players.Find(p => p.playerName == kvp.Key);
                if (player != null)
                {
                    if (!player.totalPointsPerNight.ContainsKey(raceIndex))
                        player.totalPointsPerNight[raceIndex] = 0;

                    player.totalPointsPerNight[raceIndex] += kvp.Value;
                }
            }
        }

        public int CalculatePoints(GameMode mode, RacePick pick, List<string> results)
        {
            if (mode == GameMode.SinglePick)
                return pick.racerName == results[0] ? 5 : 0;
            else
            {
                if (pick.racerName == results[0]) return 5;
                if (pick.racerName == results[1]) return 3;
                if (pick.racerName == results[2]) return 1;
                return 0;
            }
        }

        // === LEADERBOARD METHODS ===
        public List<(string playerName, int points)> GetTopThreeForNight(int nightIndex)
        {
            var scores = new Dictionary<string, int>();
            var night = nights.Find(n => n.nightIndex == nightIndex);
            if (night == null) return new List<(string, int)>();

            foreach (var race in night.races)
            {
                foreach (var kvp in race.pointsAwarded)
                {
                    if (!scores.ContainsKey(kvp.Key))
                        scores[kvp.Key] = 0;
                    scores[kvp.Key] += kvp.Value;
                }
            }

            var sorted = new List<(string, int)>();
            foreach (var kvp in scores)
                sorted.Add((kvp.Key, kvp.Value));

            sorted.Sort((a, b) => b.Item2.CompareTo(a.Item2));
            return sorted.GetRange(0, Mathf.Min(3, sorted.Count));
        }

        // === HELPERS ===
        private RaceResult GetRace(int nightIndex, int raceIndex)
        {
            if (nightIndex < 0 || nightIndex >= nights.Count) return null;
            var night = nights[nightIndex];
            if (raceIndex < 0 || raceIndex >= night.races.Count) return null;
            return night.races[raceIndex];
        }
    }
}