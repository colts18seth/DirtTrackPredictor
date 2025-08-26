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

    private void OnEnable()
    {
        currentSelection.Clear();
        BuildChoices();
    }

    private void BuildChoices()
    {
        foreach (Transform c in positionsContent) Destroy(c.gameObject);

        var race = GameManager.I.GetSelectedRace();
        if (race == null) return;

        int count = race.type == RaceType.Feature ? 20 : 10;
        var mode = GameManager.I.State.mode;
        int maxPicks = mode == GameMode.Top3 ? 3 : 1;

        if (header) header.text = $"{race.displayName} — pick up to {maxPicks}";

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

        // Simple visual: toggle interactable to reflect selection
        btn.interactable = !currentSelection.Contains(pos);
    }

    public void OnBackSave()
    {
        var player = GameManager.I.State.selectedPlayerName;
        if (!string.IsNullOrEmpty(player))
            GameManager.I.SavePicksForSelected(player, currentSelection);

        GameManager.I.Save();
        ui.Show(PanelId.Player_Panel);
    }
}