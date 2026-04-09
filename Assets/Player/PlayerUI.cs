using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{

    [Header("Bar Fills")]
    public Image healthFill;
    public Image staminaFill;

    

    [Header("Base Stats & Sizes")]
    public float baseMaxHealth = 100f;
    public float baseHealthWidth = 100f; 

    public float baseMaxStamina = 100f;
    public float baseStaminaWidth = 100f;


    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        healthFill.fillAmount = currentHealth / maxHealth;

        //update the width of fill when max health changes
        float newWidth = (maxHealth / baseMaxHealth) * baseHealthWidth;
        healthFill.rectTransform.sizeDelta = new Vector2(newWidth, healthFill.rectTransform.sizeDelta.y);
    }

    public void UpdateStaminaBar(float currentStamina, float maxStamina)
    {
        staminaFill.fillAmount = currentStamina / maxStamina;

        //update the width of fill when max stamina changes
        float newWidth = (maxStamina / baseMaxStamina) * baseStaminaWidth;
        staminaFill.rectTransform.sizeDelta = new Vector2(newWidth, staminaFill.rectTransform.sizeDelta.y);
    }
}
