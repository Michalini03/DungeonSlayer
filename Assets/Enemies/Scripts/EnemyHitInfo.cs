using UnityEngine;

public class EnemyHitInfo : MonoBehaviour
{
    public void manageEnemyHit(int playerDamage)
    {
        if (TryGetComponent<EnemyMovement>(out var skeletonBehavior))
        {
            skeletonBehavior.manageEnemyHit(playerDamage);
            return;
        }

        if (TryGetComponent<FlyingEyeBehavior>(out var flyingEyeBehavior))
        {
            flyingEyeBehavior.manageEnemyHit(playerDamage);
            return;
        }
        if (TryGetComponent<RatBehavior>(out var ratBehavior))
        {
            ratBehavior.manageEnemyHit(playerDamage);
            return;
        }
        if (TryGetComponent<BossBehavior>(out var bossBehavior))
        {
            bossBehavior.manageEnemyHit(playerDamage);
            return;
        }
        else
        {
            Debug.LogWarning("Neexistuje hledaná komponenta pro správu zásahu nepřítele.");
        }
    }
}
