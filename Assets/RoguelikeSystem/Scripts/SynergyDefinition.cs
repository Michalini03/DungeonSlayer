using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSynergy", menuName = "Roguelike/Augment Synergy")]
public class AugmentSynergyDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    [TextArea] public string description;
    [TextArea] public string flavorText;

    public List<string> requiredAugmentIds;
}