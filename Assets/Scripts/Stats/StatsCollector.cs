using UnityEngine;
using System;

// 1. Data Structure for Stats
// We use a separate class/struct to ensure it's easily serializable for saving.
[Serializable]
public class GameStats
{
    // Movement Stats
    public int StepsTaken = 0;

    // Combat Stats (Outgoing)
    public float DamageDealt = 0f;
    public int moncargsDefeated = 0;
    
    // Resource Management
    public float HPRecovered = 0f;
    public float HPLost = 0f;
    public float ManaRecovered = 0f;
    public float ManaSpent = 0f;

    // Usage and Collection
    public int PotionsUsed = 0;
    public int AbilitiesUsed = 0;
    public int moncargsCollected = 0;
}

// 2. The Stats Collector Singleton
public class StatsCollector : MonoBehaviour
{
    // Singleton Access
    public static StatsCollector Instance { get; private set; }

    // Key for PlayerPrefs/Save File
    private const string LIFETIME_RECORD_KEY = "PlayerLifetimeStats";

    [Header("Current Session Stats")]
    // Stats gathered since the game started or loaded.
    [SerializeField] private GameStats CurrentSessionStats = new GameStats();

    [Header("Lifetime Total Stats")]
    // Stats loaded from the save file (all-time total).
    [SerializeField] private GameStats LifetimeRecord = new GameStats();

    private void Awake()
    {
        // Implement Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scenes if needed
        
        LoadLifetimeRecord();
    }

    private void LoadLifetimeRecord()
{
    // Use the SaveManager to load the data
    LifetimeRecord = SaveManager.Instance.LoadLifetimeStats();
    Debug.Log("[StatsCollector] Lifetime record loaded via SaveManager.");
}

// Step 2: Merge the session stats into the lifetime record and save
public void SaveStats()
{
    // 1. Merge session stats into lifetime record (this logic is fine)
    LifetimeRecord.StepsTaken += CurrentSessionStats.StepsTaken;
    // ... (rest of the merge logic)
    LifetimeRecord.moncargsCollected += CurrentSessionStats.moncargsCollected;

    // 2. Use the SaveManager to save the updated lifetime record
    SaveManager.Instance.SaveLifetimeStats(LifetimeRecord);

    // 3. IMPORTANT: Reset session stats after merging
    CurrentSessionStats = new GameStats();

    Debug.Log($"[StatsCollector] Stats saved and merged. Lifetime Steps: {LifetimeRecord.StepsTaken}");
}

    // Public Getters (for displaying stats)

    public GameStats GetCurrentSessionStats() => CurrentSessionStats;
    public GameStats GetLifetimeRecord() => LifetimeRecord;

    // Public Recording Methods (called by other scripts)

    public void RecordStep()
    {
        CurrentSessionStats.StepsTaken++;
    }

    public void RecordDamageDealt(float amount)
    {
        CurrentSessionStats.DamageDealt += amount;
    }

    public void RecordMoncarogDefeated()
    {
        CurrentSessionStats.moncargsDefeated++;
    }

    public void RecordMoncarogCollected()
    {
        CurrentSessionStats.moncargsCollected++;
    }

    public void RecordHPChange(float amount, bool isHeal)
    {
        if (isHeal)
        {
            CurrentSessionStats.HPRecovered += amount;
        }
        else // Damage taken / HP lost
        {
            CurrentSessionStats.HPLost += amount;
        }
    }

    public void RecordManaChange(float amount, bool isRecover)
    {
        if (isRecover)
        {
            CurrentSessionStats.ManaRecovered += amount;
        }
        else // Mana spent
        {
            CurrentSessionStats.ManaSpent += amount;
        }
    }

    public void RecordPotionUsed()
    {
        CurrentSessionStats.PotionsUsed++;
    }

    public void RecordAbilityUsed()
    {
        CurrentSessionStats.AbilitiesUsed++;
    }
}
