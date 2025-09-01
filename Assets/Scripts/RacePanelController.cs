using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RacePanelController : MonoBehaviour
{
    [SerializeField] private UIManager ui;
    [SerializeField] private TMP_Dropdown raceTypeDropdown;
    [SerializeField] private Transform raceListContent;
    [SerializeField] private GameObject raceButtonPrefab;
    [SerializeField] private TMP_Text raceNightText;
    [SerializeField] private Button AddRaceButton;
    [SerializeField] private ConfirmationDialog confirmDialog;

    private void OnEnable() => RefreshList();

    public void OnAddRace()
    {
        var type = (RaceType)raceTypeDropdown.value;
        var night = GameManager.I.State.nights[GameManager.I.State.currentNightIndex];

        // Count only races of this type
        int typeCount = 0;
        foreach (var r in night.races)
        {
            if (r.type == type)
                typeCount++;
        }

        string displayName = $"{type} {typeCount + 1}";

        GameManager.I.AddRaceToCurrentNight(type, displayName);
        GameManager.I.Save();
        RefreshList();
    }

    private void RefreshList()
    {
        var raceNightNumber = GameManager.I.State.currentNightIndex;
        raceNightText.text = $"Event Night {raceNightNumber + 1}";

        var currentNight = GameManager.I.State.nights[GameManager.I.State.currentNightIndex];
        AddRaceButton.interactable = !currentNight.finished;

        // Clear old list
        foreach (Transform c in raceListContent) Destroy(c.gameObject);

        // Track numbering per race type
        Dictionary<RaceType, int> typeCounters = new Dictionary<RaceType, int>();

        for (int i = 0; i < currentNight.races.Count; i++)
        {
            int raceIdx = i;
            var race = currentNight.races[i];

            // Increment counter for this race type
            if (!typeCounters.ContainsKey(race.type))
                typeCounters[race.type] = 0;
            typeCounters[race.type]++;

            // Generate the correct display name
            string displayName = $"{race.type} {typeCounters[race.type]}";

            // Update the actual race object so GameManager sees the new name
            race.displayName = displayName;

            var go = Instantiate(raceButtonPrefab, raceListContent);

            // Label
            var label = go.GetComponentInChildren<TMP_Text>();
            if (label) label.text = displayName;

            // Main select button
            var btn = go.GetComponent<Button>();
            if (btn) btn.onClick.AddListener(() =>
            {
                GameManager.I.SelectRace(raceIdx);
                ui.Show(PanelId.Player_Panel);
            });

            // Remove button wiring
            var removeBtn = go.transform.Find("RemoveButton")?.GetComponent<Button>();
            if (removeBtn != null)
            {
                removeBtn.onClick.AddListener(() =>
                {
                    confirmDialog.Show(
                        "Remove this race?",
                        onConfirm: () =>
                        {
                            currentNight.races.RemoveAt(raceIdx);
                            GameManager.I.Save();
                            RefreshList(); // Renumber after removal
                        },
                        onCancel: RefreshList
                    );
                });
            }
        }

        // Save after renaming so changes persist
        GameManager.I.Save();
    }

    public void OnBack() => ui.Show(PanelId.Event_Panel);

    public void OnNightResults() => Debug.Log("Night results placeholder.");

    public void OnClickConfirm()
    {
        confirmDialog.Show(
            "Are You Sure?",
            onConfirm: OnFinishNight,
            onCancel: RefreshList
        );
    }

    public void OnFinishNight()
    {
        var night = GameManager.I.State.nights[GameManager.I.State.currentNightIndex];
        night.finished = true;
        GameManager.I.Save();
        AddRaceButton.interactable = false;
        ui.Show(PanelId.Event_Panel);
    }
}