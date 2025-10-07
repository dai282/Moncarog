using UnityEngine;

public class ChestDetector : MonoBehaviour
{
    public bool isStartingChest = true;

    private bool hasBeenOpened = false;

    public void OnPlayerInteract()
    {
        if (hasBeenOpened) return;

        // Show confirmation UI
        ChestManager.Instance.ShowChestConfirmation(this);
    }

    public void OpenChest()
    {
        if (hasBeenOpened) return;

        hasBeenOpened = true;

        // Get random Moncarg from database
        GameObject randomMoncarg = MoncargDatabase.Instance.GetRandomStarterMoncarg();

        if (randomMoncarg != null)
        {
            // Add to player inventory
            AddMoncargToInventory(randomMoncarg);

            // Show success UI
            ChestManager.Instance.ShowMoncargCard(randomMoncarg);
        }

        // Disable chest visually (optional)
        GetComponent<SpriteRenderer>().color = Color.gray;
    }

    private void AddMoncargToInventory(GameObject moncargPrefab)
    {
        // Instantiate the moncarg to get StoredMoncarg component
        GameObject moncargInstance = Instantiate(moncargPrefab);
        StoredMoncarg storedMoncarg = moncargInstance.GetComponent<StoredMoncarg>();

        if (storedMoncarg != null)
        {
            // Reset moncarg data
            storedMoncarg.Details.moncargData.reset();

            // Add to inventory
            storedMoncarg.AddToInventory();

            Debug.Log($"Added {storedMoncarg.Details.FriendlyName} to inventory!");
        }

        // Destroy the instance since we only needed the component
        Destroy(moncargInstance);
    }
}