using UnityEngine;

public class BossBehaviorGX : MonoBehaviour
{
    [SerializeField] private BossBehavior bossBehavior;
    [SerializeField] public Animator animator;

    public void CastSpellParent()
    {
        bossBehavior.CastSpell();
    }

    private void checkAndDamagePlayerParent()
    {
        SoundManager.PlaySound(SoundType.BOSS_ATTACK);
        bossBehavior.checkAndDamagePlayer();
    }

    private void DestroyBossAndAllEnemiesParent()
    {
        // bossBehavior.DestroyBossAndAllEnemies();
    }
}
