using UnityEngine;
using System.Collections.Generic;

public enum AugmentRarity
{
    Common,
    Uncommon,
    Rare,
    Legendary
}

public enum AugmentEffectType
{
    Passive,
    Instant
}

[CreateAssetMenu(menuName = "Roguelike/Augment")]
public class AugmentDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    [TextArea] public string description;
    [TextArea] public string flavorText;
    public Sprite icon;

    public AugmentRarity rarity;
    public int baseWeight = 100;
    public int maxStacks = 1;

    public AugmentEffectType effectType = AugmentEffectType.Passive;

    public List<string> tags = new();
    public List<string> excludes = new();
}