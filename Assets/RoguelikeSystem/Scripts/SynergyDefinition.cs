using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSynergy", menuName = "Roguelike/Augment Synergy")]
public class AugmentSynergyDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    [TextArea] public string description;
    [TextArea] public string flavorText;
    public Sprite icon;

    public bool hidesAugments = false;

    public List<string> requiredAugmentIds;

    private HashSet<string> GetAugmentsConsumedByOwnedSynergies()
    {
        HashSet<string> hiddenAugmentIds = new HashSet<string>();

        if (RunController.Instance == null)
        {
            return hiddenAugmentIds;
        }

        IReadOnlyList<AugmentSynergyDefinition> synergies = RunController.Instance.GetOwnedSynergies();
        if (synergies == null)
        {
            return hiddenAugmentIds;
        }

        foreach (AugmentSynergyDefinition synergy in synergies)
        {
            if (synergy == null || synergy.requiredAugmentIds == null || !synergy.hidesAugments)
            {
                continue;
            }

            foreach (string augmentId in synergy.requiredAugmentIds)
            {
                if (!string.IsNullOrEmpty(augmentId))
                {
                    hiddenAugmentIds.Add(augmentId);
                }
            }
        }

        return hiddenAugmentIds;
    }
}