using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private UIManager ui;

    public void OnClickStartNew()
    {
        ui.Show(PanelId.Setup_Panel);
    }

    public void OnClickResume()
    {
        if (GameManager.I.Load())
            ui.Show(PanelId.Event_Panel);
        else
            Debug.Log("No saved event to resume.");
    }
}