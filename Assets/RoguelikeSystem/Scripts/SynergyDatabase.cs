using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SynergyDatabase : MonoBehaviour
{
    [SerializeField] private List<AugmentSynergyDefinition> synergies = new();

    private Dictionary<string, AugmentSynergyDefinition> byId;

    private void Awake()
    {
        byId = synergies.ToDictionary(s => s.id, s => s);
    }

    public AugmentSynergyDefinition GetById(string id)
    {
        return byId.TryGetValue(id, out var synergy) ? synergy : null;
    }

    public IReadOnlyList<AugmentSynergyDefinition> GetAll()
    {
        return synergies;
    }
}