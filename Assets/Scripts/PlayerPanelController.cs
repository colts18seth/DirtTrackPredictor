using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPanelController : MonoBehaviour
{
    [SerializeField] private UIManager ui;
    [SerializeField] private Transform playerListContent;
    [SerializeField] private GameObject playerButtonPrefab;

    private void OnEnable() => Refresh();

    private void Refresh()
    {
        foreach (Transform c in playerListContent) Destroy(c.gameObject);
        foreach (var p in GameManager.I.State.players)
        {
            var go = Instantiate(playerButtonPrefab, playerListContent);
            var label = go.GetComponentInChildren<TMP_Text>();
            if (label) label.text = p.name;
            var btn = go.GetComponent<Button>();
            if (btn) btn.onClick.AddListener(() =>
            {
                GameManager.I.SelectPlayer(p.name);
                ui.Show(PanelId.Pick_Panel);
            });
        }
    }

    public void OnBack() => ui.Show(PanelId.Race_Panel);
}