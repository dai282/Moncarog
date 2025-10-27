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
            moncargList.fixedItemHeight = 75;

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

                    // Icon
                    var icon = new VisualElement();
                    icon.AddToClassList("moncarg-icon");
                    icon.style.backgroundImage = new StyleBackground(moncargData.Icon);

                    // Info container
                    var infoContainer = new VisualElement();
                    infoContainer.AddToClassList("info-container");

                    // Name and level
                    var nameLabel = new Label($"{moncargData.FriendlyName}");
                    nameLabel.AddToClassList("moncarg-name");

                    var levelLabel = new Label($"Lv. {moncargData.moncargData.level}");
                    levelLabel.AddToClassList("moncarg-level");

                    var nameandLevelContainer = new VisualElement();
                    infoContainer.AddToClassList("name-level-container");

                    nameandLevelContainer.Add(nameLabel);
                    nameandLevelContainer.Add(levelLabel);

                    //Type and stat container
                    var typeAndStatContainer = new VisualElement();
                    infoContainer.AddToClassList("type-stat-container");

                    // Type with appropriate color class
                    var typeLabel = new Label(moncargData.moncargData.type.ToString());
                    typeLabel.AddToClassList("moncarg-type");
                    typeLabel.AddToClassList($"type-{moncargData.moncargData.type.ToString().ToLower()}");

                    // Stats container
                    var statsContainer = new VisualElement();
                    statsContainer.AddToClassList("stats-container");

                    // Health
                    var healthContainer = new VisualElement();
                    healthContainer.AddToClassList("stat-item");

                    var healthLabel = new Label($"Health: {moncargData.moncargData.health}/{moncargData.moncargData.maxHealth}");
                    healthLabel.AddToClassList("health-text");

                    healthContainer.Add(healthLabel);

                    // Mana
                    var manaContainer = new VisualElement();
                    manaContainer.AddToClassList("stat-item");

                    var manaLabel = new Label($"Mana: {moncargData.moncargData.mana}/{moncargData.moncargData.maxMana}");
                    manaLabel.AddToClassList("mana-text");

                    manaContainer.Add(manaLabel);

                    statsContainer.Add(healthContainer);
                    statsContainer.Add(manaContainer);

                    typeAndStatContainer.Add(typeLabel);
                    typeAndStatContainer.Add(statsContainer);

                    // Build hierarchy
                    infoContainer.Add(nameandLevelContainer);
                    infoContainer.Add(typeAndStatContainer);

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