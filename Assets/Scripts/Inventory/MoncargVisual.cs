using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class MoncargVisual : VisualElement
{
    private readonly MoncargInventoryAdapter m_MoncargAdapter;
    private Vector2 m_OriginalPosition;
    private bool m_IsDragging;
    private bool m_IsMouseDown;
    private Vector2 m_MouseDownPosition;
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
        RegisterCallback<MouseDownEvent>(OnMouseDownEvent);
        RegisterCallback<MouseUpEvent>(OnMouseUpEvent);
        RegisterCallback<MouseMoveEvent>(OnMouseMoveEvent);
    }

    ~MoncargVisual()
    {
        UnregisterCallback<MouseMoveEvent>(OnMouseMoveEvent);
        UnregisterCallback<MouseUpEvent>(OnMouseUpEvent);
        UnregisterCallback<MouseDownEvent>(OnMouseDownEvent);
    }

    public void SetPosition(Vector2 pos)
    {
        style.left = pos.x;
        style.top = pos.y;
    }

    private void OnMouseDownEvent(MouseDownEvent mouseEvent)
    {
        if (mouseEvent.button == 0) // Left mouse button
        {
            m_IsMouseDown = true;
            m_MouseDownPosition = mouseEvent.localMousePosition;
            
            // Show moncarg details on mouse down
            PlayerInventory.UpdateMoncargDetails(m_MoncargAdapter);
        }
    }

    private void OnMouseUpEvent(MouseUpEvent mouseEvent)
    {
        if (mouseEvent.button == 0) // Left mouse button
        {
            if (m_IsDragging)
            {
                // End drag
                m_IsDragging = false;
                
                if (m_PlacementResults.canPlace)
                {
                    SetPosition(new Vector2(
                        m_PlacementResults.position.x - parent.worldBound.position.x,
                        m_PlacementResults.position.y - parent.worldBound.position.y));
                }
                else
                {
                    SetPosition(new Vector2(m_OriginalPosition.x, m_OriginalPosition.y));
                }
            }
            
            m_IsMouseDown = false;
        }
    }

    private void OnMouseMoveEvent(MouseMoveEvent mouseEvent)
    {
        if (m_IsMouseDown && !m_IsDragging)
        {
            // Check if mouse has moved enough to start dragging
            float dragThreshold = 5f;
            Vector2 currentMousePos = mouseEvent.localMousePosition;
            float distance = Vector2.Distance(m_MouseDownPosition, currentMousePos);
            
            if (distance > dragThreshold)
            {
                StartDrag();
            }
        }

        if (m_IsDragging)
        {
            SetPosition(GetMousePosition(mouseEvent.mousePosition));
            m_PlacementResults = PlayerInventory.Instance.ShowPlacementTarget(this);
        }
    }

    public void StartDrag()
    {
        m_IsDragging = true;
        m_OriginalPosition = worldBound.position - parent.worldBound.position;
        BringToFront();
    }

    public Vector2 GetMousePosition(Vector2 mousePosition) => 
        new Vector2(mousePosition.x - (layout.width / 2) - 
        parent.worldBound.position.x, mousePosition.y - (layout.height / 2) - 
        parent.worldBound.position.y);
}