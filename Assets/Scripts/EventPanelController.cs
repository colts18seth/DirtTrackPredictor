using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventPanelController : MonoBehaviour
{
    [SerializeField] private UIManager ui;
    [SerializeField] private TMP_Text title;
    [SerializeField] private Transform nightListContent;
    [SerializeField] private GameObject nightButtonPrefab;

    private void OnEnable() => Refresh();

    private void Refresh()
    {
        foreach (Transform c in nightListContent) Destroy(c.gameObject);
        var s = GameManager.I.State;
        if (title) title.text = s.eventName;

        for (int i = 0; i < s.raceNightCount; i++)
        {
            int nightIdx = i; // capture
            var go = Object.Instantiate(nightButtonPrefab, nightListContent);
            var label = go.GetComponentInChildren<TMP_Text>();
            if (label) label.text = $"Event Night {i + 1}";
            var btn = go.GetComponent<Button>();
            if (btn) btn.onClick.AddListener(() =>
            {
                GameManager.I.SetCurrentNight(nightIdx);
                ui.Show(PanelId.Race_Panel);
            });
        }
    }

    public void OnClickFinishEvent()
    {
        // Save current state
        GameManager.I.Save();

        // Get sorted totals from GameManager
        var sortedTotals = GameManager.I.GetEventTotalsSorted();

        // Show the podium panel
        ui.Show(PanelId.Podium_Panel);
    }


    public void OnClickLeaderboard()
    {
        ui.Show(PanelId.LeaderBoard_Panel);
    }

}