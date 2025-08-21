using UnityEngine;

[CreateAssetMenu(fileName = "RaceData", menuName = "Scriptable Objects/RaceData")]
public class RaceData : ScriptableObject
{
    public string raceName;
    public int nightIndex;          // The Night this race belongs to
    public int raceIndex;           // Position within the Night
    public Sprite raceImage;        // Optional thumbnail / track image
    public string[] carNames;       // Competitors
    public int winningCarIndex = -1; // -1 means no result yet
}

