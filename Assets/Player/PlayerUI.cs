using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public AttributesController aController;

    [Header("Bar Fills")]
    public Image healthFill;
    public Image staminaFill;

    

    [Header("Base Stats & Sizes")]
    public float baseMaxHealth = 100f;
    public float baseHealthWidth = 100f; 

    public float baseMaxStamina = 100f;
    public float baseStaminaWidth = 100f;

    private void OnEnable()
    {
        aController.OnHealthChange += UpdateHealthBar;
        aController.OnStaminaChange += UpdateStaminaBar;
    }

    private void OnDisable()
    {
        aController.OnHealthChange -= UpdateHealthBar;
        aController.OnStaminaChange -= UpdateStaminaBar;
    }


    public void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        healthFill.fillAmount = currentHealth / (float)maxHealth;

        //update the width of fill when max health changes
        float newWidth = (maxHealth / baseMaxHealth) * baseHealthWidth;
        healthFill.rectTransform.sizeDelta = new Vector2(newWidth, healthFill.rectTransform.sizeDelta.y);
    }

    public void UpdateStaminaBar(int currentStamina, int maxStamina)
    {
        staminaFill.fillAmount = currentStamina / (float)maxStamina;

        //update the width of fill when max stamina changes
        float newWidth = (maxStamina / baseMaxStamina) * baseStaminaWidth;
        staminaFill.rectTransform.sizeDelta = new Vector2(newWidth, staminaFill.rectTransform.sizeDelta.y);
    }
}
