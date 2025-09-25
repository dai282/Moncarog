using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MoncargDatabase : MonoBehaviour
{
    public GameObject[] allMoncargs;
    public List<GameObject> availableEnemyMoncargs = new List<GameObject>(); // Moncargs that can be encountered as enemies

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
        UpdateEnemyDatabase();
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
