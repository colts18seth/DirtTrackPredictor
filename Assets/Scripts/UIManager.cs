using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject[] canvases; // Assign all canvases in the Inspector

    private void DeactivateAll()
    {
        foreach (GameObject canvas in canvases)
        {
            canvas.SetActive(false);
        }
    }

    public void ShowPage(GameObject targetCanvas)
    {
        DeactivateAll();
        targetCanvas.SetActive(true);
    }
}