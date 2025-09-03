using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance;

    private VisualElement m_Root;
    private VisualElement m_InventoryGrid;

    private static Label m_ItemDetailHeader;
    private static Label m_ItemDetailBody;
    private static Label m_ItemDetailPrice;
    private bool m_IsInventoryReady;
    private VisualElement m_Telegraph;

    public List<StoredItem> StoredItems = new List<StoredItem>();
    public Dimensions InventoryDimensions;
    
    public static Dimensions SlotDimension { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Configure();
        }
        else if (Instance != this)
        {
            Destroy(this);
        }
    }

    private void Configure()
    {
        StartCoroutine(ConfigureCoroutine());
    }

    private IEnumerator ConfigureCoroutine()
    {
        m_Root = GetComponentInChildren<UIDocument>().rootVisualElement;
        
        if (m_Root == null)
        {
            Debug.LogError("No UIDocument found or rootVisualElement is null!");
            yield break;
        }
        
        m_InventoryGrid = m_Root.Q<VisualElement>("Grid");
        
        if (m_InventoryGrid == null)
        {
            Debug.LogError("Could not find Grid element in UXML!");
            yield break;
        }
        
        Debug.Log($"Found Grid element with {m_InventoryGrid.Children().Count()} children");

        VisualElement itemDetails = m_Root.Q<VisualElement>("ItemDetails");
        
        if (itemDetails != null)
        {
            m_ItemDetailHeader = itemDetails.Q<Label>("FriendlyName");
            m_ItemDetailBody = itemDetails.Q<Label>("Description");
        }
        else
        {
            Debug.LogWarning("ItemDetails element not found");
        }

        yield return null; // Wait one frame

        ConfigureSlotDimensions();

        m_IsInventoryReady = true;
        Debug.Log("Inventory configuration complete");
    }

    private void ConfigureSlotDimensions()
    {
        try
        {
            Debug.Log("Starting ConfigureSlotDimensions");
            
            var children = m_InventoryGrid.Children();
            Debug.Log($"Grid has {children.Count()} children");
            
            VisualElement firstSlot = children.FirstOrDefault();
            
            if (firstSlot == null)
            {
                Debug.LogError("No children found in Grid!");
                SlotDimension = new Dimensions { Width = 150, Height = 150 }; // Fallback values
                return;
            }
            
            // Force layout update
            m_InventoryGrid.MarkDirtyRepaint();
            
            Debug.Log($"First slot world bound: {firstSlot.worldBound}");
            
            // If worldBound is still NaN, use the fixed size from your UXML (150x150)
            float width = float.IsNaN(firstSlot.worldBound.width) ? 150f : firstSlot.worldBound.width;
            float height = float.IsNaN(firstSlot.worldBound.height) ? 150f : firstSlot.worldBound.height;

            SlotDimension = new Dimensions
            {
                Width = Mathf.RoundToInt(width),
                Height = Mathf.RoundToInt(height)
            };
            
            Debug.Log($"Configured slot dimensions: {SlotDimension.Width}x{SlotDimension.Height}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in ConfigureSlotDimensions: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
            SlotDimension = new Dimensions { Width = 150, Height = 150 }; // Fallback values
        }
    }

    private void Start() 
    {
        StartCoroutine(LoadInventoryCoroutine());
    }

    private IEnumerator LoadInventoryCoroutine()
    {
        // Wait until inventory is ready
        yield return new WaitUntil(() => m_IsInventoryReady);

        foreach (StoredItem loadedItem in StoredItems)
        {
            ItemVisual inventoryItemVisual = new ItemVisual(loadedItem.Details);
                    
            AddItemToInventoryGrid(inventoryItemVisual);

            yield return StartCoroutine(GetPositionForItemCoroutine(inventoryItemVisual, loadedItem));
        }
    }

    private IEnumerator GetPositionForItemCoroutine(VisualElement newItem, StoredItem loadedItem)
    {
        for (int y = 0; y < InventoryDimensions.Height; y++)
        {
            for (int x = 0; x < InventoryDimensions.Width; x++)
            {
                //try position
                SetItemPosition(newItem, new Vector2(SlotDimension.Width * x, 
                    SlotDimension.Height * y));

                yield return null; // Wait one frame

                StoredItem overlappingItem = StoredItems.FirstOrDefault(s => 
                    s.RootVisual != null && 
                    s.RootVisual.layout.Overlaps(newItem.layout));

                //Nothing is here! Place the item.
                if (overlappingItem == null)
                {
                    ConfigureInventoryItem(loadedItem, newItem as ItemVisual);
                    yield break; // Found space, exit coroutine
                }
            }
        }
        
        // No space found
        Debug.Log("No space - Cannot pick up the item");
        RemoveItemFromInventoryGrid(newItem);
    }

    private static void SetItemPosition(VisualElement element, Vector2 vector)
    {
        element.style.left = vector.x;
        element.style.top = vector.y;
    }

    private void AddItemToInventoryGrid(VisualElement item) => m_InventoryGrid.Add(item);
    
    private void RemoveItemFromInventoryGrid(VisualElement item) => m_InventoryGrid.Remove(item);

    private void ConfigureInventoryTelegraph()
    {
        m_Telegraph = new VisualElement
        {
            name = "Telegraph",
            style =
            {
                position = Position.Absolute,
                visibility = Visibility.Hidden
            }
        };

        m_Telegraph.AddToClassList("slot-icon-highlighted");
        AddItemToInventoryGrid(m_Telegraph);
    }

    public (bool canPlace, Vector2 position) ShowPlacementTarget(ItemVisual draggedItem)
    {
        if (draggedItem == null || m_InventoryGrid == null || m_Telegraph == null)
        {
            return (canPlace: false, position: Vector2.zero);
        }

        if (!m_InventoryGrid.layout.Contains(new Vector2(draggedItem.localBound.xMax,
            draggedItem.localBound.yMax)))
        {
            m_Telegraph.style.visibility = Visibility.Hidden;
            return (canPlace: false, position: Vector2.zero);
        }

        VisualElement targetSlot = m_InventoryGrid.Children().Where(x => 
            x != null && x.layout.Overlaps(draggedItem.layout) && x != draggedItem).OrderBy(x => 
            Vector2.Distance(x.worldBound.position, 
            draggedItem.worldBound.position)).FirstOrDefault();

        if (targetSlot == null)
        {
            m_Telegraph.style.visibility = Visibility.Hidden;
            return (canPlace: false, position: Vector2.zero);
        }

        m_Telegraph.style.width = draggedItem.style.width;
        m_Telegraph.style.height = draggedItem.style.height;

        SetItemPosition(m_Telegraph, new Vector2(targetSlot.layout.position.x,
            targetSlot.layout.position.y));

        m_Telegraph.style.visibility = Visibility.Visible;

        var overlappingItems = StoredItems.Where(x => x != null && x.RootVisual != null && 
            x.RootVisual.layout.Overlaps(m_Telegraph.layout)).ToArray();

        if (overlappingItems.Length > 1)
        {
            m_Telegraph.style.visibility = Visibility.Hidden;
            return (canPlace: false, position: Vector2.zero);
        }

        return (canPlace: true, targetSlot.worldBound.position);
    }

    public static void UpdateItemDetails(ItemDefinition item)
    {
        if (m_ItemDetailHeader != null) m_ItemDetailHeader.text = item.FriendlyName;
        if (m_ItemDetailBody != null) m_ItemDetailBody.text = item.Description;
    }

    private static void ConfigureInventoryItem(StoredItem item, ItemVisual visual)
    {
        item.RootVisual = visual;
        visual.style.visibility = Visibility.Visible;
    }
}