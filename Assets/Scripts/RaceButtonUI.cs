using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RaceButtonUI : MonoBehaviour
{
    public TMP_Text raceNameText;
    private int nightIndex;
    private int raceIndex;

    public void Init(int nightIdx, int raceIdx, RaceData raceData)
    {
        nightIndex = nightIdx;
        raceIndex = raceIdx;
        raceNameText.text = raceData.raceName;
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        RacePickUI.Instance.LoadRace(nightIndex, raceIndex);
    }
}