using UnityEngine;

public class AttributesController : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth = 100;
    public int Damage = 50;
    public int healthRegen = 0;
    public int Lives = 0;
    public float damageReduction = 0f; // Percentage (e.g., 0.2 for 20% reduction)

    public float AttackRange = 0.5f;
    public float KnockbackForce = 10f;
    
    public int maxStamina = 100;
    public int[] staminaBar = new int[3] { 6,3,1 }; // split bar into 3 sections, 3,1.5,1,1.5,3 ; for now, can be changed       
    
    public float iframesDuration = 0.5f;
    public int movementSpeed = 10;
    



    public bool canComboAttack3 = false;
    public bool canAirComboAttack2 = false;


    

    



}


