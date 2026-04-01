public class StatModifier
{
    public float BaseValue;
    public float FlatBonus;
    public float Multiplier = 1f;

    public float FinalValue => (BaseValue + FlatBonus) * Multiplier;
}