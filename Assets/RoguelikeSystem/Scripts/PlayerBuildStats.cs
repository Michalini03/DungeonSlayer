public class PlayerBuildStats
{
    public StatModifier maxHealth = new();
    public StatModifier damage = new();
    public StatModifier healthRegen = new();
    public StatModifier lives = new();
    public StatModifier damageReduction = new();

    public StatModifier attackRange = new();
    public StatModifier knockbackForce = new();

    public StatModifier maxStamina = new();
    public int[] comboBar;

    public StatModifier iframesDuration = new();
    public StatModifier movementSpeed = new();
}