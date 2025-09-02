using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum PanelId { MainMenu_Panel, Setup_Panel, Event_Panel, Race_Panel, Player_Panel, Pick_Panel, RaceResults_Panel, LeaderBoard_Panel, Podium_Panel }

public class UIManager : MonoBehaviour
{
    [System.Serializable]
    public class PanelEntry { public PanelId id; public GameObject root_Canvas; }

    [SerializeField] private List<PanelEntry> panels = new();
    [SerializeField] private PanelId startPanel = PanelId.MainMenu_Panel;

    private Dictionary<PanelId, GameObject> map;
    private readonly Stack<PanelId> history = new();
    public PanelId Current { get; private set; }

    private void Awake()
    {
        map = panels.Where(p => p.root_Canvas != null)
                    .GroupBy(p => p.id)
                    .ToDictionary(g => g.Key, g => g.First().root_Canvas);
        foreach (var go in map.Values)
        {
            go.SetActive(false);
            //Debug.Log($"map.value: {go}");
        }
    }

    private void Start() => Show(startPanel, false);

    public void Show(PanelId id) => Show(id, true);

    public void Back()
    {
        if (history.Count == 0) return;
        var prev = history.Pop();
        Show(prev, false);
    }

    private void Show(PanelId id, bool pushHistory)
    {
        //Debug.Log($"UIManager: Showing Current {Current}");
        //Debug.Log($"UIManager: Showing panel {id}");
        //if (map.ContainsKey(id))
        //    Debug.Log($"Found panel GameObject: {map[id].name}, active: {map[id].activeSelf}");
        //else
        //    Debug.LogWarning($"PanelId {id} not mapped in UIManager");

        if (!map.ContainsKey(id)) { Debug.LogWarning($"No panel for {id}"); return; }

        if (pushHistory && map.ContainsKey(Current))
            history.Push(Current);

        if (map.TryGetValue(Current, out var currentGo))
            currentGo.SetActive(false);

        map[id].SetActive(true);
        Current = id;
    }
}