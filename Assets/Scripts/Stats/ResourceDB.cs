// ResourceDB.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ResourceDB : MonoBehaviour
{
    public static ResourceDB Instance { get; private set; }
    public bool IsReady { get; private set; } = false;

    private Dictionary<string, ItemDefinition> _itemDefinitions;
    private Dictionary<string, MoncargInventoryAdapter> _moncargAdapters;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadResources();
        IsReady = true;
        Debug.Log("[ResourceDB] ResourceDB is ready.");
    }

    private void LoadResources()
    {
        // Load all ItemDefinition assets from any "Resources" folder
        var items = Resources.LoadAll<ItemDefinition>("");
        _itemDefinitions = items.ToDictionary(item => item.name, item => item);
        Debug.Log($"[ResourceDB] Loaded {_itemDefinitions.Count} Item Definitions.");

        // Load all MoncargInventoryAdapter assets
        var moncargs = Resources.LoadAll<MoncargInventoryAdapter>("");
        _moncargAdapters = moncargs.ToDictionary(moncarg => moncarg.name, moncarg => moncarg);
        Debug.Log($"[ResourceDB] Loaded {_moncargAdapters.Count} Moncarg Adapters.");
    }

    public ItemDefinition GetItemByID(string id)
    {
        _itemDefinitions.TryGetValue(id, out ItemDefinition item);
        if (item == null) Debug.LogWarning($"[ResourceDB] Could not find ItemDefinition with ID: {id}");
        return item;
    }

    public MoncargInventoryAdapter GetMoncargByID(string id)
    {
        _moncargAdapters.TryGetValue(id, out MoncargInventoryAdapter moncarg);
        if (moncarg == null) Debug.LogWarning($"[ResourceDB] Could not find MoncargInventoryAdapter with ID: {id}");
        return moncarg;
    }
}