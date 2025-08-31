using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class LeaderboardController : MonoBehaviour
{
    private enum ViewMode { NightStandings, EventStandings, Breakdown }

    [Header("Single Content Area")]
    [SerializeField] private Transform contentParent;           // one content box for all views
    [SerializeField] private TMP_Text headerText;               // shows current view title

    [Header("Row Prefabs")]
    [SerializeField] private GameObject leaderboardRowPrefab;   // LeaderboardRowView
    [SerializeField] private GameObject breakdownRowPrefab;     // BreakdownRowView

    [Header("Controls")]
    [SerializeField] private TMP_Dropdown nightDropdown;        // optional: pick night index
    [SerializeField] private bool onlyScoredRaces = true;       // filter for night totals and breakdown

    private ViewMode currentMode = ViewMode.NightStandings;
    private string currentBreakdownPlayer = null;

    private void OnEnable()
    {
        PopulateNightDropdownIfPresent();
        // Default to current night standings when panel opens
        ShowNightStandings();
    }

    // -------- Public UI Actions (bind these to buttons/tabs) --------

    public void OnClickShowNightStandings()
    {
        ShowNightStandings();
    }

    public void OnClickShowEventStandings()
    {
        ShowEventStandings();
    }

    // Called by rows via callback to display per-race breakdown
    private void OnRowClicked(string playerName)
    {
        ShowBreakdown(playerName);
    }

    // If using a dropdown, wire this in the Inspector
    public void OnNightDropdownChanged(int _)
    {
        switch (currentMode)
        {
            case ViewMode.NightStandings:
                ShowNightStandings();      // rebuild for new night
                break;
            case ViewMode.Breakdown:
                ShowBreakdown(currentBreakdownPlayer); // rebuild breakdown with new night
                break;
            case ViewMode.EventStandings:
                // Event standings do not depend on night
                break;
        }
    }

    // -------- View Builders --------

    private void ShowNightStandings()
    {
        currentMode = ViewMode.NightStandings;
        currentBreakdownPlayer = null;

        ClearContent();
        int nightIndex = ResolveNightIndex();

        var nightTotals = GetNightTotals(nightIndex, onlyScoredRaces);
        BuildStandingsList(contentParent, nightTotals);

        if (headerText) headerText.text = $"Night {nightIndex + 1} Standings";
    }

    private void ShowEventStandings()
    {
        currentMode = ViewMode.EventStandings;
        currentBreakdownPlayer = null;

        ClearContent();
        var eventTotals = GameManager.I.GetEventTotalsSorted();
        BuildStandingsList(contentParent, eventTotals);

        if (headerText) headerText.text = "Season Standings";
    }

    private void ShowBreakdown(string playerName)
    {
        currentMode = ViewMode.Breakdown;
        currentBreakdownPlayer = playerName;

        ClearContent();
        int nightIndex = ResolveNightIndex();
        BuildBreakdown(contentParent, playerName, nightIndex);

        if (headerText) headerText.text = $"{playerName} — Night {nightIndex + 1} Breakdown";
    }

    // -------- Internals --------

    private void BuildStandingsList(Transform parent, List<PlayerTotal> data)
    {
        if (!parent || leaderboardRowPrefab == null) return;

        foreach (var row in data)
        {
            var go = Instantiate(leaderboardRowPrefab, parent);
            var view = go.GetComponent<LeaderboardRowView>();
            if (view != null)
            {
                // Clicking a row switches the single content box to Breakdown view
                view.Set(row.playerName, row.points, OnRowClicked);
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

    private void BuildBreakdown(Transform parent, string playerName, int nightIndex)
    {
        if (!parent || breakdownRowPrefab == null) return;
        if (string.IsNullOrEmpty(playerName)) return;

        var state = GameManager.I.State;
        if (state == null || nightIndex < 0 || nightIndex >= state.nights.Count) return;

        var night = state.nights[nightIndex];

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

            var go = Instantiate(breakdownRowPrefab, parent);
            var view = go.GetComponent<BreakdownRowView>();
            if (view != null)
            {
                view.Set(race.displayName, points);
            }
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

        nightDropdown.value = Mathf.Clamp(GameManager.I.State.currentNightIndex, 0, Mathf.Max(0, nights.Count - 1));
        nightDropdown.onValueChanged.RemoveAllListeners();
        nightDropdown.onValueChanged.AddListener(OnNightDropdownChanged);
    }

    private void ClearContent()
    {
        foreach (Transform c in contentParent)
            Destroy(c.gameObject);
    }
}