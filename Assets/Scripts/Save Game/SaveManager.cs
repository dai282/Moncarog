// SaveManager.cs

using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private string _runDataPath;
    private string _lifetimeStatsPath;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Use Application.persistentDataPath for safe cross-platform saving
        _runDataPath = Path.Combine(Application.persistentDataPath, "current_run.json");
        _lifetimeStatsPath = Path.Combine(Application.persistentDataPath, "lifetime_stats.json");
    }

    #region --- Game Run Saving & Loading ---

    [System.Obsolete]
    public void SaveRun()
    {
        Debug.Log($"Saving run data to: {_runDataPath}");
        RunData data = new RunData();

        // 1. Save Player & Room Position
        var playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
        {
            data.playerPosition = playerMovement.transform.position;
            data.currentRoomId = playerMovement.roomGrid.roomGridID; // Assuming roomGrid has an ID
        }

        // 2. Save Inventory
        var inventory = PlayerInventory.Instance;
        foreach (var item in inventory.StoredItems)
        {
            data.items.Add(new SavedStoredItem { itemDefinitionId = item.Details.name });
        }
        foreach (var moncarg in inventory.StoredMoncargs)
        {
            data.moncargs.Add(new SavedStoredMoncarg
            {
                moncargAdapterId = moncarg.Details.name,
                currentHealth = moncarg.Details.moncargData.health,
                currentMana = moncarg.Details.moncargData.mana,
                isEquipped = moncarg.IsEquipped
            });
        }

        // 3. Save Map and Traversal Path (Requires modifications to other scripts)
        var mapManager = FindObjectOfType<MapManager>();
        if (mapManager != null)
        {
            data.mapNodes = mapManager.GetSerializableMap();
            data.traversalPath = mapManager.traversalOverlay.GetTraversalPath();
        }

        // Write to file
        string json = JsonUtility.ToJson(data, true); // 'true' for pretty print
        File.WriteAllText(_runDataPath, json);
        Debug.Log("Run saved successfully.");
    }

    public RunData LoadRun()
    {
        if (File.Exists(_runDataPath))
        {
            Debug.Log($"Loading run data from: {_runDataPath}");
            string json = File.ReadAllText(_runDataPath);
            RunData data = JsonUtility.FromJson<RunData>(json);
            return data;
        }
        Debug.Log("No run data file found.");
        return null;
    }

    public bool HasSavedRun()
    {
        return File.Exists(_runDataPath);
    }
    
    public void DeleteSavedRun()
    {
        if(File.Exists(_runDataPath))
        {
            File.Delete(_runDataPath);
            Debug.Log("Deleted saved run data.");
        }
    }

    #endregion

    #region --- Lifetime Stats Saving & Loading ---

    public void SaveLifetimeStats(GameStats stats)
    {
        string json = JsonUtility.ToJson(stats, true);
        File.WriteAllText(_lifetimeStatsPath, json);
        Debug.Log("Lifetime stats saved.");
    }

    public GameStats LoadLifetimeStats()
    {
        if (File.Exists(_lifetimeStatsPath))
        {
            string json = File.ReadAllText(_lifetimeStatsPath);
            return JsonUtility.FromJson<GameStats>(json);
        }
        // If no file, return a fresh object
        return new GameStats();
    }

    #endregion
}