using UnityEngine;
using System;

// 1. Data Structure for Stats
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
    public float timeSpentIngame = 0f;
}

// 2. The Stats Collector Singleton
public class StatsCollector : MonoBehaviour
{
    public static StatsCollector Instance { get; private set; }
    private const string LIFETIME_RECORD_KEY = "PlayerLifetimeStats";

    [Header("Current Session Stats")]
    [SerializeField] private GameStats CurrentSessionStats = new GameStats();
    private GameStats _lastSavedSessionStats = new GameStats(); 

// Helper method to create a deep copy of GameStats
private GameStats DeepCopy(GameStats original)
{
    // Use Unity's JsonUtility for a quick and reliable deep copy
    string json = JsonUtility.ToJson(original);
    return JsonUtility.FromJson<GameStats>(json);
}

    [Header("Lifetime Total Stats")]
    [SerializeField] private GameStats LifetimeRecord = new GameStats();

    // Subscribe in Awake. This is robust and survives pausing/scene loads.
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadLifetimeRecord();

        _lastSavedSessionStats = DeepCopy(CurrentSessionStats);

        // Subscribe to the time event HERE
        GameManager.OnTimeTick += RecordTimeTick; 
    }

    // Unsubscribe in OnDestroy to prevent memory leaks when app quits.
    private void OnDestroy()
    {
        GameManager.OnTimeTick -= RecordTimeTick;
    }

    // This method now directly updates the stats object.
    // No more _sessionTimeElapsed.
    public void RecordTimeTick(float deltaTime, float timeScale)
    {
        if (timeScale > 0f)
        {
            CurrentSessionStats.timeSpentIngame += deltaTime;
        }
    }

    private void LoadLifetimeRecord()
    {
        LifetimeRecord = SaveManager.Instance.LoadLifetimeStats();
        Debug.Log("[StatsCollector] Lifetime record loaded via SaveManager.");
    }

    // Cleaned up and removed all references to _sessionTimeElapsed.
    public void SaveStats()
    {
        // 1. Calculate the Delta (Change since last save) for every stat
        GameStats delta = new GameStats
        {
            StepsTaken = CurrentSessionStats.StepsTaken - _lastSavedSessionStats.StepsTaken,
            DamageDealt = CurrentSessionStats.DamageDealt - _lastSavedSessionStats.DamageDealt,
            moncargsDefeated = CurrentSessionStats.moncargsDefeated - _lastSavedSessionStats.moncargsDefeated,
            HPRecovered = CurrentSessionStats.HPRecovered - _lastSavedSessionStats.HPRecovered,
            HPLost = CurrentSessionStats.HPLost - _lastSavedSessionStats.HPLost,
            ManaRecovered = CurrentSessionStats.ManaRecovered - _lastSavedSessionStats.ManaRecovered,
            ManaSpent = CurrentSessionStats.ManaSpent - _lastSavedSessionStats.ManaSpent,
            PotionsUsed = CurrentSessionStats.PotionsUsed - _lastSavedSessionStats.PotionsUsed,
            AbilitiesUsed = CurrentSessionStats.AbilitiesUsed - _lastSavedSessionStats.AbilitiesUsed,
            timeSpentIngame = CurrentSessionStats.timeSpentIngame - _lastSavedSessionStats.timeSpentIngame
        }; // <-- The object initializer block is correct and will now compile.

        // 2. Add the Delta (only the new progress) to the LifetimeRecord
        LifetimeRecord.StepsTaken += delta.StepsTaken;
        LifetimeRecord.DamageDealt += delta.DamageDealt;
        LifetimeRecord.moncargsDefeated += delta.moncargsDefeated;
        LifetimeRecord.HPRecovered += delta.HPRecovered;
        LifetimeRecord.HPLost += delta.HPLost;
        LifetimeRecord.ManaRecovered += delta.ManaRecovered;
        LifetimeRecord.ManaSpent += delta.ManaSpent;
        LifetimeRecord.PotionsUsed += delta.PotionsUsed;
        LifetimeRecord.AbilitiesUsed += delta.AbilitiesUsed;
        LifetimeRecord.moncargsCollected += delta.moncargsCollected;
        LifetimeRecord.timeSpentIngame += delta.timeSpentIngame;

        Debug.Log("Current time - " + CurrentSessionStats.timeSpentIngame + " | Lifetime time - " + LifetimeRecord.timeSpentIngame);
        
        // 3. Save the updated lifetime record
        SaveManager.Instance.SaveLifetimeStats(LifetimeRecord);

        // 4. CRITICAL: Update the snapshot to reflect the current session state.
        _lastSavedSessionStats = DeepCopy(CurrentSessionStats); 

        Debug.Log($"[StatsCollector] Stats saved and merged. Lifetime Steps: {LifetimeRecord.StepsTaken}, Lifetime Damage: {LifetimeRecord.DamageDealt}");
    }

    // Simply returns the stats object.
    public GameStats GetCurrentSessionStats()
    {
        return CurrentSessionStats;
    }

    public GameStats GetLifetimeRecord() => LifetimeRecord;

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
            CurrentSessionStats.HPRecovered += amount;
        else
            CurrentSessionStats.HPLost += amount;
    }

    public void RecordManaChange(float amount, bool isRecover)
    {
        if (isRecover)
            CurrentSessionStats.ManaRecovered += amount;
        else
            CurrentSessionStats.ManaSpent += amount;
    }

    public void RecordPotionUsed()
    {
        CurrentSessionStats.PotionsUsed++;
    }

    public void RecordAbilityUsed()
    {
        CurrentSessionStats.AbilitiesUsed++;
    }

    // Cleaned up.
    public void SetCurrentSessionStats(GameStats stats)
    {
        if (stats != null)
        {
            CurrentSessionStats = stats;
            // CRITICAL: Initialize the last saved stats tracker with the loaded data
            _lastSavedSessionStats = DeepCopy(stats); 
            Debug.Log($"[StatsCollector] Session stats restored from save file. ({stats.StepsTaken} steps)");
        }
        else
        {
            CurrentSessionStats = new GameStats();
            _lastSavedSessionStats = new GameStats(); // Fresh start also means fresh snapshot
            Debug.Log("[StatsCollector] No session stats in save file, starting fresh.");
        }
    }

    // Updated: Reset snapshot when a new run begins
    public void ResetSessionStats()
    {
        CurrentSessionStats = new GameStats();
        // CRITICAL: Reset the snapshot too
        _lastSavedSessionStats = new GameStats(); 
        Debug.Log("[StatsCollector] Session stats reset for new game.");
    }
    // Cleaned up.
    // public void SetCurrentSessionStats(GameStats stats)
    // {
    //     if (stats != null)
    //     {
    //         CurrentSessionStats = stats;
    //         Debug.Log($"[StatsCollector] Session stats restored from save file. ({stats.StepsTaken} steps)");
    //     }
    //     else
    //     {
    //         CurrentSessionStats = new GameStats();
    //         Debug.Log("[StatsCollector] No session stats in save file, starting fresh.");
    //     }
    // }
}