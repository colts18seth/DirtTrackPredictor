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
    [SerializeField] private TMP_InputField qualifyingInput; // NEW

    [Header("Settings")]
    [SerializeField] private int overrideMaxPicks = 0; // 0 = use GameManager mode

    private readonly List<int> currentSelection = new();
    private readonly List<(int pos, Button btn)> buttons = new();
    private int maxPicks;
    private Race currentRace;

    private void OnEnable()
    {
        currentSelection.Clear();
        currentSelection.AddRange(GameManager.I.GetActivePlayerPicks());

        currentRace = GameManager.I.GetSelectedRace();

        // Show/hide qualifying input
        if (qualifyingInput != null)
        {
            bool isQualifying = currentRace != null && currentRace.type == RaceType.Qualifying;
            qualifyingInput.gameObject.SetActive(isQualifying);

            if (isQualifying)
            {
                // Pre-fill if we already have a saved prediction
                string playerName = GameManager.I.State.selectedPlayerName;
                if (currentRace.qualifyingPredictions != null &&
                    currentRace.qualifyingPredictions.TryGetValue(playerName, out var savedPrediction))
                {
                    qualifyingInput.text = savedPrediction;
                }
                else
                {
                    qualifyingInput.text = "";
                }
            }
        }

        BuildChoices();
    }

    private void BuildChoices()
    {
        foreach (Transform c in positionsContent) Destroy(c.gameObject);
        buttons.Clear();

        if (currentRace == null) return;

        // If qualifying, skip building position buttons
        if (currentRace.type == RaceType.Qualifying)
        {
            if (header) header.text = $"{currentRace.displayName} : enter pole sitter";
            return;
        }


        var mode = GameManager.I.State.mode;
        maxPicks = overrideMaxPicks > 0
            ? overrideMaxPicks
            : (mode == GameMode.Top3 ? 3 : 1);

        if (header)
            header.text = $"{currentRace.displayName} : pick {maxPicks}";

        int count = currentRace.type == RaceType.Feature ? 20 : 10;

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
                UIHighlightHelper.ApplyPickHighlight(btn, pos, currentSelection);
                btn.interactable = true;
            }
        }
    }

    public List<int> GetCurrentSelection() => new List<int>(currentSelection);

    public void OnBackSave()
    {
        if (currentSelection.Count == 0 && currentRace.type != RaceType.Qualifying)

        {
            Debug.LogWarning("No picks selected.");
        }

        GameManager.I.SaveActivePlayerPicks(currentSelection);

        // Save qualifying prediction if applicable
        if (qualifyingInput != null && qualifyingInput.gameObject.activeSelf)
        {
            string prediction = qualifyingInput.text.Trim();
            string playerName = GameManager.I.State.selectedPlayerName;

            if (!string.IsNullOrEmpty(prediction))
            {
                if (currentRace.qualifyingPredictions == null)
                    currentRace.qualifyingPredictions = new Dictionary<string, string>();

                currentRace.qualifyingPredictions[playerName] = prediction;
                Debug.Log($"Quali pick for  {GameManager.I.State.selectedPlayerName}: {string.Join(", ", prediction)}");
            }
        }

        GameManager.I.Save();

        Debug.Log($"Saved picks for {GameManager.I.State.selectedPlayerName}: {string.Join(", ", currentSelection)}");
        ui.Show(PanelId.Player_Panel);
    }
}