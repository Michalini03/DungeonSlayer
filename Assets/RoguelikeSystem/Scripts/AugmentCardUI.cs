using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AugmentCardUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text flavorText;
    [SerializeField] private Button button;

    private AugmentDefinition currentAugment;
    private AugmentSelectionUI selectionUI;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
    }

    public void Setup(AugmentDefinition augment, AugmentSelectionUI owner)
    {
        currentAugment = augment;
        selectionUI = owner;

        if (titleText != null)
            titleText.text = augment.displayName;

        if (descriptionText != null)
            descriptionText.text = augment.description;

        if (flavorText != null)
            flavorText.text = augment.flavorText;

        if (iconImage != null)
        {
            iconImage.sprite = augment.icon;
            iconImage.enabled = augment.icon != null;
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);
        }
    }

    private void OnClicked()
    {
        if (selectionUI != null && currentAugment != null)
        {
            selectionUI.SelectAugment(currentAugment);
        }
    }
}