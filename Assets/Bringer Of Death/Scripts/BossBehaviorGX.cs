using UnityEngine;

public class BossBehaviorGX : MonoBehaviour
{
    [SerializeField]
    private BossBehavior bossBehavior;

    public void CastSpellParent()
    {
        bossBehavior.CastSpell();
    }
}
