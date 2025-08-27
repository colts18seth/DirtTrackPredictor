using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RacePanelController : MonoBehaviour
{
    [SerializeField] private UIManager ui;
    [SerializeField] private TMP_Dropdown raceTypeDropdown;
    [SerializeField] private Transform raceListContent;
    [SerializeField] private GameObject raceButtonPrefab;
    [SerializeField] private TMP_Text raceNightText;

    private void OnEnable() => RefreshList();

    public void OnAddRace()
    {
        var type = (RaceType)raceTypeDropdown.value;
        var night = GameManager.I.State.nights[GameManager.I.State.currentNightIndex];
        string displayName = $"{type} {night.races.Count + 1}";
        GameManager.I.AddRaceToCurrentNight(type, displayName);
        GameManager.I.Save();
        RefreshList();
    }

    private void RefreshList()
    {
        var raceNightNumber = GameManager.I.State.currentNightIndex;
        raceNightText.text = $"Event Night {raceNightNumber + 1}";

        foreach (Transform c in raceListContent) Destroy(c.gameObject);
        var night = GameManager.I.State.nights[GameManager.I.State.currentNightIndex];

        for (int i = 0; i < night.races.Count; i++)
        {
            int raceIdx = i;
            var race = night.races[i];
            var go = Instantiate(raceButtonPrefab, raceListContent);
            var label = go.GetComponentInChildren<TMP_Text>();
            if (label) label.text = race.displayName;
            var btn = go.GetComponent<Button>();
            if (btn) btn.onClick.AddListener(() =>
            {
                GameManager.I.SelectRace(raceIdx);
                ui.Show(PanelId.Player_Panel);
            });
        }
    }

    public void OnBack() => ui.Show(PanelId.Event_Panel);

    public void OnNightResults() => Debug.Log("Night results placeholder.");

    public void OnFinishNight()
    {
        var night = GameManager.I.State.nights[GameManager.I.State.currentNightIndex];
        night.finished = true;
        GameManager.I.Save();
        ui.Show(PanelId.Event_Panel);
    }
}