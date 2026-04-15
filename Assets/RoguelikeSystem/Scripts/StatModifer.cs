using UnityEngine;

public class StatModifier
{
    public float BaseValue;
    public float FlatBonus;
    public float Multiplier = 1f;

    public float FinalValue => (BaseValue + FlatBonus) * Multiplier;

    public void ApplyMinMultiplier(float newMultiplier)
    {
        Multiplier = Mathf.Min(Multiplier, newMultiplier);
    }

    public void ApplyMaxMultiplier(float newMultiplier)
    {
        Multiplier = Mathf.Max(Multiplier, newMultiplier);
    }
}