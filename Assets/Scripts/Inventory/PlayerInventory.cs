using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Assets.Scripts.Inventory; 

namespace Assets.Scripts
{
    /// <summary>
    /// Serializable container for storing item data and its UI representation
    /// </summary>
    [Serializable]
    public class StoredItem
    {
        public ItemDefinition Details;   // Reference to the item data
        public ItemVisual RootVisual;    // Reference to the visual element in the UI
    }

    /// <summary>
    /// Player inventory system that manages items and their UI in a grid layout.
    /// Works fully with Unity's UI Toolkit.
    /// </summary>
    public sealed class PlayerInventory : MonoBehaviour
    {
        // Singleton instance
        public static PlayerInventory Instance;

        // Inventory configuration
        public List<StoredItem> StoredItems = new List<StoredItem>();
        public Dimensions InventoryDimensions;

        // UI toolkit references
        private VisualElement m_Root;
        private VisualElement m_InventoryGrid;
        private VisualElement m_Telegraph; // Highlighted slot for placement preview

        // Inventory state
        private bool m_IsInventoryReady;

        // Slot dimensions (calculated from the first slot in the grid)
        public static Dimensions SlotDimension { get; private set; }

        // UI elements for item details panel
        private static Label m_ItemDetailHeader;
        private static Label m_ItemDetailBody;
        private static Label m_ItemDetailPrice;

        /// <summary>
        /// Singleton setup and initial configuration
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                Configure(); // Setup UI references and telegraph
            }
            else if (Instance != this)
            {
                Destroy(this);
            }
        }

        /// <summary>
        /// Start loading the inventory once the object is active
        /// </summary>
        private void Start()
        {
            StartCoroutine(LoadInventoryCoroutine());
        }

        /// <summary>
        /// Grab UI references, create telegraph, and calculate slot dimensions after one frame
        /// </summary>
        private void Configure()
        {
            // Get the root UI element from the UIDocument
            m_Root = GetComponentInChildren<UIDocument>().rootVisualElement;

            // Grab the grid that will hold all inventory slots/items
            m_InventoryGrid = m_Root.Q<VisualElement>("Grid");

            // Grab the item details panel elements
            VisualElement itemDetails = m_Root.Q<VisualElement>("ItemDetails");
            m_ItemDetailHeader = itemDetails.Q<Label>("Header");
            m_ItemDetailBody = itemDetails.Q<Label>("Body");
            m_ItemDetailPrice = itemDetails.Q<Label>("SellPrice");

            // Create the "telegraph" element used for showing potential placement
            ConfigureInventoryTelegraph();

            // Start coroutine to calculate slot dimensions after UI is built
            StartCoroutine(ConfigureAfterFrame());
        }

        /// <summary>
        /// Wait one frame for UI toolkit to build before calculating slot dimensions
        /// </summary>
        private IEnumerator ConfigureAfterFrame()
        {
            yield return null; // wait one frame
            ConfigureSlotDimensions();
            m_IsInventoryReady = true; // Inventory ready to be populated
        }

        /// <summary>
        /// Create and add a yellow highlight element to the inventory grid
        /// </summary>
        private void ConfigureInventoryTelegraph()
        {
            m_Telegraph = new VisualElement { name = "Telegraph" };
            m_Telegraph.AddToClassList("slot-icon-highlighted");
            AddItemToInventoryGrid(m_Telegraph);
        }

        /// <summary>
        /// Calculate single slot width and height based on the first slot in the grid
        /// </summary>
        private void ConfigureSlotDimensions()
        {
            VisualElement firstSlot = m_InventoryGrid.Children().FirstOrDefault();
            if (firstSlot != null)
            {
                SlotDimension = new Dimensions
                {
                    Width = Mathf.RoundToInt(firstSlot.worldBound.width),
                    Height = Mathf.RoundToInt(firstSlot.worldBound.height)
                };
            }
        }

        /// <summary>
        /// Add an element to the inventory grid
        /// </summary>
        private void AddItemToInventoryGrid(VisualElement item) => m_InventoryGrid.Add(item);

        /// <summary>
        /// Remove an element from the inventory grid
        /// </summary>
        private void RemoveItemFromInventoryGrid(VisualElement item) => m_InventoryGrid.Remove(item);

/// <summary>
/// Load inventory items, position them in the grid, and configure their visuals
/// </summary>
private IEnumerator LoadInventoryCoroutine()
{
    // Wait until inventory is ready (UI initialized)
    yield return new WaitUntil(() => m_IsInventoryReady);

    foreach (StoredItem storedItem in StoredItems)
    {
        // Create the visual element for this item
        ItemVisual itemVisual = new ItemVisual(storedItem.Details);
        AddItemToInventoryGrid(itemVisual);

        // Attempt to find a free slot for the item
        bool hasSpace = false;
        for (int y = 0; y < InventoryDimensions.Height && !hasSpace; y++)
        {
            for (int x = 0; x < InventoryDimensions.Width && !hasSpace; x++)
            {
                // Set the item position
                SetItemPosition(itemVisual, new Vector2(SlotDimension.Width * x, SlotDimension.Height * y));

                // Wait a frame to ensure layout updates
                yield return null;

                // Check if any other item overlaps this slot
                StoredItem overlapping = StoredItems.FirstOrDefault(
                    s => s.RootVisual != null && s.RootVisual.layout.Overlaps(itemVisual.layout)
                );

                if (overlapping == null)
                {
                    hasSpace = true;
                }
            }
        }

        // If no space is found, remove the item from the grid
        if (!hasSpace)
        {
            Debug.Log("No space - Cannot pick up the item");
            RemoveItemFromInventoryGrid(itemVisual);
            continue;
        }

        // Assign the visual to the stored item
        ConfigureInventoryItem(storedItem, itemVisual);
    }
}

/// <summary>
/// Finds a position for a new item in the inventory grid (not currently used).
/// </summary>
private IEnumerator GetPositionForItemCoroutine(VisualElement newItem, Action<bool> callback)
{
    // Wait until inventory is ready (UI initialized)
    yield return new WaitUntil(() => m_IsInventoryReady);

    // Implementation placeholder
    callback?.Invoke(true);
}

        /// <summary>
        /// Assign the visual element to the item and make it visible
        /// </summary>
        private static void ConfigureInventoryItem(StoredItem item, ItemVisual visual)
        {
            item.RootVisual = visual;
            visual.style.visibility = Visibility.Visible;
        }

        /// <summary>
        /// Update the details panel with a selected item's data
        /// </summary>
        public static void UpdateItemDetails(ItemDefinition item)
        {
            m_ItemDetailHeader.text = item.FriendlyName;
            m_ItemDetailBody.text = item.Description;
  
        }

        /// <summary>
        /// Set a visual element's position in the grid
        /// </summary>
        private static void SetItemPosition(VisualElement element, Vector2 position)
        {
            element.style.left = position.x;
            element.style.top = position.y;
        }

        /// <summary>
        /// Show placement preview for a dragged item and return whether it can be placed
        /// </summary>
        public (bool canPlace, Vector2 position) ShowPlacementTarget(ItemVisual draggedItem)
        {
            // Check if dragged item is outside the inventory grid
            if (!m_InventoryGrid.layout.Contains(new Vector2(draggedItem.localBound.xMax, draggedItem.localBound.yMax)))
            {
                m_Telegraph.style.visibility = Visibility.Hidden;
                return (false, Vector2.zero);
            }

            // Find closest overlapping slot (ignoring the dragged item itself)
            VisualElement targetSlot = m_InventoryGrid.Children()
                .Where(x => x.layout.Overlaps(draggedItem.layout) && x != draggedItem)
                .OrderBy(x => Vector2.Distance(x.worldBound.position, draggedItem.worldBound.position))
                .FirstOrDefault();

            if (targetSlot == null)
            {
                m_Telegraph.style.visibility = Visibility.Hidden;
                return (false, Vector2.zero);
            }

            // Resize telegraph to match dragged item
            m_Telegraph.style.width = draggedItem.style.width;
            m_Telegraph.style.height = draggedItem.style.height;
            SetItemPosition(m_Telegraph, targetSlot.layout.position);
            m_Telegraph.style.visibility = Visibility.Visible;

            // Check if more than one item overlaps the telegraph
            bool multipleOverlap = StoredItems.Count(s => s.RootVisual != null && s.RootVisual.layout.Overlaps(m_Telegraph.layout)) > 1;
            if (multipleOverlap)
            {
                m_Telegraph.style.visibility = Visibility.Hidden;
                return (false, Vector2.zero);
            }

            // Return valid placement
            return (true, targetSlot.worldBound.position);
        }
    }
}
