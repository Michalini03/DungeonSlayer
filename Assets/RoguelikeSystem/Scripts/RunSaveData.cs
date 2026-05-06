using System;
using System.Collections.Generic;

[Serializable]
public class RunSaveData
{
    public int runSeed;
    public string currentSceneName;

    public int currentHealth;
    public int currentStamina;

    public List<string> ownedAugmentIds = new();
    public List<int> ownedAugmentStacks = new();
}

// In case we want to implement augment unlocks (don't wanna rn)
/*
[Serializable]
public class MetaProgressionData
{
    public List<string> unlockedAugmentIds = new();
}
*/