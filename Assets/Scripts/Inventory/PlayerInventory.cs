using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class PlayerInventory : MonoBehaviour
{
    #region Static Instance
    public static PlayerInventory Instance;
    #endregion

    #region Inventory Data
    [Header("Inventory Content")]
    public List<StoredItem> StoredItems = new List<StoredItem>();
    public List<StoredMoncarg> StoredMoncargs = new List<StoredMoncarg>();
    
    [Header("Grid Settings")]
    public Dimensions InventoryDimensions = new Dimensions { Width = 8, Height = 8 };
    
    [Header("Weight Settings")]
    public int MaxWeight = 54;
    
    public static Dimensions SlotDimension { get; private set; }
    #endregion

    #region UI Elements
    private VisualElement m_Root;
    private VisualElement m_InventoryGrid;
    private VisualElement m_Telegraph;
    
    private Button m_MoncargButton;
    private Button m_ItemsButton;
    private Button m_EquipButton;
    private Button m_DropButton;
    private Label m_MoncargEquippedLabel;
    private Label m_WeightLabel;
    
    private static Label m_ItemDetailHeader;
    private static Label m_ItemDetailBody;
    #endregion

    #region State Management
    private enum InventoryMode { Items, Moncargs }
    private InventoryMode m_CurrentMode = InventoryMode.Items;
    private bool m_IsInventoryReady = false;
    
    private ItemDefinition m_CurrentSelectedItem;
    private MoncargInventoryAdapter m_CurrentSelectedMoncarg;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            StartCoroutine(InitializeInventory());
        }
        else if (Instance != this)
        {
            Destroy(this);
        }
    }

    private void Update()
    {
        // Check for 'I' key press to toggle inventory using new Input System
        if (UnityEngine.InputSystem.Keyboard.current.iKey.wasPressedThisFrame)
        {
            ToggleInventory();
        }
    }
    #endregion

    #region Initialization
    private IEnumerator InitializeInventory()
    {
        yield return StartCoroutine(SetupUIElements());
        yield return StartCoroutine(ConfigureInventorySettings());
        
        // Wait until inventory is fully ready before loading content
        yield return new WaitUntil(() => m_IsInventoryReady);
        
        LoadCurrentInventoryMode();
        UpdateMoncargEquippedCount();
        UpdateWeightDisplay();
        ClearSelection();
        
        Debug.Log("Inventory initialization complete");
    }

    private IEnumerator SetupUIElements()
    {
        // Get root UI element
        m_Root = GetComponentInChildren<UIDocument>().rootVisualElement;
        if (m_Root == null)
        {
            Debug.LogError("No UIDocument found!");
            yield break;
        }

        // Find grid container
        m_InventoryGrid = m_Root.Q<VisualElement>("Grid");
        if (m_InventoryGrid == null)
        {
            Debug.LogError("Grid element not found in UXML!");
            yield break;
        }

        // Setup buttons
        SetupNavigationButtons();
        
        // Setup item details panel
        SetupItemDetailsPanel();
        
        // Setup weight display
        SetupWeightDisplay();
        
        // Configure telegraph for drag preview
        ConfigureInventoryTelegraph();
        
        Debug.Log($"UI setup complete. Grid has {m_InventoryGrid.Children().Count()} slots");
    }

    private void SetupNavigationButtons()
    {
        var buttonContainer = m_Root.Q<VisualElement>("Container_Level");
        if (buttonContainer != null)
        {
            var buttons = buttonContainer.Children().OfType<Button>().ToArray();
            if (buttons.Length >= 2)
            {
                m_MoncargButton = buttons[0]; // MONCARG button
                m_ItemsButton = buttons[1];   // Items button

                m_MoncargButton.clicked += SwitchToMoncargMode;
                m_ItemsButton.clicked += SwitchToItemsMode;

                // Set initial button states
                SetActiveButton(m_ItemsButton);
            }
        }

        // Find equipped Moncarg counter
        var moncargEquip = m_Root.Q<VisualElement>("MoncargEquip");
        m_MoncargEquippedLabel = moncargEquip?.Q<Label>("Value");
    }

    private void SetupItemDetailsPanel()
    {
        var itemDetails = m_Root.Q<VisualElement>("ItemDetails");
        if (itemDetails != null)
        {
            m_ItemDetailHeader = itemDetails.Q<Label>("FriendlyName");
            m_ItemDetailBody = itemDetails.Q<Label>("Description");
            
            // Setup buttons
            var buttonContainer = itemDetails.Q<VisualElement>("Container_Buttons");
            if (buttonContainer != null)
            {
                m_EquipButton = buttonContainer.Q<Button>("btn_Equip");
                m_DropButton = buttonContainer.Q<Button>("btn_Drop");
                
                if (m_EquipButton != null) m_EquipButton.clicked += OnEquipButtonClicked;
                if (m_DropButton != null) m_DropButton.clicked += OnDropButtonClicked;
            }
        }
    }

    private void SetupWeightDisplay()
    {
        var weightContainer = m_Root.Q<VisualElement>("Container_Weight");
        m_WeightLabel = weightContainer?.Q<Label>("Value");
    }

    private IEnumerator ConfigureInventorySettings()
    {
        yield return null; // Wait for UI layout
        
        // Configure slot dimensions with fallback
        SlotDimension = GetSlotDimensions();
        m_IsInventoryReady = true;
        
        Debug.Log($"Slot dimensions: {SlotDimension.Width}x{SlotDimension.Height}");
    }

    private Dimensions GetSlotDimensions()
    {
        var firstSlot = m_InventoryGrid.Children().FirstOrDefault();
        if (firstSlot != null && !float.IsNaN(firstSlot.worldBound.width))
        {
            return new Dimensions
            {
                Width = Mathf.RoundToInt(firstSlot.worldBound.width),
                Height = Mathf.RoundToInt(firstSlot.worldBound.height)
            };
        }
        
        // Fallback to UXML defined size
        return new Dimensions { Width = 150, Height = 150 };
    }
    #endregion

    #region Public Interface
    public void ToggleInventory()
    {
        if (m_Root != null)
        {
            bool isVisible = m_Root.style.display == DisplayStyle.Flex;
            m_Root.style.display = isVisible ? DisplayStyle.None : DisplayStyle.Flex;
            Debug.Log($"Inventory {(isVisible ? "hidden" : "shown")}");
        }
    }

    public void ShowInventory()
    {
        if (m_Root != null)
        {
            m_Root.style.display = DisplayStyle.Flex;
        }
    }

    public void HideInventory()
    {
        if (m_Root != null)
        {
            m_Root.style.display = DisplayStyle.None;
        }
    }

    public bool IsInventoryVisible()
    {
        return m_Root != null && m_Root.style.display == DisplayStyle.Flex;
    }
    #endregion

    #region Button Actions
    private void OnEquipButtonClicked()
    {
        if (m_CurrentMode == InventoryMode.Items && m_CurrentSelectedItem != null)
        {
            Debug.Log($"Equipping item: {m_CurrentSelectedItem.FriendlyName}");
            // Add your item equipping logic here
        }
        else if (m_CurrentMode == InventoryMode.Moncargs && m_CurrentSelectedMoncarg != null)
        {
            // Check if we can equip (max 3)
            int currentEquipped = StoredMoncargs.Count(m => m?.Details != null && m.Details.IsEquipped);
            
            if (!m_CurrentSelectedMoncarg.IsEquipped && currentEquipped >= 3)
            {
                Debug.Log("Cannot equip - maximum 3 Moncargs allowed");
                return;
            }
            
            m_CurrentSelectedMoncarg.IsEquipped = !m_CurrentSelectedMoncarg.IsEquipped;
            UpdateMoncargEquippedCount();
            
            string action = m_CurrentSelectedMoncarg.IsEquipped ? "Equipped" : "Unequipped";
            Debug.Log($"{action} Moncarg: {m_CurrentSelectedMoncarg.FriendlyName}");
        }
    }

    private void OnDropButtonClicked()
    {
        if (m_CurrentMode == InventoryMode.Items && m_CurrentSelectedItem != null)
        {
            var itemToRemove = StoredItems.FirstOrDefault(x => x.Details == m_CurrentSelectedItem);
            if (itemToRemove != null)
            {
                if (itemToRemove.RootVisual != null)
                {
                    itemToRemove.RootVisual.RemoveFromHierarchy();
                }
                StoredItems.Remove(itemToRemove);
                UpdateWeightDisplay();
                Debug.Log($"Dropped item: {m_CurrentSelectedItem.FriendlyName}");
            }
        }
        else if (m_CurrentMode == InventoryMode.Moncargs && m_CurrentSelectedMoncarg != null)
        {
            if (m_CurrentSelectedMoncarg.IsEquipped)
            {
                // If equipped, just unequip it
                m_CurrentSelectedMoncarg.IsEquipped = false;
                UpdateMoncargEquippedCount();
                Debug.Log($"Unequipped Moncarg: {m_CurrentSelectedMoncarg.FriendlyName}");
            }
            else
            {
                // If not equipped, remove it completely
                var moncargToRemove = StoredMoncargs.FirstOrDefault(x => x.Details == m_CurrentSelectedMoncarg);
                if (moncargToRemove != null)
                {
                    if (moncargToRemove.RootVisual != null)
                    {
                        moncargToRemove.RootVisual.RemoveFromHierarchy();
                    }
                    StoredMoncargs.Remove(moncargToRemove);
                    UpdateWeightDisplay();
                    Debug.Log($"Released Moncarg: {m_CurrentSelectedMoncarg.FriendlyName}");
                }
            }
        }
        
        // Clear selection and hide details
        ClearSelection();
    }

    private void ClearSelection()
    {
        m_CurrentSelectedItem = null;
        m_CurrentSelectedMoncarg = null;
        
        if (m_ItemDetailHeader != null) m_ItemDetailHeader.text = "Select an item";
        if (m_ItemDetailBody != null) m_ItemDetailBody.text = "Click on an item to see details";
    }
    #endregion

    #region Inventory Mode Switching
    private void SwitchToItemsMode()
    {
        Debug.Log("Switching to Items mode");
        m_CurrentMode = InventoryMode.Items;
        SetActiveButton(m_ItemsButton);
        RefreshInventoryDisplay();
        UpdateWeightDisplay();
        ClearSelection();
    }

    private void SwitchToMoncargMode()
    {
        Debug.Log("Switching to Moncargs mode");
        m_CurrentMode = InventoryMode.Moncargs;
        SetActiveButton(m_MoncargButton);
        RefreshInventoryDisplay();
        UpdateWeightDisplay();
        ClearSelection();
    }

    private void SetActiveButton(Button activeButton)
    {
        // Reset all button colors
        var normalColor = new Color(226f/255f, 137f/255f, 45f/255f);
        var activeColor = Color.yellow;
        
        if (m_ItemsButton != null) m_ItemsButton.style.backgroundColor = normalColor;
        if (m_MoncargButton != null) m_MoncargButton.style.backgroundColor = normalColor;
        
        // Highlight active button
        if (activeButton != null) activeButton.style.backgroundColor = activeColor;
    }

    private void RefreshInventoryDisplay()
    {
        ClearGrid();
        LoadCurrentInventoryMode();
    }
    #endregion

    #region Inventory Loading
    private void LoadCurrentInventoryMode()
    {
        switch (m_CurrentMode)
        {
            case InventoryMode.Items:
                LoadItems();
                break;
            case InventoryMode.Moncargs:
                LoadMoncargs();
                break;
        }
    }

    private void LoadItems()
    {
        for (int i = 0; i < StoredItems.Count; i++)
        {
            var storedItem = StoredItems[i];
            if (storedItem?.Details == null) continue;

            var itemVisual = new ItemVisual(storedItem.Details);
            m_InventoryGrid.Add(itemVisual);
            
            PositionItemInGrid(itemVisual, i);
            itemVisual.style.visibility = Visibility.Visible;
            
            storedItem.RootVisual = itemVisual;
        }
    }

    private void LoadMoncargs()
    {
        for (int i = 0; i < StoredMoncargs.Count; i++)
        {
            var storedMoncarg = StoredMoncargs[i];
            if (storedMoncarg?.Details == null) continue;

            var moncargVisual = new MoncargVisual(storedMoncarg.Details);
            m_InventoryGrid.Add(moncargVisual);
            
            PositionItemInGrid(moncargVisual, i);
            moncargVisual.style.visibility = Visibility.Visible;
            
            storedMoncarg.RootVisual = moncargVisual;
        }
    }

    private void PositionItemInGrid(VisualElement item, int index)
    {
        int column = index % InventoryDimensions.Width;
        int row = index / InventoryDimensions.Width;
        
        var position = new Vector2(
            column * SlotDimension.Width, 
            row * SlotDimension.Height
        );
        
        item.style.left = position.x;
        item.style.top = position.y;
    }

    private void ClearGrid()
    {
        var itemsToRemove = m_InventoryGrid.Children()
            .Where(child => child.name != "SlotIcon" && child.name != "Telegraph")
            .ToList();
        
        foreach (var item in itemsToRemove)
        {
            m_InventoryGrid.Remove(item);
        }
    }
    #endregion

    #region Weight Management
    private void UpdateWeightDisplay()
    {
        if (m_WeightLabel != null)
        {
            int currentWeight = CalculateCurrentWeight();
            m_WeightLabel.text = $"{currentWeight}/{MaxWeight}";
        }
    }

    private int CalculateCurrentWeight()
    {
        int totalWeight = 0;
        
        if (m_CurrentMode == InventoryMode.Items)
        {
            // Count items - you can add weight property to ItemDefinition if needed
            totalWeight = StoredItems.Count(x => x?.Details != null);
        }
        else
        {
            // Count moncargs
            totalWeight = StoredMoncargs.Count(x => x?.Details != null);
        }
        
        return totalWeight;
    }
    #endregion

    #region Telegraph System (Drag Preview)
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
        m_InventoryGrid.Add(m_Telegraph);
    }

    public (bool canPlace, Vector2 position) ShowPlacementTarget(VisualElement draggedItem)
    {
        if (!IsValidDragOperation(draggedItem))
        {
            HideTelegraph();
            return (false, Vector2.zero);
        }

        var targetSlot = FindNearestSlot(draggedItem);
        if (targetSlot == null)
        {
            HideTelegraph();
            return (false, Vector2.zero);
        }

        ShowTelegraphAt(targetSlot, draggedItem);

        if (HasOverlappingItems(targetSlot))
        {
            HideTelegraph();
            return (false, Vector2.zero);
        }

        return (true, targetSlot.worldBound.position);
    }

    private bool IsValidDragOperation(VisualElement draggedItem)
    {
        return draggedItem != null && 
               m_InventoryGrid != null && 
               m_Telegraph != null &&
               m_InventoryGrid.layout.Contains(new Vector2(draggedItem.localBound.xMax, draggedItem.localBound.yMax));
    }

    private VisualElement FindNearestSlot(VisualElement draggedItem)
    {
        return m_InventoryGrid.Children()
            .Where(x => x != null && 
                       x.name == "SlotIcon" && 
                       x.layout.Overlaps(draggedItem.layout) && 
                       x != draggedItem)
            .OrderBy(x => Vector2.Distance(x.worldBound.position, draggedItem.worldBound.position))
            .FirstOrDefault();
    }

    private void ShowTelegraphAt(VisualElement targetSlot, VisualElement draggedItem)
    {
        m_Telegraph.style.width = draggedItem.style.width;
        m_Telegraph.style.height = draggedItem.style.height;
        m_Telegraph.style.left = targetSlot.layout.position.x;
        m_Telegraph.style.top = targetSlot.layout.position.y;
        m_Telegraph.style.visibility = Visibility.Visible;
    }

    private void HideTelegraph()
    {
        if (m_Telegraph != null)
        {
            m_Telegraph.style.visibility = Visibility.Hidden;
        }
    }

    private bool HasOverlappingItems(VisualElement targetSlot)
    {
        if (m_CurrentMode == InventoryMode.Items)
        {
            return StoredItems.Count(x => x?.RootVisual != null && 
                                         x.RootVisual.layout.Overlaps(m_Telegraph.layout)) > 1;
        }
        else
        {
            return StoredMoncargs.Count(x => x?.RootVisual != null && 
                                           x.RootVisual.layout.Overlaps(m_Telegraph.layout)) > 1;
        }
    }
    #endregion

    #region Item Details Display
    public static void UpdateItemDetails(ItemDefinition item)
    {
        Instance.m_CurrentSelectedItem = item;
        Instance.m_CurrentSelectedMoncarg = null;
        
        if (m_ItemDetailHeader != null) m_ItemDetailHeader.text = item.FriendlyName;
        if (m_ItemDetailBody != null) m_ItemDetailBody.text = item.Description;
    }

    public static void UpdateMoncargDetails(MoncargInventoryAdapter moncargAdapter)
    {
        Instance.m_CurrentSelectedMoncarg = moncargAdapter;
        Instance.m_CurrentSelectedItem = null;
        
        if (m_ItemDetailHeader != null) m_ItemDetailHeader.text = moncargAdapter.FriendlyName;
        if (m_ItemDetailBody != null) m_ItemDetailBody.text = moncargAdapter.Description;
    }
    #endregion

    #region Moncarg Management
    private void UpdateMoncargEquippedCount()
    {
        int equippedCount = StoredMoncargs.Count(m => m?.Details != null && m.Details.IsEquipped);
        if (m_MoncargEquippedLabel != null)
        {
            m_MoncargEquippedLabel.text = $"{equippedCount}/3";
        }
    }

    public GameObject SpawnMoncargFromInventory(MoncargInventoryAdapter adapter)
    {
        return adapter?.CreateMoncargGameObject();
    }
    #endregion

    #region Utility Methods
    private void AddItemToInventoryGrid(VisualElement item) => m_InventoryGrid.Add(item);
    private void RemoveItemFromInventoryGrid(VisualElement item) => m_InventoryGrid.Remove(item);
    #endregion
}