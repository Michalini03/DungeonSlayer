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

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthFill == null || maxHealth <= 0f)
            return;

        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        float ratio = currentHealth / maxHealth;
        healthFill.fillAmount = ratio;

        if (healthBarRoot != null)
        {
            float newWidth = (maxHealth / baseMaxHealth) * baseHealthWidth;
            healthBarRoot.sizeDelta = new Vector2(newWidth, healthBarRoot.sizeDelta.y);
        }

        if (healthText != null)
            healthText.text = $"{Mathf.RoundToInt(currentHealth)} / {Mathf.RoundToInt(maxHealth)}";
    }

    public void UpdateStaminaBar(float currentStamina, float maxStamina)
    {
        if (staminaFill == null || maxStamina <= 0f)
            return;

        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

        float ratio = currentStamina / maxStamina;
        staminaFill.fillAmount = ratio;

        if (staminaBarRoot != null)
        {
            float newWidth = (maxStamina / baseMaxStamina) * baseStaminaWidth;
            staminaBarRoot.sizeDelta = new Vector2(newWidth, staminaBarRoot.sizeDelta.y);
        }

        if (staminaText != null)
            staminaText.text = $"{Mathf.RoundToInt(currentStamina)} / {Mathf.RoundToInt(maxStamina)}";
    }
}