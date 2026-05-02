using UnityEngine;

public class BossBehaviorGX : MonoBehaviour
{
    [SerializeField]
    private BossBehavior bossBehavior;
    public Animator animator;

    public void CastSpellParent()
    {
        bossBehavior.CastSpell();
    }
}
