using UnityEngine;

[CreateAssetMenu(menuName = "Racing/Scoring Settings", fileName = "ScoringSettings")]
public class ScoringSettings : ScriptableObject
{
    [Header("Base podium points (Single Pick)")]
    public int winPoints = 10;
    public int placePoints = 6;
    public int showPoints = 3;

    [Header("Top3 mode")]
    [Tooltip("Award for being in top 3 (even if not exact place)")]
    public int top3BasePoints = 3;
    public int top3ExactPlaceBonus1 = 5;
    public int top3ExactPlaceBonus2 = 3;
    public int top3ExactPlaceBonus3 = 2;

    [Header("Underdog multiplier (exponential)")]
    [Tooltip("Higher K increases reward for deeper starting positions")]
    public float underdogK = 0.12f;
    [Tooltip("Clamp multiplier to avoid extremes (1 = no cap if <= 0)")]
    public float underdogMax = 2.5f;

    [Header("Invert handling")]
    [Tooltip("Use qualifying rank within inverted block for difficulty math")]
    public bool useEffectiveStartForInvert = true;

    [Tooltip("x: 0=fastest in invert, 1=slowest; y: weight applied to underdog multiplier")]
    public AnimationCurve invertQualRankWeight = AnimationCurve.Linear(0f, 0.8f, 1f, 1.1f);

    [Tooltip("Extra multiplicative boost for the polesitter when invert > 1 (per car)")]
    public float poleBoostPerInvert = 0.02f;

    [Tooltip("Final cap after invert adjustments (0 or less = no cap)")]
    public float invertAdjustedMax = 2.5f;

    [Header("Qualifying bonus")]
    public int qualifyingExactBonus = 5;

    private void OnValidate()
    {
        winPoints = Mathf.Max(0, winPoints);
        placePoints = Mathf.Max(0, placePoints);
        showPoints = Mathf.Max(0, showPoints);
        top3BasePoints = Mathf.Max(0, top3BasePoints);
        top3ExactPlaceBonus1 = Mathf.Max(0, top3ExactPlaceBonus1);
        top3ExactPlaceBonus2 = Mathf.Max(0, top3ExactPlaceBonus2);
        top3ExactPlaceBonus3 = Mathf.Max(0, top3ExactPlaceBonus3);
        underdogK = Mathf.Max(0f, underdogK);
        underdogMax = Mathf.Max(0f, underdogMax);
        invertAdjustedMax = Mathf.Max(0f, invertAdjustedMax);
        poleBoostPerInvert = Mathf.Max(0f, poleBoostPerInvert);
    }
}