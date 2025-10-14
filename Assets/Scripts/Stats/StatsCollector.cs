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

    // --- Loading & Saving Logic ---
    
    // Step 1: Load the existing lifetime record from the save file
    private void LoadLifetimeRecord()
    {
        if (PlayerPrefs.HasKey(LIFETIME_RECORD_KEY))
        {
            string json = PlayerPrefs.GetString(LIFETIME_RECORD_KEY);
            LifetimeRecord = JsonUtility.FromJson<GameStats>(json);
            Debug.Log("[StatsCollector] Lifetime record loaded successfully.");
        }
        else
        {
            // If no record exists, initialize with a new, empty GameStats object
            LifetimeRecord = new GameStats();
            Debug.Log("[StatsCollector] No existing lifetime record found. Initializing new record.");
        }
    }

    // Step 2: Merge the session stats into the lifetime record and save
    public void SaveStats()
    {
        // 1. Merge CurrentSessionStats into LifetimeRecord (additive)
        LifetimeRecord.StepsTaken += CurrentSessionStats.StepsTaken;
        LifetimeRecord.DamageDealt += CurrentSessionStats.DamageDealt;
        LifetimeRecord.moncargsDefeated += CurrentSessionStats.moncargsDefeated;
        LifetimeRecord.HPRecovered += CurrentSessionStats.HPRecovered;
        LifetimeRecord.HPLost += CurrentSessionStats.HPLost;
        LifetimeRecord.ManaRecovered += CurrentSessionStats.ManaRecovered;
        LifetimeRecord.ManaSpent += CurrentSessionStats.ManaSpent;
        LifetimeRecord.PotionsUsed += CurrentSessionStats.PotionsUsed;
        LifetimeRecord.AbilitiesUsed += CurrentSessionStats.AbilitiesUsed;
        LifetimeRecord.moncargsCollected += CurrentSessionStats.moncargsCollected;

        // 2. Save the merged LifetimeRecord back to PlayerPrefs/save file
        string json = JsonUtility.ToJson(LifetimeRecord);
        PlayerPrefs.SetString(LIFETIME_RECORD_KEY, json);
        PlayerPrefs.Save(); // Ensure data is written to disk
        
        // 3. Optional: Reset session stats after successful save
        CurrentSessionStats = new GameStats();

        Debug.Log($"[StatsCollector] Stats saved and merged. Lifetime Steps: {LifetimeRecord.StepsTaken}");
    }

    // --- Public Getters (for displaying stats) ---

    public GameStats GetCurrentSessionStats() => CurrentSessionStats;
    public GameStats GetLifetimeRecord() => LifetimeRecord;

    // --- Public Recording Methods (called by other scripts) ---

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
