using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerListManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField nameInput;
    public Button addButton;
    public Transform listContainer;      // ScrollView -> Viewport -> Content
    public GameObject listItemPrefab;    // Prefab with a TextMeshProUGUI on it

    [Header("Rules")]
    public bool preventDuplicates = true;
    public bool caseInsensitiveNames = true;

    private List<string> players = new List<string>();
    private StringComparer comparer;

    [Serializable]
    private class PlayerListData { public List<string> players; }

    private void Awake()
    {
        comparer = caseInsensitiveNames ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

        addButton.onClick.AddListener(AddFromInput);

        // Submit on Enter
        // If your TMP version lacks onSubmit, hook AddFromInput to On End Edit in the Inspector.
        nameInput.onSubmit.AddListener(_ => AddFromInput());

        // Optional: Load previously saved players
        LoadPlayers();
    }

    private void AddFromInput()
    {
        var raw = nameInput.text ?? string.Empty;
        var name = raw.Trim();

        if (string.IsNullOrEmpty(name))
        {
            // Optional: give user feedback (shake, color, etc.)
            return;
        }

        if (preventDuplicates && Contains(players, name))
        {
            // Optional: feedback for duplicate
            nameInput.Select();
            nameInput.ActivateInputField();
            return;
        }

        players.Add(name);
        CreateRow(name);

        nameInput.text = string.Empty;
        nameInput.Select();
        nameInput.ActivateInputField();

        SavePlayers();
    }

    private bool Contains(List<string> list, string value)
    {
        foreach (var s in list)
            if (comparer.Equals(s, value)) return true;
        return false;
    }

    private void CreateRow(string playerName)
    {
        var go = Instantiate(listItemPrefab, listContainer);

        // Set name label
        var label = go.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null) label.text = playerName;

        // Find the remove button inside this instance
        var removeBtn = go.transform.Find("RemoveButton")?.GetComponent<Button>();
        if (removeBtn != null)
        {
            removeBtn.onClick.AddListener(() =>
            {
                RemovePlayer(playerName, go);
            });
        }
    }

    // If you ever need to rebuild the list from scratch
    private void RefreshList()
    {
        foreach (Transform child in listContainer)
            Destroy(child.gameObject);

        foreach (var p in players)
            CreateRow(p);
    }

    private void SavePlayers()
    {
        var data = new PlayerListData { players = players };
        var json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("players", json);
        PlayerPrefs.Save();
    }

    private void LoadPlayers()
    {
        if (!PlayerPrefs.HasKey("players")) return;
        var json = PlayerPrefs.GetString("players");
        var data = JsonUtility.FromJson<PlayerListData>(json);
        players = data?.players ?? new List<string>();
        RefreshList();
    }

    // Optional public API
    public IReadOnlyList<string> GetPlayers() => players;

    private void RemovePlayer(string name, GameObject rowGO)
    {
        // Remove from the list
        players.RemoveAll(p => comparer.Equals(p, name));

        // Destroy the row's GameObject
        Destroy(rowGO);

        // Save changes
        SavePlayers();
    }
}