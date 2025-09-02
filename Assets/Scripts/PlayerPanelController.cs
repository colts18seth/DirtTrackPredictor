using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPanelController : MonoBehaviour
{
    [SerializeField] private UIManager ui;
    [SerializeField] private Transform playerListContent;
    [SerializeField] private GameObject playerButtonPrefab;
    [SerializeField] private TMP_Text raceName;

    private void OnEnable() => Refresh();

    private void Refresh()
    {
        var race = GameManager.I.GetSelectedRace();
        if (race != null) raceName.text = race.displayName;

        foreach (Transform c in playerListContent) Destroy(c.gameObject);
        foreach (var p in GameManager.I.State.players)
        {
            var go = Instantiate(playerButtonPrefab, playerListContent);
            var label = go.GetComponentInChildren<TMP_Text>();
            if (label) label.text = p.name;
            var scoreText = go.transform.Find("Score_Text")?.GetComponent<TMP_Text>();
            if (scoreText != null)
            {
                if (!race.picksLocked)
                {
                    scoreText.text = GameManager.I.GetPlayerNightTotal(p.name, true).ToString();
                }
                else
                {
                    scoreText.text = race.GetPlayerPoints(p.name).ToString();
                    Button plrButton = go.GetComponent<Button>();
                    plrButton.interactable = false;
                }
            }
            // Find the PickedImage child  
            var pickedImage = go.transform.Find("PickedImage");
            if (pickedImage != null)
            {
                // Enable the image only if the player has made a pick for the current race  
                bool hasPicked = race != null && GameManager.I.HasPlayerPicked(race, p);
                pickedImage.gameObject.SetActive(hasPicked);
            }
            var btn = go.GetComponent<Button>();
            if (btn) btn.onClick.AddListener(() =>
            {
                GameManager.I.SelectPlayer(p.name);
                ui.Show(PanelId.Pick_Panel);
            });
        }
    }

    public void OnAddRaceResults()
    {
        if (GameManager.I.ArePicksLocked()) return;
     
        ui.Show(PanelId.RaceResults_Panel);
    }
    public void OnBack() => ui.Show(PanelId.Race_Panel);
}