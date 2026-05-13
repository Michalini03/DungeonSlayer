using UnityEngine;

public class BossSpellBeahvior : MonoBehaviour
{
    [SerializeField] private BossSpellBehaviorGX bossSpellBehaviorGX;
    [SerializeField] private BossBehavior bossBehavior;
    [SerializeField] private GameObject player;
    [SerializeField] private int attackSpellDamage = 50;
    [SerializeField] private string spellLayerName = "Spell";
    [SerializeField] private string playerLayerName = "Player";
    [SerializeField] private string enemyLayerName = "Enemy";

    private void Awake()
    {
        if (player == null)
        {
            player = GameObject.Find("Player");
        }

        ConfigureSpellCollisions();
    }

    private void ConfigureSpellCollisions()
    {
        // Nastavit vrstvu spell objektu
        int spellLayer = LayerMask.NameToLayer(spellLayerName);
        if (spellLayer == -1)
        {
            Debug.LogWarning($"Vrstva '{spellLayerName}' neexistuje. Spell zůstane na výchozí vrstvě.", this);
            return;
        }

        gameObject.layer = spellLayer;

        // Ignorovat kolize se vrstvou Player
        int playerLayer = LayerMask.NameToLayer(playerLayerName);
        if (playerLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(spellLayer, playerLayer, true);
        }

        // Ignorovat kolize se vrstvou Enemy
        int enemyLayer = LayerMask.NameToLayer(enemyLayerName);
        if (enemyLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(spellLayer, enemyLayer, true);
        }
    }

    public void DestroySpell()
    {
        Destroy(gameObject);
    }

    public void checkIfSpellHitsPlayer()
    {
        // Najdeme collider spell objektu
        Collider2D spellCollider = GetComponent<Collider2D>();
        if (spellCollider == null)
        {
            spellCollider = GetComponentInChildren<Collider2D>(true);
        }

        // Ověřit hráče
        if (player == null)
        {
            Debug.LogError("CHYBA: Objekt 'player' je prázdný! Skript ho nenašel.");
            return;
        }

        Collider2D playerCollider = player.GetComponent<Collider2D>();
        if (playerCollider == null)
        {
            playerCollider = player.GetComponentInChildren<Collider2D>(true);
        }

        // Diagnostika
        if (spellCollider == null)
        {
            Debug.LogError($"CHYBA: Spell '{name}' nemá Collider2D! Přidej ho na root nebo child.", this);
            return;
        }

        if (playerCollider == null)
        {
            Debug.LogError("CHYBA: Hráč nemá na sobě Collider2D!", this);
            return;
        }

        // AABB detekce zásahu (bounds intersection) - funguje bez fyzických kolizí
        if (spellCollider.bounds.Intersects(playerCollider.bounds))
        {
            player.GetComponent<PlayerCombat>().takeDamage(attackSpellDamage);
        }
    }
}
