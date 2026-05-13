using UnityEngine;

public class BossSpellBehaviorGX : MonoBehaviour
{
    [SerializeField]
    private BossSpellBeahvior BossSpellBehavior;

    public void DestroySpellParent()
    {
        BossSpellBehavior.DestroySpell();
    }

    public void checkIfSpellHitsPlayerParent()
    {
        BossSpellBehavior.checkIfSpellHitsPlayer();
    }
}
