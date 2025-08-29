using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform nightContent;
    [SerializeField] private Transform eventContent;
    [SerializeField] private GameObject rowPrefab;

    [Header("Filters (Optional)")]
    [SerializeField] private TMP_Dropdown nightDropdown; // If omitted, uses current night
    [SerializeField] private Toggle onlyScoredToggle;     // If omitted, defaults true

    private void OnEnable()
    {
        PopulateNightDropdownIfPresent();
        Refresh();
    }

    public void Refresh()
    {
        int nightIndex = ResolveNightIndex();
        bool onlyScored = onlyScoredToggle ? onlyScoredToggle.isOn : true;

        var nightTotals = GameManager.I.GetNightTotals(nightIndex, onlyScored);
        var eventTotals = GameManager.I.GetEventTotals(onlyScored);

        BuildList(nightContent, nightTotals);
        BuildList(eventContent, eventTotals);
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

        nightDropdown.value = GameManager.I.State.currentNightIndex;
        nightDropdown.onValueChanged.AddListener(_ => Refresh());
    }

    private void BuildList(Transform parent, List<PlayerScore> data)
    {
        if (!parent) return;

        foreach (Transform c in parent) Destroy(c.gameObject);

        foreach (var row in data)
        {
            var go = Instantiate(rowPrefab, parent);
            var texts = go.GetComponentsInChildren<TMP_Text>();
            // Expecting two texts: [0] Name, [1] Points
            if (texts.Length >= 2)
            {
                texts[0].text = row.playerName;
                texts[1].text = row.points.ToString();
            }
        }
    }
}