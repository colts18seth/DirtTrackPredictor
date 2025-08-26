using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SetupPanelController : MonoBehaviour
{
    [SerializeField] private UIManager ui;
    [SerializeField] private TMP_InputField eventNameInput;
    [SerializeField] private TMP_InputField nightsInput;
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private Transform playerListContent;
    [SerializeField] private GameObject playerButtonPrefab; // Prefab_Button_Player
    [SerializeField] private TMP_Dropdown modeDropdown;     // SinglePick / Top3
    [SerializeField] private Button nextButton;

    private readonly List<string> players = new();

    private void OnEnable() => ValidateNext();
    public void OnPlayerAdd()
    {
        var name = playerNameInput.text.Trim();
        if (string.IsNullOrEmpty(name)) return;
        if (players.Contains(name)) return;
        players.Add(name);
        playerNameInput.text = "";
        AddPlayerRow(name);
        ValidateNext();
    }

    private void AddPlayerRow(string name)
    {
        var go = Instantiate(playerButtonPrefab, playerListContent);
        var label = go.GetComponentInChildren<TMP_Text>();
        if (label) label.text = name;
        // Find the remove button inside this instance
        var removeBtn = go.transform.Find("Remove_Button")?.GetComponent<Button>();
        if (removeBtn)
        {
            removeBtn.onClick.AddListener(() =>
            {
                RemovePlayer(name, go);
            });
        }

    }

    public void OnNext()
    {
        if (!int.TryParse(nightsInput.text, out var nights) || nights <= 0) return;
        var eventName = eventNameInput.text.Trim();
        var mode = (GameMode)modeDropdown.value;

        GameManager.I.CreateNewEvent(eventName, nights, players, mode);
        GameManager.I.Save();

        ui.Show(PanelId.Event_Panel);
    }

    private void RemovePlayer(string name, GameObject row)
    {
        players.Remove(name);
        Destroy(row);
        ValidateNext();
    }


    public void ValidateNext()
    {
        bool okName = !string.IsNullOrWhiteSpace(eventNameInput.text);
        bool okNights = int.TryParse(nightsInput.text, out var nights) && nights > 0;
        bool okPlayers = players.Count > 0;
        nextButton.interactable = okName && okNights && okPlayers;
    }
}