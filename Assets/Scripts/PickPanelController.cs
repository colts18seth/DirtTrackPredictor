using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PickPanelController : MonoBehaviour
{
    [SerializeField] private UIManager ui;
    [SerializeField] private Transform positionsContent;
    [SerializeField] private GameObject positionButtonPrefab;
    [SerializeField] private TMP_Text header;

    private readonly List<int> currentSelection = new();
    private int maxPicks;

    private void OnEnable()
    {
        if (GameManager.I.ArePicksLocked())
        {
            //ui.ShowMessage("Picks for this race are locked — results have been entered.");
            ui.Show(PanelId.Player_Panel);
            return;
        }

        currentSelection.Clear();
        currentSelection.AddRange(GameManager.I.GetActivePlayerPicks());
        BuildChoices();
    }

    private void BuildChoices()
    {
        foreach (Transform c in positionsContent) Destroy(c.gameObject);

        var race = GameManager.I.GetSelectedRace();
        if (race == null) return;

        int count = race.type == RaceType.Feature ? 20 : 10;
        var mode = GameManager.I.State.mode;
        maxPicks = mode == GameMode.Top3 ? 3 : 1;

        if (header) header.text = $"{race.displayName} — pick up to {maxPicks}";

        for (int i = 1; i <= count; i++)
        {
            int pos = i;
            var go = Instantiate(positionButtonPrefab, positionsContent);
            var label = go.GetComponentInChildren<TMP_Text>();
            if (label) label.text = pos.ToString();

            var btn = go.GetComponent<Button>();
            RefreshButtonVisual(btn, pos);

            btn.onClick.AddListener(() =>
            {
                TogglePick(pos, btn);
            });
        }
    }

    private void TogglePick(int pos, Button btn)
    {
        if (currentSelection.Contains(pos))
            currentSelection.Remove(pos);
        else
        {
            if (currentSelection.Count >= maxPicks) return;
            currentSelection.Add(pos);
        }

        RefreshButtonVisual(btn, pos);
    }

    private void RefreshButtonVisual(Button btn, int pos)
    {
        bool picked = currentSelection.Contains(pos);
        UIHighlightHelper.ApplyPickHighlight(btn, picked);
    }

    public void OnBackSave()
    {
        if (currentSelection.Count == 0)
        {
            Debug.LogWarning("No picks selected.");
            // Optional: prompt user
        }

        GameManager.I.SaveActivePlayerPicks(currentSelection);
        GameManager.I.Save();
        Debug.Log($"Saved picks for {GameManager.I.State.selectedPlayerName}: {string.Join(", ", currentSelection)}");
        ui.Show(PanelId.Player_Panel);
    }
}