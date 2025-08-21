using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameManager;

public class SetupUI : MonoBehaviour
{
    [Header("Inputs")]
    public TMP_InputField totalNightsInput;
    public Transform playerNameContainer; // Parent holding player name input fields
    public TMP_Dropdown gameModeDropdown; // 0 = SinglePick, 1 = TopThree

    [Header("Prefabs")]
    public TMP_InputField playerNameFieldPrefab;

    private List<TMP_InputField> playerNameFields = new();

    public void AddPlayerField()
    {
        var field = Instantiate(playerNameFieldPrefab, playerNameContainer);
        playerNameFields.Add(field);
    }

    public void OnConfirmSetup()
    {
        int totalNights = int.Parse(totalNightsInput.text);
        List<string> names = new();
        foreach (var field in playerNameFields)
        {
            if (!string.IsNullOrWhiteSpace(field.text))
                names.Add(field.text.Trim());
        }

        GameMode mode = (GameMode)gameModeDropdown.value;

        // Initialize data & go to next UI
        GameSessionData.Instance.InitializeSession(totalNights, names, mode);
        UIManager.Instance.OnSetupComplete(totalNights, names.ToArray(), mode);
    }
}