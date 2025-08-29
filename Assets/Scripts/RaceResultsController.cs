using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RaceResultsController : MonoBehaviour
{
    [SerializeField] private UIManager ui;
    [SerializeField] private Transform positionsContent;
    [SerializeField] private GameObject positionButtonPrefab;
    [SerializeField] private TMP_InputField Invert_Input ;
    [SerializeField] private TMP_Text feedback; // optional console text

    private readonly List<int> currentSelection = new();

    private static readonly Color GoldColor = new Color(1.0f, 0.843f, 0.0f);
    private static readonly Color SilverColor = new Color(0.60f, 0.60f, 0.60f);
    private static readonly Color BronzeColor = new Color(0.8f, 0.5f, 0.2f);


    private void OnEnable()
    {
        currentSelection.Clear();
        BuildResultsChoices();
    }

    private void BuildResultsChoices()
    {
        foreach (Transform c in positionsContent) Destroy(c.gameObject);

        var race = GameManager.I.GetSelectedRace();
        if (race == null) return;

        int count = race.type == RaceType.Feature ? 20 : 10;
        var mode = GameManager.I.State.mode;
        int maxPicks =  3;

        for (int i = 1; i <= count; i++)
        {
            int pos = i;
            var go = Instantiate(positionButtonPrefab, positionsContent);
            var label = go.GetComponentInChildren<TMP_Text>();
            if (label) label.text = pos.ToString();

            var btn = go.GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                TogglePick(pos, maxPicks, btn);
            });
        }
    }

    private void TogglePick(int pos, int maxPicks, Button btn)
    {
        if (currentSelection.Contains(pos))
            currentSelection.Remove(pos);
        else
        {
            if (currentSelection.Count >= maxPicks) return;
            currentSelection.Add(pos);
        }

        btn.interactable = !currentSelection.Contains(pos);

        RefreshResultsPanel();
    }

    private void RefreshResultsPanel()
    {
        foreach (Transform c in positionsContent)
        {
            var btn = c.GetComponent<Button>();
            var label = c.GetComponentInChildren<TMP_Text>();
            if (btn && label && int.TryParse(label.text, out int pos))
            {
                int order = currentSelection.IndexOf(pos);
                var colors = btn.colors;
                if (order == 0)
                    colors.disabledColor = GoldColor; // First place
                else if (order == 1)
                    colors.disabledColor = SilverColor;   // Second place
                else if (order == 2)
                    colors.disabledColor = BronzeColor;  // Third place
                else
                    colors.disabledColor = Color.white;  // Not selected
                btn.colors = colors;
            }
        }
    }

    //public void OnSave()
    //{
    //    //var invert = Invert_Input.GetComponentInChildren<TMP_Text>();
    //    GameManager.I.SaveRaceResultsForSelected( currentSelection );
    //    OnComputeScores();
    //    GameManager.I.LockPicksForSelectedRace();
    //    GameManager.I.Save();
    //    Debug.Log($"Saved picks: {string.Join(", ", currentSelection)}");
    //    ui.Show(PanelId.Race_Panel);

    //}

    public void OnSave()
    {
        // Save picks for the active player if applicable (optional)
        // GameManager.I.SaveActivePlayerPicks(currentSelection);

        // Save race results and invert
        int inv = int.TryParse(Invert_Input.text, out var invParsed) ? Mathf.Max(0, invParsed) : 0;
        GameManager.I.SaveRaceResultsForSelected(currentSelection, inv);

        // Compute per-race points and persist onto race.scores
        GameManager.I.RecomputeAndPersistScoresForSelectedRace();

        GameManager.I.LockPicksForSelectedRace();

        // Persist state
        GameManager.I.Save();

        // Optionally refresh leaderboard if you have a reference

        ui.Show(PanelId.Player_Panel);
    }


    public void OnComputeScores()
    {
        var s = GameManager.I.State;
        var night = s.nights[s.currentNightIndex];
        var race = night.races[s.selectedRaceIndex ?? 0];

        race.results.Clear();
        for (int i = 0; i < currentSelection.Count && i < 3; i++)
        {
            if (currentSelection[i] > 0)
                race.results.Add(currentSelection[i]);
        }


        race.invertCount = int.TryParse(Invert_Input.text, out int inv) ? Mathf.Max(0, inv) : 0;

        // Score all players for this race
        Dictionary<string, int> points = ScoreManager.I.ScoreCurrentRace(s);

        // Optionally accumulate to event totals (simple runtime example)
        foreach (var kv in points)
            AddToEventTotal(kv.Key, kv.Value);

        GameManager.I.Save();

        if (feedback)
        {
            feedback.text = $"Scored {race.displayName}:\n";
            foreach (var kv in points)
                feedback.text += $"{kv.Key}: {kv.Value}\n";
        }
        //Debug.Log($"Scored {race.displayName}:\n");
        //foreach (var kv in points)
        //Debug.Log($"{kv.Key}: {kv.Value}");
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
}

