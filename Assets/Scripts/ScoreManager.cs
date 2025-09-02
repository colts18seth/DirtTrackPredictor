using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager I { get; private set; }
    [SerializeField] private ScoringSettings settings;

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    // SINGLE PICK: 1 pick, podium pays, underdog multiplier applies
    public int ScoreSinglePick(int pickedStartPos, List<int> results, int invertCount)
    {
        if (results == null || results.Count < 3) return 0;

        int idx = IndexOf(results, pickedStartPos); // 0,1,2 or -1
        if (idx == -1) return 0;

        int basePts = idx switch
        {
            0 => settings.winPoints,
            1 => settings.placePoints,
            2 => settings.showPoints,
            _ => 0
        };

        float mult = UnderdogMultiplier(pickedStartPos, invertCount);
        return Mathf.RoundToInt(basePts * mult);
    }

    // TOP 3: 3 ordered picks; pays for being in top3 + extra for exact place, underdog applies per pick
    public int ScoreTop3(IList<int> picksInOrder, IList<int> resultsTop3, int invertCount)
    {
        if (picksInOrder == null || resultsTop3 == null) return 0;

        int total = 0;
        for (int i = 0; i < Mathf.Min(3, picksInOrder.Count); i++)
        {
            int picked = picksInOrder[i];
            int exactIdx = IndexOf(resultsTop3, picked);
            if (exactIdx == -1) continue;

            bool correctPlace = (exactIdx == i);

            int exactBonus = correctPlace? (i switch
            {
                0 => settings.top3ExactPlaceBonus1,
                1 => settings.top3ExactPlaceBonus2,
                2 => settings.top3ExactPlaceBonus3,
                _ => 0
            })
            : 0; // No exact bonus if wrong slot



        float mult = UnderdogMultiplier(picked, invertCount);
            total += Mathf.RoundToInt((settings.top3BasePoints + exactBonus) * mult);
        }
        return total;
    }

    // QUALIFYING: exact match bonus
    public int ScoreQualifying(int predictedCarNumber, int actualTopCarNumber)
    {
        return predictedCarNumber == actualTopCarNumber ? settings.qualifyingExactBonus : 0;
    }

    // Convenience: score the currently selected race for all players and return a per-player map
    public Dictionary<string, int> ScoreCurrentRace(EventState s)
    {
        var result = new Dictionary<string, int>();
        if (s == null || s.selectedRaceIndex == null) return result;

        var night = s.nights[s.currentNightIndex];
        var race = night.races[s.selectedRaceIndex.Value];

        foreach (var p in s.players)
        {
            var picks = GetPicksForPlayer(race, p.name) ?? new List<int>();

            int pts = 0;

            if (race.type == RaceType.Qualifying)
            {
                // Expecting a single car number pick saved somewhere you choose
                // pts = ScoreQualifying(predictedCarNumberForPlayer, race.qualifyingWinnerCarNumber);
            }
            else if (s.mode == GameMode.SinglePick)
            {
                if (picks != null && picks.Count > 0)
                    pts = ScoreSinglePick(picks[0], race.results, race.invertCount);
            }
            else // Top3
            {
                pts = ScoreTop3(picks ?? new List<int>(), race.results, race.invertCount);
            }

            result[p.name] = pts;
        }

        return result;
    }

    // ---- internals ----
    private float UnderdogMultiplier(int startingPos, int invertCount)
    {
        int effectiveStart = startingPos;
        bool invertedStarter = invertCount > 0 && startingPos <= invertCount;

        if (settings.useEffectiveStartForInvert && invertedStarter)
        {
            // Map starting position to qualifying rank within inverted block
            effectiveStart = invertCount - startingPos + 1;
        }

        float m = 1f + (1f - Mathf.Exp(-settings.underdogK * (effectiveStart - 1)));
        if (settings.underdogMax > 0f) m = Mathf.Min(m, settings.underdogMax);
        //if (invertedStarter) m *= settings.invertStarterPenalty;
        return m;
    }

    private static int IndexOf(IList<int> list, int value)
    {
        for (int i = 0; i < list.Count; i++)
            if (list[i] == value) return i;
        return -1;
    }

    private static List<int> GetPicksForPlayer(Race race, string playerName)
    {
        if (race == null || race.picks == null) return null;
        for (int i = 0; i < race.picks.Count; i++)
        {
            if (race.picks[i].playerName == playerName) return race.picks[i].positions;
        }
        return null;
    }

    public int CalculateScoreForActivePlayer()
    {
        var picks = GameManager.I.GetActivePlayerPicks();
        var race = GameManager.I.GetSelectedRace();
        if (picks.Count == 0 || race == null) return 0;

        int score = 0;
        foreach (var pick in picks)
        {
            // Example: 1st place is worth 10, 2nd = 9, etc.
            int points = Mathf.Max(0, 11 - pick);
            score += points;
        }
        return score;
    }

}