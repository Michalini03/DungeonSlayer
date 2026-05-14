using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthBarUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject root;
    [SerializeField] private Image healthFill;
    [SerializeField] private TMP_Text bossNameText;

    [Header("Settings")]
    [SerializeField] private string bossDisplayName = "Boss";

    private int maxHealth = 1500;
    private int currentHealth = 1500;

    public void Initialize(string displayName, int startingHealth, int startingMaxHealth)
    {
        bossDisplayName = displayName;
        maxHealth = Mathf.Max(1, startingMaxHealth);
        currentHealth = Mathf.Clamp(startingHealth, 0, maxHealth);

        if (root != null)
        {
            root.SetActive(true);
        }

        Refresh();
    }

    public void SetHealth(int newHealth, int newMaxHealth)
    {
        maxHealth = Mathf.Max(1, newMaxHealth);
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        Refresh();
    }

    public void Hide()
    {
        if (root != null)
        {
            root.SetActive(false);
        }
    }

    private void Refresh()
    {
        if (bossNameText != null)
        {
            bossNameText.text = bossDisplayName;
        }

        if (healthFill != null)
        {
            healthFill.fillAmount = currentHealth / (float)maxHealth;
        }
    }
}