using System.Collections.Generic;
using UnityEngine;

public class PauseAugmentPage : MonoBehaviour
{
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject entryPrefab;
    [SerializeField] private GameObject synergyEntryPrefab;

    private void OnEnable()
    {
        Rebuild();
    }

    public void Rebuild()
    {
        ClearEntries();

        if (RunController.Instance == null || contentParent == null || entryPrefab == null)
        {
            return;
        }

        BuildOwnedAugments();
        BuildOwnedSynergies();
    }

    private void BuildOwnedAugments()
    {
        HashSet<string> hiddenAugmentIds = GetAugmentsConsumedByOwnedSynergies();

        List<KeyValuePair<string, int>> owned = new List<KeyValuePair<string, int>>(RunController.Instance.GetOwnedAugments());

        owned.Sort((a, b) =>
        {
            AugmentDefinition aDef = RunController.Instance.GetAugmentDefinition(a.Key);
            AugmentDefinition bDef = RunController.Instance.GetAugmentDefinition(b.Key);

            string aName = aDef != null ? aDef.displayName : a.Key;
            string bName = bDef != null ? bDef.displayName : b.Key;

            return string.Compare(aName, bName, System.StringComparison.OrdinalIgnoreCase);
        });

        foreach (KeyValuePair<string, int> kv in owned)
        {
            if (hiddenAugmentIds.Contains(kv.Key))
            {
                continue;
            }

            AugmentDefinition augment = RunController.Instance.GetAugmentDefinition(kv.Key);
            if (augment == null)
            {
                continue;
            }

            GameObject entryObj = Instantiate(entryPrefab, contentParent);
            PauseAugmentEntry entryUI = entryObj.GetComponent<PauseAugmentEntry>();

            if (entryUI != null)
            {
                entryUI.Setup(augment, kv.Value);
            }
        }
    }

    private void BuildOwnedSynergies()
    {
        IReadOnlyList<AugmentSynergyDefinition> synergies = RunController.Instance.GetOwnedSynergies();
        if (synergies == null || synergies.Count == 0)
        {
            return;
        }

        List<AugmentSynergyDefinition> sorted = new List<AugmentSynergyDefinition>(synergies);
        sorted.Sort((a, b) =>
        {
            string aName = a != null ? a.displayName : "";
            string bName = b != null ? b.displayName : "";
            return string.Compare(aName, bName, System.StringComparison.OrdinalIgnoreCase);
        });

        GameObject prefabToUse = synergyEntryPrefab != null ? synergyEntryPrefab : entryPrefab;

        foreach (AugmentSynergyDefinition synergy in sorted)
        {
            if (synergy == null)
            {
                continue;
            }

            GameObject entryObj = Instantiate(prefabToUse, contentParent);

            PauseAugmentEntry synergyUI = entryObj.GetComponent<PauseAugmentEntry>();
            if (synergyUI != null)
            {
                synergyUI.SetupSynergy(synergy);
            }
        }
    }

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

    private void ClearEntries()
    {
        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }
    }
}