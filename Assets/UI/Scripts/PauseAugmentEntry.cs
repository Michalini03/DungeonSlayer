using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PauseAugmentEntry : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text flavorText;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private TMP_Text stackText;

    public void Setup(AugmentDefinition augment, int stacks)
    {
        if (augment == null)
        {
            return;
        }

        if (iconImage != null)
        {
            iconImage.sprite = augment.icon;
            iconImage.enabled = augment.icon != null;
        }

        if (titleText != null)
        {
            titleText.text = augment.displayName;
        }

        if (descriptionText != null)
        {
            descriptionText.text = augment.description;
        }

        if (flavorText != null)
        {
            flavorText.text = augment.flavorText;
        }

        if (rarityText != null)
        {
            rarityText.text = augment.rarity.ToString();
        }

        if (stackText != null)
        {
            stackText.text = stacks > 1 ? $"x{stacks}" : "";
        }
    }

    public void SetupSynergy(AugmentSynergyDefinition synergy)
    {
        if (synergy == null)
        {
            return;
        }

        if (iconImage != null)
        {
            iconImage.sprite = synergy.icon;
            iconImage.enabled = synergy.icon != null;
        }

        if (titleText != null)
        {
            titleText.text = synergy.displayName;
        }

        if (descriptionText != null)
        {
            descriptionText.text = synergy.description;
        }

        if (flavorText != null)
        {
            flavorText.text = synergy.flavorText;
        }
    }
}