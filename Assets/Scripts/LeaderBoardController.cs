using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class LeaderboardController : MonoBehaviour
{
    [Header("Standings UI")]
    [SerializeField] private Transform nightContent;         // rows use LeaderboardRowView
    [SerializeField] private Transform eventContent;         // rows use LeaderboardRowView
    [SerializeField] private GameObject leaderboardRowPrefab;

    [Header("Breakdown UI")]
    [SerializeField] private Transform breakdownContent;     // rows use BreakdownRowView
    [SerializeField] private GameObject breakdownRowPrefab;
    [SerializeField] private TMP_Text breakdownHeader;

    [Header("Controls (Optional)")]
    [SerializeField] private TMP_Dropdown nightDropdown;     // leave null to use current night
    [SerializeField] private TMP_Text nightHeader;           // shows “Night X Standings”
    [SerializeField] private bool onlyScoredRaces = true;    // filter for breakdown and night totals

    private void OnEnable()
    {
        PopulateNightDropdownIfPresent();
        Refresh();
    }

    public void Refresh()
    {
        if (GameManager.I == null || GameManager.I.State == null) return;

        int nightIndex = ResolveNightIndex();

        // Night standings
        var nightTotals = GetNightTotals(nightIndex, onlyScoredRaces);
        BuildStandingsList(nightContent, nightTotals);

        // Event standings (season totals)
        var eventTotals = GameManager.I.GetEventTotalsSorted();
        BuildStandingsList(eventContent, eventTotals);

        // Optional headers
        if (nightHeader) nightHeader.text = $"Night {nightIndex + 1} Standings";

        // Clear breakdown on refresh
        BuildBreakdown(null, nightIndex);
    }

    // --- Standings builders ---

    private void BuildStandingsList(Transform parent, List<PlayerTotal> data)
    {
        if (!parent || leaderboardRowPrefab == null) return;
        foreach (Transform c in parent) Destroy(c.gameObject);

        foreach (var row in data)
        {
            var go = Instantiate(leaderboardRowPrefab, parent);
            var view = go.GetComponent<LeaderboardRowView>();
            if (view != null)
            {
                // Clicking a player row shows breakdown for selected night
                view.Set(row.playerName, row.points, playerName =>
                {
                    int nightIndex = ResolveNightIndex();
                    BuildBreakdown(playerName, nightIndex);
                });
            }
            else
            {
                // Fallback if prefab missing component
                var texts = go.GetComponentsInChildren<TMP_Text>();
                if (texts.Length >= 2)
                {
                    texts[0].text = row.playerName;
                    texts[1].text = row.points.ToString();
                }
            }
        }
    }

    // --- Breakdown builder ---

    private void BuildBreakdown(string playerName, int nightIndex)
    {
        if (!breakdownContent || breakdownRowPrefab == null) return;
        foreach (Transform c in breakdownContent) Destroy(c.gameObject);

        if (string.IsNullOrEmpty(playerName))
        {
            if (breakdownHeader) breakdownHeader.text = "Select a player for per-race breakdown";
            return;
        }

        var state = GameManager.I.State;
        if (state == null || nightIndex < 0 || nightIndex >= state.nights.Count) return;

        var night = state.nights[nightIndex];
        if (breakdownHeader) breakdownHeader.text = $"{playerName} — Night {nightIndex + 1} Breakdown";

        // Build rows in race order
        foreach (var race in night.races)
        {
            if (onlyScoredRaces && (race.scores == null || race.scores.Count == 0))
                continue;

            int points = 0;
            if (race.scores != null)
            {
                var ps = race.scores.FirstOrDefault(s => s.playerName == playerName);
                points = ps != null ? ps.points : 0;
            }

            var go = Instantiate(breakdownRowPrefab, breakdownContent);
            var view = go.GetComponent<BreakdownRowView>();
            if (view != null)
                view.Set(race.displayName, points);
            else
            {
                var texts = go.GetComponentsInChildren<TMP_Text>();
                if (texts.Length >= 2)
                {
                    texts[0].text = race.displayName;
                    texts[1].text = points.ToString();
                }
            }
        }
    }

    // --- Night totals ---

    private List<PlayerTotal> GetNightTotals(int nightIndex, bool onlyScored)
    {
        var totals = new Dictionary<string, int>();
        var state = GameManager.I.State;
        if (state == null || nightIndex < 0 || nightIndex >= state.nights.Count) return new List<PlayerTotal>();

        var night = state.nights[nightIndex];
        foreach (var race in night.races)
        {
            if (onlyScored && (race.scores == null || race.scores.Count == 0)) continue;

            if (race.scores == null) continue;
            foreach (var ps in race.scores)
            {
                if (!totals.ContainsKey(ps.playerName))
                    totals[ps.playerName] = 0;
                totals[ps.playerName] += ps.points;
            }
        }

        return totals
            .Select(kv => new PlayerTotal { playerName = kv.Key, points = kv.Value })
            .OrderByDescending(pt => pt.points)
            .ThenBy(pt => pt.playerName)
            .ToList();
    }

    // --- Controls ---

    private int ResolveNightIndex()
    {
        if (nightDropdown && nightDropdown.options != null && nightDropdown.options.Count > 0)
            return Mathf.Clamp(nightDropdown.value, 0, GameManager.I.State.nights.Count - 1);

        return GameManager.I.State.currentNightIndex;
    }

    private void PopulateNightDropdownIfPresent()
    {
        if (!nightDropdown) return;

        nightDropdown.ClearOptions();
        var opts = new List<string>();
        var nights = GameManager.I.State.nights;
        for (int i = 0; i < nights.Count; i++)
            opts.Add($"Night {i + 1}");
        nightDropdown.AddOptions(opts);

        nightDropdown.value = GameManager.I.State.currentNightIndex;
        nightDropdown.onValueChanged.AddListener(_ =>
        {
            Refresh(); // also clears breakdown
        });
    }
}