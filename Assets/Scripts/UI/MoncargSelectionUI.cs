using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

public class MoncargSelectionUI : MonoBehaviour
{
    [SerializeField] private UIDocument selectionUI;
    private VisualElement root;
    private ListView moncargList;
    private Button confirmButton;
    private Button cancelButton;

    private List<MoncargInventoryAdapter> equippedMoncargs = new List<MoncargInventoryAdapter>();
    private MoncargInventoryAdapter selectedMoncarg;

    public System.Action<MoncargInventoryAdapter> OnMoncargSelected;
    public System.Action OnSelectionCancelled;

    private void Awake()
    {
        if (selectionUI != null)
        {
            root = selectionUI.rootVisualElement;
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

    private void SetupListView()
    {
        if (moncargList != null)
        {
            moncargList.makeItem = () => new Label();
            moncargList.bindItem = (element, index) => {
                if (element is Label label && index < equippedMoncargs.Count)
                {
                    label.text = equippedMoncargs[index].FriendlyName;
                }
            };

            moncargList.onSelectionChange += OnSelectionChanged;
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

    public void Show(List<MoncargInventoryAdapter> equippedMoncargs)
    {
        this.equippedMoncargs = equippedMoncargs;
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
}