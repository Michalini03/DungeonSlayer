using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AugmentCardUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text flavorText;
    [SerializeField] private Button button;

    private AugmentDefinition augment;
    private AugmentSelectionUI owner;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);
        }
    }

    public void Setup(AugmentDefinition newAugment, AugmentSelectionUI newOwner)
    {
        augment = newAugment;
        owner = newOwner;

        if (iconImage != null)
        {
            iconImage.sprite = augment.icon;
            iconImage.enabled = augment.icon != null;
        }

        if (titleText != null)
        {
            titleText.text = augment.displayName;
        }

        if (rarityText != null)
        {
            rarityText.text = augment.rarity.ToString();
        }

        if (descriptionText != null)
        {
            descriptionText.text = augment.description;
        }

        if (flavorText != null)
        {
            flavorText.text = augment.flavorText;
        }
    }

    private void OnClicked()
    {
        if (owner != null && augment != null)
        {
            owner.SelectAugment(augment);
        }
    }
}