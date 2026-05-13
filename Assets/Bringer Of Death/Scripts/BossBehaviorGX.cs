using UnityEngine;

public class BossBehaviorGX : MonoBehaviour
{
    [SerializeField] private BossBehavior bossBehavior;
    [SerializeField] public Animator animator;

    public void CastSpellParent()
    {
        bossBehavior.CastSpell();
    }
}
