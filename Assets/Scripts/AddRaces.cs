using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class AddRaces : MonoBehaviour
{
    public TMP_Dropdown dropdown;          // Assign in Inspector
    public GameObject buttonPrefab;        // Assign a prefab with a Button + TMP_Text
    public Transform buttonParent;         // Assign a UI container (like a VerticalLayoutGroup)

    private Dictionary<string, int> nameCounters = new Dictionary<string, int>();

    public void AddRace()
    {
        // Get selected option text
        string baseName = dropdown.options[dropdown.value].text;

        // Increase counter (start at 1)
        if (!nameCounters.ContainsKey(baseName))
        {
            nameCounters[baseName] = 1;
        }
        else
        {
            nameCounters[baseName]++;
        }

        // Build final name (always with number)
        string finalName = baseName + " " + nameCounters[baseName];

        // Instantiate button
        GameObject newButton = Instantiate(buttonPrefab, buttonParent);
        TMP_Text buttonText = newButton.GetComponentInChildren<TMP_Text>();
        buttonText.text = finalName;

        // Find child remove button
        Button removeBtn = newButton.transform.Find("RemoveRaceButton").GetComponent<Button>();

        // Hook up click to remove this button
        removeBtn.onClick.AddListener(() =>
        {
            Destroy(newButton);
        });

        Debug.Log("Created button: " + finalName);
    }
}