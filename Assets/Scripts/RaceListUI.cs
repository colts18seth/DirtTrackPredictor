using UnityEngine;
using UnityEngine.UI;
using TMPro;   // if using TextMeshPro

public class RaceListUI : MonoBehaviour
{
    public GameObject raceButtonPrefab;   // assign prefab in Inspector
    public Transform raceListParent;      // the panel with Vertical Layout Group
    //public int numberOfRaces = 0;         // set from setup screen
    public TMP_InputField inputField; // assign in Inspector
    private int numberValue;

    public void GenerateRaceButtons()
    {
        Debug.Log("Generating Buttons");
        // clear old buttons if needed
        foreach (Transform child in raceListParent)
        {
            Destroy(child.gameObject);
        }

        if (int.TryParse(inputField.text, out numberValue))
        {
            // create new buttons
            for (int i = 1; i <= numberValue; i++)
            {
                GameObject newButton = Instantiate(raceButtonPrefab, raceListParent);

                // set button text
                TMP_Text label = newButton.GetComponentInChildren<TMP_Text>();
                if (label != null)
                {
                    label.text = "Race Night - " + i;
                    //label.color = Color.white;
                }

                // add click listener
                int raceIndex = i;  // important: capture loop variable
                newButton.GetComponent<Button>().onClick.AddListener(() => OnRaceSelected(raceIndex));
            }
        }
        else
        {
            Debug.LogWarning("Invalid number entered!");
        }
    }

    void OnRaceSelected(int raceNumber)
    {
        Debug.Log("Selected Race " + raceNumber);
        // TODO: switch to pick screen, pass along raceNumber
    }
}