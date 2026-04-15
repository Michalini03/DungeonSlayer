using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("Health")]
    public Image healthFill;
    public TMP_Text healthText;
    public RectTransform healthBarRoot;

    [Header("Stamina")]
    public Image staminaFill;
    public TMP_Text staminaText;
    public RectTransform staminaBarRoot;

    [Header("Base Stats & Sizes")]
    public float baseMaxHealth = 120f;
    public float baseHealthWidth = 100f;
    public float baseMaxStamina = 100f;
    public float baseStaminaWidth = 100f;

    public AttributesController aController;

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
        if (healthFill == null || maxHealth <= 0f)
            return;

        healthFill.fillAmount = currentHealth / (float)maxHealth;

        if (healthBarRoot != null)
        {
            float newWidth = (maxHealth / baseMaxHealth) * baseHealthWidth;
            healthBarRoot.sizeDelta = new Vector2(newWidth, healthBarRoot.sizeDelta.y);
        }

        if (healthText != null)
            healthText.text = $"{Mathf.RoundToInt(currentHealth)} / {Mathf.RoundToInt(maxHealth)}";
    }

    public void UpdateStaminaBar(int currentStamina, int maxStamina)
    {
        if (staminaFill == null || maxStamina <= 0f)
            return;

        staminaFill.fillAmount = currentStamina / (float)maxStamina;

        if (staminaBarRoot != null)
        {
            float newWidth = (maxStamina / baseMaxStamina) * baseStaminaWidth;
            staminaBarRoot.sizeDelta = new Vector2(newWidth, staminaBarRoot.sizeDelta.y);
        }

        if (staminaText != null)
            staminaText.text = $"{Mathf.RoundToInt(currentStamina)} / {Mathf.RoundToInt(maxStamina)}";
    }
}