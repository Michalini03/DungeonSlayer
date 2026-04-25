using UnityEngine;

public class BossBehavior : MonoBehaviour
{
    Rigidbody2D rb;
    [SerializeField] private GameObject spellPrefab;
    [SerializeField] private GameObject player;
    private PlayerMovement playerMovement;
    [SerializeField] private Vector2 castSpellOffset;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = player.GetComponent<PlayerMovement>();
    }

    void Update()
    {
        
    }

    public void CastSpell()
    {

        // 1. Kontrola, zda máme přiřazený prefab
        if (spellPrefab == null)
        {
            Debug.LogError("Chybí spellPrefab v BossBehavior!");
            return;
        }

        // 2. Kontrola, zda hráč stále žije/existuje
        if (player != null)
        {   
            if (playerMovement.IsInAir())
            {   
                return;
            }
            Instantiate(spellPrefab, player.transform.position + (Vector3)castSpellOffset, Quaternion.identity);
        }

        
        else
        {
            Debug.LogWarning("Boss se pokusil kouzlit, ale hráč už neexistuje (asi je po smrti).");
        }
    }
}
