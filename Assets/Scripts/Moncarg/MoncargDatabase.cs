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

    private int currentRoomLevel = 1;
    private const int TOTAL_ROOMS = 12;
    private const int MAX_LEVEL = 12;

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

    public int GetNumberOfBossAndMiniboss()
    {
        int count = 0;
        
        foreach (GameObject moncarg in availableEnemyMoncargs)
        {
            if (moncarg.GetComponent<StoredMoncarg>().Details.moncargData.isBoss == true || moncarg.GetComponent<StoredMoncarg>().Details.moncargData.isMiniBoss == true)
            {
                count++;
            }
        }
        return count;
    }

    public void SetRoomLevel(int roomNumber)
    {
        currentRoomLevel = Mathf.Clamp(roomNumber, 1, TOTAL_ROOMS);
        UpdateEnemyLevels();
    }

    public void resetMoncargDatabase()
    {
        foreach (GameObject moncarg in allMoncargs)
        {
            StoredMoncarg storedMoncarg = moncarg.GetComponent<StoredMoncarg>();
            if (storedMoncarg != null)
            {
                storedMoncarg.Details.moncargData.InitializeBaseStats();
            }
        }
        UpdateMoncargLists();
        currentRoomLevel = 1;
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

    private void UpdateEnemyLevels()
    {
        // Enemy level scales with room progression
        // Room 1: level 1, Room 12: level 12 (linear progression)
        int targetLevel = Mathf.Clamp(currentRoomLevel, 1, MAX_LEVEL);

        foreach (GameObject moncargPrefab in availableEnemyMoncargs)
        {
            StoredMoncarg storedMoncarg = moncargPrefab.GetComponent<StoredMoncarg>();
            if (storedMoncarg != null && storedMoncarg.Details != null)
            {
                MoncargData data = storedMoncarg.Details.moncargData;

                // Don't scale bosses - they have fixed levels
                if (!data.isBoss && !data.isMiniBoss)
                {
                    // Set enemy level based on room progression
                    data.level = targetLevel;
                    data.ScaleStatsToLevel();
                    data.reset(); // Reset health/mana
                }
            }
        }

        Debug.Log($"Enemy levels updated to level {targetLevel} for room {currentRoomLevel}");
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

    // Add this to MoncargDatabase.cs
    public List<GameObject> GetStarterMoncargs()
    {
        if (allMoncargs.Length == 0) return null;

        // Filter for starter-appropriate Moncargs (you can customize this logic)
        List<GameObject> starterMoncargs = new List<GameObject>();
        foreach (GameObject moncarg in allMoncargs)
        {

            StoredMoncarg stored = moncarg.GetComponent<StoredMoncarg>();
            MoncargData data = stored.Details.moncargData;

            //exclude boss and miniboss moncargs
            if (!(data.isBoss || data.isMiniBoss))
            {
                starterMoncargs.Add(moncarg);
            }

        }

        // If no filtered Moncargs, use all available
        if (starterMoncargs.Count == 0)
            starterMoncargs = allMoncargs.ToList();

        List<GameObject> moncargsToChooseFrom = new List<GameObject>();

        //3 starter moncargs to choose from
        for (int i = 0; i < 3; i++)
        {
            int randomIndex = Random.Range(0, starterMoncargs.Count);

            //make sure all moncargs are different types
            if (moncargsToChooseFrom.Count > 0)
            {
                StoredMoncarg chosen = starterMoncargs[randomIndex].GetComponent<StoredMoncarg>();

                for (int j = 0; j < moncargsToChooseFrom.Count; j++)
                {
                    //if the randomly chosen moncarg is of the same type as one already chosen, pick a new one
                    if (chosen.Details.moncargData.type == moncargsToChooseFrom[j].GetComponent<StoredMoncarg>().Details.moncargData.type)
                    {
                        randomIndex = Random.Range(0, starterMoncargs.Count);
                        chosen = starterMoncargs[randomIndex].GetComponent<StoredMoncarg>();
                        j = -1; // Restart the check
                    }
                }
            }

            moncargsToChooseFrom.Add(starterMoncargs[randomIndex]);
            starterMoncargs.RemoveAt(randomIndex); // Ensure uniqueness
        }
        return moncargsToChooseFrom;
    }
}
