using UnityEngine;
using UnityEngine.UI;

public class InventoryButton : MonoBehaviour
{
    [SerializeField] private Button inventoryButton;
    
    private void Start()
    {
        if (inventoryButton == null)
            inventoryButton = GetComponent<Button>();
            
        if (inventoryButton != null)
            inventoryButton.onClick.AddListener(OpenInventory);
    }
    
    private void OpenInventory()
    {
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.ShowInventory();
        }
    }
    
    private void OnDestroy()
    {
        if (inventoryButton != null)
            inventoryButton.onClick.RemoveListener(OpenInventory);
    }
}