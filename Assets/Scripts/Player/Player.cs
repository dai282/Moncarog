using System.Linq;
using UnityEngine;


public class Player : MonoBehaviour
{
    [SerializeField] private string playerName;
    [SerializeField] private PlayerInventory inventory;
    
    public string Name 
    { 
        get { return playerName; } 
        set { playerName = value; } 
    }
    
    public PlayerInventory Inventory 
    { 
        get { return inventory; } 
        set { inventory = value; } 
    }
    
    private void Start()
    {
        // Initialize inventory if not assigned
        if (inventory == null)
        {
            inventory = PlayerInventory.Instance;
        }
    }
    
    public void ViewInventory()
    {
        if (inventory != null)
        {
            // Toggle inventory visibility or trigger inventory UI display
            Debug.Log($"{playerName} is viewing inventory");
            // You can add UI logic here to show/hide inventory panel
        }
        else
        {
            Debug.LogWarning("No inventory assigned to player");
        }
    }
    
    public void UseItem(ItemDefinition item)
    {
        if (inventory == null)
        {
            Debug.LogWarning("No inventory assigned to player");
            return;
        }
        
        // Find the item in inventory
        StoredItem storedItem = inventory.StoredItems.FirstOrDefault(s => s.Details == item);
        
        if (storedItem != null)
        {
            Debug.Log($"{playerName} is using {item.FriendlyName}");
            
            // Add item usage logic here
            // For example: apply effects, consume item, etc.
            
            // Optionally remove item from inventory after use
            // RemoveItemFromInventory(storedItem);
        }
        else
        {
            Debug.Log($"{playerName} doesn't have {item.FriendlyName} in inventory");
        }
    }
    
    public void UseItem(string itemName)
    {
        if (inventory == null)
        {
            Debug.LogWarning("No inventory assigned to player");
            return;
        }
        
        // Find item by name
        StoredItem storedItem = inventory.StoredItems.FirstOrDefault(s => 
            s.Details.FriendlyName.Equals(itemName, System.StringComparison.OrdinalIgnoreCase));
        
        if (storedItem != null)
        {
            UseItem(storedItem.Details);
        }
        else
        {
            Debug.Log($"{playerName} doesn't have '{itemName}' in inventory");
        }
    }
    
    private void RemoveItemFromInventory(StoredItem item)
    {
        if (item.RootVisual != null)
        {
            // Remove visual element
            item.RootVisual.RemoveFromHierarchy();
        }
        
        // Remove from stored items list
        inventory.StoredItems.Remove(item);
        
        Debug.Log($"Removed {item.Details.FriendlyName} from {playerName}'s inventory");
    }
}