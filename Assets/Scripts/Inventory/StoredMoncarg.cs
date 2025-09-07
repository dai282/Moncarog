using System;
using UnityEngine.UIElements;
using UnityEngine;

[Serializable]
public class StoredMoncarg : MonoBehaviour //MoncargInventoryItem
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
    /*
        // Method to add this Moncarg to inventory
        public void AddToInventory()
        {
            if (PlayerInventory.Instance != null && MoncargItem != null)
            {
                // Create a StoredItem for the inventory system
                StoredItem storedItem = new StoredItem
                {
                    Details = MoncargItem,
                    RootVisual = null // This will be created by the inventory system
                };

                PlayerInventory.Instance.StoredItems.Add(storedItem);

                // Optional: Hide or disable the Moncarg in the world
                gameObject.SetActive(false);
            }
        }

        // Method to remove from inventory and spawn in world
        public void RemoveFromInventory(Vector3 position)
        {
            if (PlayerInventory.Instance != null && MoncargItem != null)
            {
                // Find and remove from inventory
                StoredItem itemToRemove = PlayerInventory.Instance.StoredItems
                    .Find(item => item.Details == MoncargItem);

                if (itemToRemove != null)
                {
                    PlayerInventory.Instance.StoredItems.Remove(itemToRemove);

                    // Spawn in world
                    transform.position = position;
                    gameObject.SetActive(true);
                }
            }
        }
        */
}