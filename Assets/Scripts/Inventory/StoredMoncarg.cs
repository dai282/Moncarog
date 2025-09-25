using System;
using UnityEngine.UIElements;
using UnityEngine;

[Serializable]
public class StoredMoncarg : MonoBehaviour //MoncargInventoryItem (but MonoBehaviour to attach to GameObject)
{
    public MoncargInventoryAdapter Details;
    public VisualElement RootVisual;
    private Moncarg moncargComponent;

    
    void Awake()
    {
        moncargComponent = GetComponent<Moncarg>();

        if (Details != null && moncargComponent != null)
        {
            // Initialize Moncarg from item data if needed
            moncargComponent.LoadMoncargData(Details.moncargData);
        }
    }

    // Method to add this Moncarg to inventory
    public void AddToInventory()
    {
        if (PlayerInventory.Instance != null && Details != null)
        {
            // Check if already in inventory by comparing ItemDefinition
            bool alreadyInInventory = PlayerInventory.Instance.StoredMoncargs
                .Exists(item => item.Details == Details);

            if (!alreadyInInventory)
            {
                // Create a StoredMoncargData (data container, not MonoBehaviour)
                StoredMoncargData storedMoncarg = new StoredMoncargData
                {
                    Details = Details, // This is the ScriptableObject
                    RootVisual = null,
                    IsEquipped = false
                };

                PlayerInventory.Instance.StoredMoncargs.Add(storedMoncarg);
                Debug.Log($"Added {Details.FriendlyName} to inventory");

            }
            else
            {
                Debug.LogWarning($"{Details.FriendlyName} is already in inventory");
            }
        }

        //update enemy moncarg database
        MoncargDatabase.Instance.UpdateEnemyDatabase();
    }
}