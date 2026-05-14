using UnityEngine;

public class EnemyHitInfo : MonoBehaviour
{
    public void manageEnemyHit(int playerDamage, Vector2 sourcePosition, float knockbackForce)
    {
        if (TryGetComponent<EnemyMovement>(out var skeletonBehavior))
        {
            skeletonBehavior.manageEnemyHit(playerDamage, sourcePosition, knockbackForce);
            return;
        }

        if (TryGetComponent<FlyingEyeBehavior>(out var flyingEyeBehavior))
        {
            flyingEyeBehavior.manageEnemyHit(playerDamage, sourcePosition, knockbackForce);
            return;
        }
        if (TryGetComponent<RatBehavior>(out var ratBehavior))
        {
            ratBehavior.manageEnemyHit(playerDamage, sourcePosition, knockbackForce);
            return;
        }
        if (TryGetComponent<BossBehavior>(out var bossBehavior))
        {
            bossBehavior.manageEnemyHit(playerDamage, sourcePosition, knockbackForce);
            return;
        }
        else
        {
            Debug.LogWarning("Neexistuje hledaná komponenta pro správu zásahu nepřítele.");
        }
    }
}
