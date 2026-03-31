using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AugmentDatabase : MonoBehaviour
{
    [SerializeField] private List<AugmentDefinition> augments = new();

    private Dictionary<string, AugmentDefinition> byId;

    private void Awake()
    {
        byId = augments.ToDictionary(a => a.id, a => a);
    }

    public IReadOnlyList<AugmentDefinition> GetAll()
    {
        return augments;
    }

    public AugmentDefinition GetById(string id)
    {
        return byId.TryGetValue(id, out AugmentDefinition augment) ? augment : null;
    }
}