using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AugmentSelectionUI : MonoBehaviour
{
    [SerializeField] private AugmentCardUI[] cards;
    [SerializeField] private EnemySpawner spawner;

    [Header("UI References")]
    [SerializeField] private GameObject playerUI;

    public bool isSelecting = false;

    private List<AugmentDefinition> currentChoices = new();

    private void Awake()
    {
        if (cards == null || cards.Length == 0)
        {
            cards = GetComponentsInChildren<AugmentCardUI>(true);
        }
            
    }

    private void Start()
    {
        Hide();
    }

    public void ShowSelection()
    {
        if (RunController.Instance == null)
        {
            return;
        }

        if (playerUI != null)
        {
            playerUI.SetActive(false);
        }

        currentChoices = RunController.Instance.GetThreeRandomAugments();
        gameObject.SetActive(true);

        EventSystem.current?.SetSelectedGameObject(null);

        for (int i = 0; i < cards.Length; i++)
        {
            if (i < currentChoices.Count)
            {
                cards[i].gameObject.SetActive(true);
                cards[i].Setup(currentChoices[i], this);
            }
            else
            {
                cards[i].gameObject.SetActive(false);
            }
        }

        isSelecting = true;
        RunController.Instance.SetAugmentMenuOpen(true);

    }

    public void Hide()
    {
        if (playerUI != null)
        {
            playerUI.SetActive(true);
        }

        EventSystem.current?.SetSelectedGameObject(null);
        gameObject.SetActive(false);

        if (RunController.Instance != null)
        {
            RunController.Instance.SetAugmentMenuOpen(false);
        }

    }

    public void SelectAugment(AugmentDefinition augment)
    {
        if (RunController.Instance == null)
        {
            return;
        }

        RunController.Instance.GiveAugment(augment);
        Hide();
        isSelecting = false;
        spawner.selectedAugment = true;
    }
}