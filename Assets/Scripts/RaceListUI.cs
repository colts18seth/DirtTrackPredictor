using UnityEngine;
using static GameManager;

public class RaceListUI : MonoBehaviour
{
    public static RaceListUI Instance;
    public Transform container;
    public RaceButtonUI raceButtonPrefab;

    private void Awake() => Instance = this;

    public void LoadNight(int nightIndex)
    {
        foreach (Transform child in container)
            Destroy(child.gameObject);

        var night = GameSessionData.Instance.nights[nightIndex];
        for (int r = 0; r < night.races.Count; r++)
        {
            var btn = Instantiate(raceButtonPrefab, container);
            btn.Init(nightIndex, r, night.races[r]);
        }
    }
}