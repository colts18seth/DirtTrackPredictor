using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NightButtonUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text nightLabel; // e.g., "Night 1"

    private int nightIndex;

    /// <summary>
    /// Called by NightSelectionUI right after instantiation
    /// </summary>
    public void Init(int index)
    {
        nightIndex = index;

        if (nightLabel != null)
            nightLabel.text = $"Night {index + 1}";

        // Ensure we only add the click once
        var btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        // Tell the UIManager to load the race list for this night
        UIManager.Instance.OnNightSelected(nightIndex);
    }
}