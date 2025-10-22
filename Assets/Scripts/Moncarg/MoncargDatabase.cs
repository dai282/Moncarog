using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Elementals;

public class MoncargDatabase : MonoBehaviour
{
    public GameObject[] allMoncargs;
    public List<GameObject> availableEnemyMoncargs = new List<GameObject>(); // Moncargs that can be encountered as enemies
    public List<GameObject> fireMoncargs = new List<GameObject>();
    public List<GameObject> waterMoncargs = new List<GameObject>();
    public List<GameObject> plantMoncargs = new List<GameObject>();
    public List<GameObject> normalMoncargs = new List<GameObject>();

    //static instance that stores reference to the GameManager. public get and private set
    public static MoncargDatabase Instance { get; private set; }

    private void Awake()
    {
        // Singleton pattern to ensure only one instance of GameManager exists
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void resetMoncargDatabase()
    {
        foreach (GameObject moncarg in allMoncargs)
        {
            StoredMoncarg storedMoncarg = moncarg.GetComponent<StoredMoncarg>();
            if (storedMoncarg != null)
            {
                storedMoncarg.Details.resetData();
            }
        }
        UpdateMoncargLists();
        UpdateEnemyDatabase();
    }

    public void UpdateMoncargLists()
    {
        // Clear all lists
        fireMoncargs.Clear();
        waterMoncargs.Clear();
        plantMoncargs.Clear();
        normalMoncargs.Clear();

        // Get all moncargs that player owns
        var ownedMoncargNames = GetOwnedMoncargNames();

        foreach (GameObject moncargPrefab in allMoncargs)
        {
            StoredMoncarg storedMoncarg = moncargPrefab.GetComponent<StoredMoncarg>();

            if (storedMoncarg == null || storedMoncarg.Details == null)
                continue;

            var details = storedMoncarg.Details;

            // Skip player-owned Moncargs
            if (ownedMoncargNames.Contains(details.FriendlyName))
                continue;

            // Sort by element type
            if (details.moncargData.isMiniBoss == true)
                continue;

            if (details.moncargData.isBoss == true)
                continue;

            switch (details.moncargData.type)
            {
                case ElementalType.Fire:
                    fireMoncargs.Add(moncargPrefab);
                    normalMoncargs.Add(moncargPrefab);
                    break;
                case ElementalType.Water:
                    waterMoncargs.Add(moncargPrefab);
                    normalMoncargs.Add(moncargPrefab);
                    break;
                case ElementalType.Plant:
                    plantMoncargs.Add(moncargPrefab);
                    normalMoncargs.Add(moncargPrefab);
                    break;
                case ElementalType.Normal:
                default:
                    fireMoncargs.Add(moncargPrefab);
                    waterMoncargs.Add(moncargPrefab);
                    plantMoncargs.Add(moncargPrefab);
                    normalMoncargs.Add(moncargPrefab);
                    break;
            }
        }

        Debug.Log($"Moncarg lists updated: Fire={fireMoncargs.Count}, Water={waterMoncargs.Count}, Plant={plantMoncargs.Count}, Normal={normalMoncargs.Count}");
    }


    public void UpdateEnemyDatabase()
    {
        // Clear current available enemies
        availableEnemyMoncargs.Clear();

        // Get all moncargs that player owns
        var ownedMoncargNames = GetOwnedMoncargNames();

        // Filter out moncargs that player already owns
        foreach (GameObject moncargPrefab in allMoncargs)
        {
            StoredMoncarg storedMoncarg = moncargPrefab.GetComponent<StoredMoncarg>();
            if (storedMoncarg != null && storedMoncarg.Details != null)
            {
                // Check if player owns this moncarg
                if (!ownedMoncargNames.Contains(storedMoncarg.Details.FriendlyName))
                {
                    availableEnemyMoncargs.Add(moncargPrefab);
                }
            }
        }

        Debug.Log($"Enemy database updated. Available enemies: {availableEnemyMoncargs.Count}/{allMoncargs.Length}");
    }

    private HashSet<string> GetOwnedMoncargNames()
    {
        HashSet<string> ownedNames = new HashSet<string>();

        if (PlayerInventory.Instance != null)
        {
            foreach (var storedMoncarg in PlayerInventory.Instance.StoredMoncargs)
            {
                if (storedMoncarg?.Details != null)
                {
                    ownedNames.Add(storedMoncarg.Details.FriendlyName);
                }
            }
        }

        return ownedNames;
    }
}
