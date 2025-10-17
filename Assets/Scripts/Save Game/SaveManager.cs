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

        // --- CORRECTED FILE PATH LOGIC ---

        // 1. Application.dataPath IS the "Assets" folder in the editor.
        // We want to create a folder inside it.
        string saveDirectory = Path.Combine(Application.dataPath, "UserData", "files");

        // 2. Create the directory if it doesn't already exist.
        // This is a crucial step.
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
            Debug.Log($"SUCCESS: Created save directory at: {saveDirectory}");
        }

        // 3. Set the final paths for your save files.
        _runDataPath = Path.Combine(saveDirectory, "current_run.json");
        _lifetimeStatsPath = Path.Combine(saveDirectory, "lifetime_stats.json");

        // 4. ADD THIS DEBUG LOG to be 100% sure the paths are correct.
        Debug.Log($"SaveManager Initialized. Run data will be saved to: {_runDataPath}");
    }

    #region --- Game Run Saving & Loading ---

    public void SaveRun()
    {
        Debug.Log($"Saving run data to: {_runDataPath}");
        RunData data = new RunData();

        // Player position
        var playerMovement = FindFirstObjectByType<PlayerMovement>();
        if (playerMovement != null)
        {
            data.playerPosition = playerMovement.transform.position;
            Debug.Log($"Saved player position: {data.playerPosition}");
        }

        // Inventory
        var inventory = PlayerInventory.Instance;
        if (inventory != null)
        {
            foreach (var item in inventory.StoredItems)
                data.items.Add(new SavedStoredItem { itemDefinitionId = item.Details.name });

            foreach (var moncarg in inventory.StoredMoncargs)
                data.moncargs.Add(new SavedStoredMoncarg {
                    moncargAdapterId = moncarg.Details.name,
                    currentHealth = moncarg.Details.moncargData.health,
                    currentMana = moncarg.Details.moncargData.mana,
                    isEquipped = moncarg.IsEquipped
                });
            
            Debug.Log($"Saved inventory: {data.items.Count} items, {data.moncargs.Count} moncargs");
        }

        if (GameManager.Instance != null && GameManager.Instance.mapManager != null)
        {
            var mm = GameManager.Instance.mapManager;

            // *** REVISED LOGIC ***
            var activeNode = mm.traversalOverlay.GetCurrentNode(); // Get the live node object.
            // Pass the active node in to get both the map data and the correct ID for that node.
            data.mapNodes = mm.GetSerializedMapData(activeNode, out int activeNodeId); 
            data.traversalPath = mm.GetTraversalPath();
            data.currentRoomId = activeNodeId; // Save the correct, generated ID.
            
            Debug.Log($"Saved map: {data.mapNodes.Count} nodes, {data.traversalPath.Count} path steps, currentRoomId={data.currentRoomId}");
        }
        else
        {
            Debug.LogError("Save failed: MapManager null.");
        }

        // Serialize and write file
        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(_runDataPath, json);
            Debug.Log("SaveRun: wrote file -> " + _runDataPath);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("SaveRun: file write failed: " + ex);
        }
    }

    // REMOVED the duplicate SerializableMapNode class from here

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