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

    private bool suppressRuntimeStatSync = false;

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
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        if (attributesController != null)
        {
            attributesController.OnHealthChange -= HandleHealthChanged;
            attributesController.OnStaminaChange -= HandleStaminaChanged;
        }
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

        suppressRuntimeStatSync = true;

        RebindSceneReferences();
        RefreshBuild();
        RestoreRuntimeStats();

        suppressRuntimeStatSync = false;

        if (attributesController != null)
        {
            runState.CurrentHealth = attributesController.currentHealth;
            runState.CurrentStamina = attributesController.currentStamina;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        suppressRuntimeStatSync = true;

        RebindSceneReferences();
        RefreshBuild();
        RestoreRuntimeStats();

        suppressRuntimeStatSync = false;

        if (attributesController != null)
        {
            runState.CurrentHealth = attributesController.currentHealth;
            runState.CurrentStamina = attributesController.currentStamina;
        }

        if (scene.name == "main_menu")
        {
            return;
        }

        runState.CurrentSceneName = scene.name;
        SaveSystem.SaveRun(runState.ToSaveData());
    }

    private void RebindSceneReferences()
    {
        if (attributesController != null)
        {
            attributesController.OnHealthChange -= HandleHealthChanged;
            attributesController.OnStaminaChange -= HandleStaminaChanged;
        }

        attributesController = FindFirstObjectByType<AttributesController>();

        if (attributesController != null)
        {
            attributesController.OnHealthChange += HandleHealthChanged;
            attributesController.OnStaminaChange += HandleStaminaChanged;
        }
    }

    private void HandleHealthChanged(int currentHealth, int maxHealth)
    {
        if (suppressRuntimeStatSync)
        {
            return;
        }

        runState.CurrentHealth = currentHealth;
    }

    private void HandleStaminaChanged(int currentStamina, int maxStamina)
    {
        if (suppressRuntimeStatSync)
        {
            return;
        }

        runState.CurrentStamina = currentStamina;
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
        Destroy(transform.root.gameObject);
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
        {
            return;
        }

        if (TryApplyInstantAugment(augment))
        {
            return;
        }

        runState.AddAugment(augment.id);
        RefreshBuild();
    }

    public bool HasAugment(string augmentId)
    {
        return runState != null && runState.HasAugment(augmentId);
    }

    private bool TryApplyInstantAugment(AugmentDefinition augment)
    {
        if (augment.effectType != AugmentEffectType.Instant)
        {
            return false;
        }
            
        switch (augment.id)
        {
            case "rejuv":
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

            case "gamba":
                GiveRandomAugments(1);
                return true;

            case "random_augments":
                GiveRandomAugments(2);
                return true;

            default:
                return false;
        }
    }

    private void GiveRandomAugments(int number)
    {
        if (augmentDatabase == null)
        {
            return;
        }

        List<AugmentDefinition> candidates = new List<AugmentDefinition>();

        foreach (AugmentDefinition candidate in augmentDatabase.GetAll())
        {
            if (candidate == null)
            {
                continue;
            }

            if (candidate.id == "gamba")
            {
                continue;
            }

            if (runState.GetStacks(candidate.id) >= candidate.maxStacks)
            {
                continue;
            }

            if (IsExcludedByOwnedAugments(candidate))
            {
                continue;
            }

            if (IsExcludedByCandidate(candidate))
            {
                continue;
            }

            candidates.Add(candidate);
        }

        if (candidates.Count == 0)
        {
            return;
        }

        for (int i = 0; i < number; i++)
        {
            int index = Random.Range(0, candidates.Count);
            AugmentDefinition rolled = candidates[index];

            GiveAugment(rolled);
        }
    }

    private bool IsExcludedByOwnedAugments(AugmentDefinition candidate)
    {
        foreach (var kv in runState.OwnedStacks)
        {
            AugmentDefinition owned = augmentDatabase.GetById(kv.Key);

            if (owned == null)
            {
                continue;
            }

            if (owned.excludes != null && owned.excludes.Contains(candidate.id))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsExcludedByCandidate(AugmentDefinition candidate)
    {
        if (candidate.excludes == null)
        {
            return false;
        }

        foreach (var kv in runState.OwnedStacks)
        {
            if (candidate.excludes.Contains(kv.Key))
            {
                return true;
            }    
        }

        return false;
    }

    public void RefreshBuild()
    {
        if (attributesController == null || synergyDatabase == null)
        {
            return;
        }

        PlayerBuildStats stats = BuildCalculator.BuildStats(attributesController, runState, synergyDatabase.GetAll());

        attributesController.ApplyCalculatedStats(stats);
    }

    private void RestoreRuntimeStats()
    {
        if (attributesController == null)
        {
            return;
        }

        if (runState.CurrentHealth > 0)
        {
            attributesController.currentHealth = Mathf.Clamp(runState.CurrentHealth, 0, attributesController.maxHealth);
        }

        if (runState.CurrentStamina > 0)
        {
            attributesController.currentStamina = Mathf.Clamp(runState.CurrentStamina, 0, attributesController.maxStamina);
        }
    }
    public IReadOnlyDictionary<string, int> GetOwnedAugments()
    {
        return runState.OwnedStacks;
    }

    public IReadOnlyList<AugmentSynergyDefinition> GetOwnedSynergies()
    {
        if (synergyDatabase == null)
        {
            return new List<AugmentSynergyDefinition>();
        }

        return SynergyResolver.GetActiveSynergies(runState, synergyDatabase.GetAll());
    }

    public AugmentDefinition GetAugmentDefinition(string augmentId)
    {
        if (augmentDatabase == null)
        {
            return null;
        }

        return augmentDatabase.GetById(augmentId);
    }
}