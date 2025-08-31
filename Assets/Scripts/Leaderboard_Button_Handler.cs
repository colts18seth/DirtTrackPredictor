using UnityEngine;

public class LeaderboardButtonHandler : MonoBehaviour
{
    [Header("Assign your panels here")]
    [SerializeField] private GameObject eventPanel;       // The panel you’re leaving
    [SerializeField] private GameObject leaderboardPanel; // The panel you’re showing

    /// <summary>
    /// Call this from the Button's OnClick event in the Inspector.
    /// </summary>
    public void ShowLeaderboard()
    {
        if (eventPanel != null)
            eventPanel.SetActive(false);

        if (leaderboardPanel != null)
            leaderboardPanel.SetActive(true);
    }
}
