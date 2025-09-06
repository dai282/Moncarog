using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class MoncargVisual : VisualElement
{
    private readonly MoncargInventoryAdapter m_MoncargAdapter;
    private Vector2 m_OriginalPosition;
    private bool m_IsDragging;
    private (bool canPlace, Vector2 position) m_PlacementResults;

    public MoncargVisual(MoncargInventoryAdapter moncargAdapter)
    {
        m_MoncargAdapter = moncargAdapter;

        name = $"{m_MoncargAdapter.FriendlyName}";
        style.height = m_MoncargAdapter.SlotDimension.Height * 
            PlayerInventory.SlotDimension.Height;
        style.width = m_MoncargAdapter.SlotDimension.Width * 
            PlayerInventory.SlotDimension.Width;
        style.visibility = Visibility.Hidden;

        VisualElement icon = new VisualElement
        {
            style = { backgroundImage = m_MoncargAdapter.Icon.texture }
        };
        Add(icon);

        icon.AddToClassList("visual-icon");
        AddToClassList("visual-icon-container");

        // Register mouse events for drag and drop
        RegisterCallback<MouseUpEvent>(OnMouseUpEvent);
        RegisterCallback<MouseMoveEvent>(OnMouseMoveEvent);
    }

    ~MoncargVisual()
    {
        UnregisterCallback<MouseMoveEvent>(OnMouseMoveEvent);
        UnregisterCallback<MouseUpEvent>(OnMouseUpEvent);
    }

    public void SetPosition(Vector2 pos)
    {
        style.left = pos.x;
        style.top = pos.y;
    }

    private void OnMouseUpEvent(MouseUpEvent mouseEvent)
    {
        if (!m_IsDragging)
        {
            StartDrag();
            PlayerInventory.UpdateMoncargDetails(m_MoncargAdapter);
            return;
        }

        m_IsDragging = false;
                
        if (m_PlacementResults.canPlace)
        {
            SetPosition(new Vector2(
                m_PlacementResults.position.x - parent.worldBound.position.x,
                m_PlacementResults.position.y - parent.worldBound.position.y));
            return;
        }

        SetPosition(new Vector2(m_OriginalPosition.x, m_OriginalPosition.y));
    }

    public void StartDrag()
    {
        m_IsDragging = true;

        m_OriginalPosition = worldBound.position - parent.worldBound.position;
        BringToFront();
    }

    private void OnMouseMoveEvent(MouseMoveEvent mouseEvent)
    {
        if (!m_IsDragging) { return; }

        SetPosition(GetMousePosition(mouseEvent.mousePosition));
        m_PlacementResults = PlayerInventory.Instance.ShowPlacementTarget(this);
    }

    public Vector2 GetMousePosition(Vector2 mousePosition) => 
        new Vector2(mousePosition.x - (layout.width / 2) - 
        parent.worldBound.position.x, mousePosition.y - (layout.height / 2) - 
        parent.worldBound.position.y);
}