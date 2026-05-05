using System.Collections.Generic;
using UnityEngine;

public class EncyclopediaAugmentPage: MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private AugmentDatabase augmentDatabase;

    [Header("UI")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject augmentEntryPrefab;

    private void Start()
    {
        Rebuild();
    }

    public void Rebuild()
    {
        if (augmentDatabase == null || contentParent == null || augmentEntryPrefab == null)
        {
            return;
        }

        ClearEntries();

        List<AugmentDefinition> augments = new List<AugmentDefinition>(augmentDatabase.GetAll());

        augments.Sort((a, b) => string.Compare(a.displayName, b.displayName, System.StringComparison.OrdinalIgnoreCase));
        augments.Sort((a, b) =>
        {
            int rarityCompare = a.rarity.CompareTo(b.rarity);
            if (rarityCompare != 0)
            {
                return rarityCompare;
            }

            return string.Compare(a.displayName, b.displayName, System.StringComparison.OrdinalIgnoreCase);
        });

        foreach (AugmentDefinition augment in augments)
        {
            if (augment == null)
            {
                continue;
            }

            GameObject entryObj = Instantiate(augmentEntryPrefab, contentParent);
            EncyclopediaAugmentEntry entryUI = entryObj.GetComponent<EncyclopediaAugmentEntry>();

            if (entryUI != null)
            {
                entryUI.Setup(augment);
            }
        }
    }

    private void ClearEntries()
    {
        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }
    }
}