using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class RunController : MonoBehaviour
{
    public static RunController Instance { get; private set; }

    [SerializeField] private AugmentDatabase augmentDatabase;
    [SerializeField] private SynergyDatabase synergyDatabase;

    private AttributesController attributesController;
    private AttributesController[] allAttributesControllers;
    private RunState runState = new();
    private AugmentDraftService draftService;

    public bool IsAugmentMenuOpen { get; private set; }
    public bool IsGameplayInputBlocked => IsAugmentMenuOpen || PauseMenu.isPaused;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

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
            runState.Seed = Random.Range(1, 999999);

        draftService = new AugmentDraftService(runState.Seed);

        RebindSceneReferences();
        RefreshBuild();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RebindSceneReferences();
        RefreshBuild();
    }

    private void RebindSceneReferences()
    {
        attributesController = FindFirstObjectByType<AttributesController>();
        allAttributesControllers = FindObjectsByType<AttributesController>(FindObjectsSortMode.None);
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

    public List<AugmentDefinition> GetThreeRandomAugments()
    {
        if (augmentDatabase == null)
        {
            Debug.LogError("AugmentDatabase is null");
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
        SaveSystem.SaveRun(runState.ToSaveData());
        RefreshBuild();
    }

    private bool TryApplyInstantAugment(AugmentDefinition augment)
    {
        if (augment.effectType != AugmentEffectType.Instant)
            return false;

        switch (augment.id)
        {
            case "health_potion":
                foreach (var ac in GetAllAttributesControllers())
                    ac.HealPercent(0.5f);
                return true;

            case "full_heal":
                foreach (var ac in GetAllAttributesControllers())
                    ac.FullHeal();
                return true;

            default:
                return false;
        }
    }

    private AttributesController[] GetAllAttributesControllers()
    {
        if (allAttributesControllers == null || allAttributesControllers.Length == 0)
            allAttributesControllers = FindObjectsByType<AttributesController>(FindObjectsSortMode.None);
        return allAttributesControllers;
    }

    public void RefreshBuild()
    {
        if (synergyDatabase == null)
            return;

        foreach (var ac in GetAllAttributesControllers())
        {
            if (ac == null) continue;

            PlayerBuildStats stats = BuildCalculator.BuildStats(
                ac,
                runState,
                synergyDatabase.GetAll());

            ac.ApplyCalculatedStats(stats);
        }
    }
}