using UnityEngine;
using TMPro;

public class ResultRowUI : MonoBehaviour
{
    public TMP_Text positionText;
    public TMP_Text nameText;
    public TMP_Text pointsText;

    public void Init(int position, string playerName, int points)
    {
        positionText.text = position.ToString();
        nameText.text = playerName;
        pointsText.text = points.ToString();
    }
}