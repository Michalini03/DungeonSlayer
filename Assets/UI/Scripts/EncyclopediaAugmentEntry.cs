using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EncyclopediaAugmentEntry : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text flavorText;
    [SerializeField] private TMP_Text rarityText;

    public void Setup(AugmentDefinition augment)
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
    }
}