using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AugmentDatabase : MonoBehaviour
{
    [SerializeField] private List<AugmentDefinition> augments;

    private Dictionary<string, AugmentDefinition> byId;

    private void Awake()
    {
        byId = augments.ToDictionary(a => a.id, a => a);
    }   

    public AugmentDefinition GetById(string id)
        => byId.TryGetValue(id, out var aug) ? aug : null;

    public IReadOnlyList<AugmentDefinition> GetAll() => augments;
}