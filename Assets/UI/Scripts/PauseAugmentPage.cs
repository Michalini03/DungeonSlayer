using System.Collections.Generic;
using UnityEngine;

public class PauseAugmentPage : MonoBehaviour
{
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject entryPrefab;

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

    private void ClearEntries()
    {
        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }
    }
}