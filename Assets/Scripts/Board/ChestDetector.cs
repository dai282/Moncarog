using UnityEngine;

public class ChestDetector : MonoBehaviour
{
    public bool isStartingChest = true;

    private bool hasBeenOpened = false;

    public void OnPlayerInteract()
    {
        if (hasBeenOpened) return;

        // Show confirmation UI
        ChestManager.Instance.ShowMoncargSelection();
    }

    public void SetOpened()
    {
        hasBeenOpened = true;
        // Optional: Change chest appearance
        GetComponent<SpriteRenderer>().color = Color.gray;
    }
}