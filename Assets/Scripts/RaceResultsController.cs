using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RaceResultsController : MonoBehaviour
{
    [SerializeField] private UIManager ui;
    [SerializeField] private Transform positionsContent;
    [SerializeField] private GameObject positionButtonPrefab;

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

    public void OnSave()
    {
        GameManager.I.SaveRaceResultsForSelected(currentSelection);

        GameManager.I.Save();
        Debug.Log($"Saved picks: {string.Join(", ", currentSelection)}");
        ui.Show(PanelId.Race_Panel);
    }
}
