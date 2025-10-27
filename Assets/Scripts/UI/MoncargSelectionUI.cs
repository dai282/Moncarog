using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using Elementals;

public class MoncargSelectionUI : MonoBehaviour
{
    [SerializeField] private UIDocument selectionUI;
    [SerializeField] private StyleSheet selectionUIStyleSheet;
    private VisualElement root;
    private ListView moncargList;
    private Button confirmButton;
    private Button cancelButton;

    private List<MoncargInventoryAdapter> equippedMoncargs = new List<MoncargInventoryAdapter>();
    private MoncargInventoryAdapter selectedMoncarg;

    private bool isEncounterStart = false;

    public System.Action<MoncargInventoryAdapter> OnMoncargSelected;
    public System.Action OnSelectionCancelled;

    private void Awake()
    {
        if (selectionUI != null)
        {
            root = selectionUI.rootVisualElement;

            if (selectionUIStyleSheet != null)
            {
                root.styleSheets.Add(selectionUIStyleSheet);
            }

            root.style.display = DisplayStyle.None;

            moncargList = root.Q<ListView>("MoncargList");
            confirmButton = root.Q<Button>("ConfirmButton");
            cancelButton = root.Q<Button>("CancelButton");

            if (confirmButton != null) confirmButton.clicked += OnConfirm;
            if (cancelButton != null) cancelButton.clicked += OnCancel;

            SetupListView();
        }
        else
        {
            Debug.LogError("Selection UI Document is not assigned in the inspector!");
        }
    }

    private void OnSelectionChanged(IEnumerable<object> selectedItems)
    {
        var selected = selectedItems.FirstOrDefault();
        if (selected != null)
        {
            int index = moncargList.selectedIndex;
            if (index >= 0 && index < equippedMoncargs.Count)
            {
                selectedMoncarg = equippedMoncargs[index];
                confirmButton.SetEnabled(true);
            }
        }
    }

    private void OnConfirm()
    {
        if (selectedMoncarg != null)
        {
            OnMoncargSelected?.Invoke(selectedMoncarg);
            Hide();
        }
    }

    private void OnCancel()
    {
        OnSelectionCancelled?.Invoke();
        Hide();
    }

    public void Show(List<MoncargInventoryAdapter> equippedMoncargs, bool isEncounterStart = false)
    {
        this.equippedMoncargs = equippedMoncargs;
        this.isEncounterStart = isEncounterStart; // Store the context
        selectedMoncarg = null;

        if (moncargList != null)
        {
            moncargList.itemsSource = equippedMoncargs;
            moncargList.Rebuild();
        }

        confirmButton.SetEnabled(false);
        root.style.display = DisplayStyle.Flex;
    }

    public void Hide()
    {
        root.style.display = DisplayStyle.None;
    }

    private void SetupListView()
    {
        if (moncargList != null)
        {
            moncargList.fixedItemHeight = 150;

            moncargList.makeItem = () =>
            {
                var entry = new VisualElement();
                entry.AddToClassList("moncarg-item");
                return entry;
            };

            moncargList.bindItem = (element, index) =>
            {
                if (index < equippedMoncargs.Count)
                {
                    var moncargData = equippedMoncargs[index];
                    element.Clear();

                    // Root layout for this Moncarg
                    element.style.flexDirection = FlexDirection.Row;
                    element.style.justifyContent = Justify.FlexStart;
                    element.style.alignItems = Align.Center;

                    // === ICON ===
                    var icon = new VisualElement();
                    icon.AddToClassList("moncarg-icon");
                    icon.style.backgroundImage = new StyleBackground(moncargData.Icon);

                    // === INFO CONTAINER (two columns) ===
                    var infoContainer = new VisualElement();
                    infoContainer.AddToClassList("info-container");
                    infoContainer.style.flexDirection = FlexDirection.Row;
                    infoContainer.style.justifyContent = Justify.SpaceBetween;
                    infoContainer.style.alignItems = Align.FlexStart;
                    infoContainer.style.flexGrow = 1;

                    // LEFT COLUMN
                    var leftColumn = new VisualElement();
                    leftColumn.AddToClassList("info-left");
                    leftColumn.style.flexDirection = FlexDirection.Column;

                    // Line 1: Name + Type
                    var nameTypeRow = new VisualElement();
                    nameTypeRow.style.flexDirection = FlexDirection.Row;
                    nameTypeRow.style.alignItems = Align.Center;

                    var nameLabel = new Label(moncargData.FriendlyName);
                    nameLabel.AddToClassList("moncarg-name");

                    var typeLabel = new Label(moncargData.moncargData.type.ToString());
                    typeLabel.AddToClassList("moncarg-type");
                    typeLabel.AddToClassList($"type-{moncargData.moncargData.type.ToString().ToLower()}");

                    nameTypeRow.Add(nameLabel);
                    nameTypeRow.Add(typeLabel);

                    // Line 2: Level
                    var levelLabel = new Label($"Level {moncargData.moncargData.level}");
                    levelLabel.AddToClassList("moncarg-level");

                    leftColumn.Add(nameTypeRow);
                    leftColumn.Add(levelLabel);

                    // RIGHT COLUMN
                    var rightColumn = new VisualElement();
                    rightColumn.AddToClassList("info-right");
                    rightColumn.style.flexDirection = FlexDirection.Column;
                    rightColumn.style.alignItems = Align.FlexEnd;

                    // Line 1: Health
                    var healthLabel = new Label($"Health: {moncargData.moncargData.health}/{moncargData.moncargData.maxHealth}");
                    healthLabel.AddToClassList("health-text");

                    // Line 2: Mana
                    var manaLabel = new Label($"Mana: {moncargData.moncargData.mana}/{moncargData.moncargData.maxMana}");
                    manaLabel.AddToClassList("mana-text");

                    rightColumn.Add(healthLabel);
                    rightColumn.Add(manaLabel);

                    // Build info container
                    infoContainer.Add(leftColumn);
                    infoContainer.Add(rightColumn);

                    // Build hierarchy
                    element.Add(icon);
                    element.Add(infoContainer);


                    // Build hierarchy
                    element.Add(icon);
                    element.Add(infoContainer);
                }
            };


            moncargList.selectionChanged += OnSelectionChanged;
        }
    }

    private Color GetTypeColor(ElementalType type)
    {
        switch (type)
        {
            case ElementalType.Fire: return new Color(1f, 0.4f, 0.2f);
            case ElementalType.Water: return new Color(0.2f, 0.6f, 1f);
            case ElementalType.Plant: return new Color(0.4f, 0.8f, 0.2f);
            default: return Color.white;
        }
    }

    private void OnEntryMouseEnter(VisualElement element)
    {
        element.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
    }

    private void OnEntryMouseLeave(VisualElement element)
    {
        element.style.backgroundColor = new StyleColor(StyleKeyword.Null);
    }

    // ADD THIS: Public getter so CombatHandler can check the context
    public bool IsEncounterStart()
    {
        return isEncounterStart;
    }
}