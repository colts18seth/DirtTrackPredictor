using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject setupCanvas;
    public GameObject racesCanvas;

    public void ShowRacesPage()
    {
        setupCanvas.SetActive(false);
        racesCanvas.SetActive(true);
    }

    public void ShowSetupPage()
    {
        setupCanvas.SetActive(true);
        racesCanvas.SetActive(false);
    }
}