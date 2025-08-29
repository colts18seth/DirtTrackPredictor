using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardRowView : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text pointsText;
    [SerializeField] private Button button;

    private string _playerName;
    private Action<string> _onClicked;

    public void Set(string playerName, int points, Action<string> onClicked)
    {
        _playerName = playerName;
        _onClicked = onClicked;

        if (nameText) nameText.text = playerName;
        if (pointsText) pointsText.text = points.ToString();

        if (button)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => _onClicked?.Invoke(_playerName));
        }
    }
}