using UnityEngine;

public class NightSelectionUI : MonoBehaviour
{
    public static NightSelectionUI Instance;
    public Transform container;
    public NightButtonUI nightButtonPrefab;

    private void Awake() => Instance = this;

    public void BuildNightButtons(int count)
    {
        foreach (Transform child in container)
            Destroy(child.gameObject);

        for (int i = 0; i < count; i++)
        {
            var btn = Instantiate(nightButtonPrefab, container);
            btn.Init(i);
        }
    }
}