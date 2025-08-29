using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PickPanelController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private UIManager ui;
    [SerializeField] private Transform positionsContent;
    [SerializeField] private GameObject positionButtonPrefab;
    [SerializeField] private TMP_Text header;

    [Header("Settings")]
    [SerializeField] private int overrideMaxPicks = 0; // 0 = use GameManager mode

    private readonly List<int> currentSelection = new();
    private readonly List<(int pos, Button btn)> buttons = new();
    private int maxPicks;

    private void OnEnable()
    {
        if (GameManager.I.ArePicksLocked())
        {
            ui.Show(PanelId.Player_Panel);
            return;
        }

        currentSelection.Clear();
        currentSelection.AddRange(GameManager.I.GetActivePlayerPicks());

        BuildChoices();
    }

    private void BuildChoices()
    {
        // Clear old buttons
        foreach (Transform c in positionsContent) Destroy(c.gameObject);
        buttons.Clear();

        var race = GameManager.I.GetSelectedRace();
        if (race == null) return;

        // Determine max picks
        var mode = GameManager.I.State.mode;
        maxPicks = overrideMaxPicks > 0
            ? overrideMaxPicks
            : (mode == GameMode.Top3 ? 3 : 1);

        if (header)
            header.text = $"{race.displayName} — pick up to {maxPicks}";

        int count = race.type == RaceType.Feature ? 20 : 10;

        for (int i = 1; i <= count; i++)
        {
            int pos = i;
            var go = Instantiate(positionButtonPrefab, positionsContent);
            var label = go.GetComponentInChildren<TMP_Text>();
            if (label) label.text = pos.ToString();

            var btn = go.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => TogglePick(pos));
                buttons.Add((pos, btn));
            }
        }

        RefreshAllButtonVisuals();
    }

    private void TogglePick(int pos)
    {
        if (currentSelection.Contains(pos))
        
            currentSelection.Remove(pos);
        
        else
        {
            if (currentSelection.Count >= maxPicks) return;
            currentSelection.Add(pos);
        }

        RefreshAllButtonVisuals();
    }


    private void RefreshAllButtonVisuals()
    {
        foreach (Transform child in positionsContent)
        {
            var btn = child.GetComponent<Button>();
            var label = child.GetComponentInChildren<TMP_Text>();

            if (btn != null && label != null && int.TryParse(label.text, out int pos))
            {
                // Apply highlight based on current pick order
                UIHighlightHelper.ApplyPickHighlight(btn, pos, currentSelection);

                // Keep buttons interactable so players can unpick
                btn.interactable = true;
            }
        }
    }



    public List<int> GetCurrentSelection() => new List<int>(currentSelection);

    public void OnBackSave()
    {
        if (currentSelection.Count == 0)
        {
            Debug.LogWarning("No picks selected.");
            // Optional: prompt user here
        }

        GameManager.I.SaveActivePlayerPicks(currentSelection);
        GameManager.I.Save();

        Debug.Log($"Saved picks for {GameManager.I.State.selectedPlayerName}: {string.Join(", ", currentSelection)}");
        ui.Show(PanelId.Player_Panel);
    }
}