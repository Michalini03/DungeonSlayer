using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class RunController : MonoBehaviour
{
    public static RunController Instance { get; private set; }

    [SerializeField] private AugmentDatabase augmentDatabase;
    [SerializeField] private SynergyDatabase synergyDatabase;

    private AttributesController attributesController;
    private RunState runState = new();
    private AugmentDraftService draftService;

    public bool IsAugmentMenuOpen { get; private set; }
    public bool IsGameplayInputBlocked => IsAugmentMenuOpen || PauseMenu.isPaused;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            DontDestroyOnLoad(transform.root.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(transform.root.gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        RunSaveData save = SaveSystem.LoadRun();
        runState.LoadFromSaveData(save);

        if (runState.Seed == 0)
        {
            runState.Seed = Random.Range(1, 999999);
        }

        draftService = new AugmentDraftService(runState.Seed);

        RebindSceneReferences();
        RefreshBuild();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        runState.CurrentSceneName = scene.name;
        SaveSystem.SaveRun(runState.ToSaveData());

        RebindSceneReferences();
        RefreshBuild();
    }

    private void RebindSceneReferences()
    {
        attributesController = FindFirstObjectByType<AttributesController>();
    }

    public void SetAugmentMenuOpen(bool isOpen)
    {
        IsAugmentMenuOpen = isOpen;
        RefreshPauseState();
    }

    public void RefreshPauseState()
    {
        Time.timeScale = (IsAugmentMenuOpen || PauseMenu.isPaused) ? 0f : 1f;
    }

    public void EndRun()
    {
        SaveSystem.DeleteRun();
    }

    public List<AugmentDefinition> GetThreeRandomAugments()
    {
        if (augmentDatabase == null)
        {
            return new List<AugmentDefinition>();
        }

        return draftService.GenerateDraft(augmentDatabase.GetAll(), runState, 3);
    }

    public void GiveAugment(AugmentDefinition augment)
    {
        if (augment == null)
            return;

        if (TryApplyInstantAugment(augment))
            return;

        runState.AddAugment(augment.id);
        RefreshBuild();
    }

    private bool TryApplyInstantAugment(AugmentDefinition augment)
    {
        if (augment.effectType != AugmentEffectType.Instant)
            return false;

        switch (augment.id)
        {
            case "health_potion":
                if (attributesController != null)
                {
                    attributesController.HealPercent(0.5f);
                }
                return true;

            case "full_heal":
                if (attributesController != null)
                {
                    attributesController.FullHeal();
                }
                return true;

            default:
                return false;
        }
    }

    public void RefreshBuild()
    {
        if (attributesController == null || synergyDatabase == null)
        {
            return;
        }

        PlayerBuildStats stats = BuildCalculator.BuildStats(
            attributesController,
            runState,
            synergyDatabase.GetAll()
        );

        attributesController.ApplyCalculatedStats(stats);
    }
}