using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public RoomGrid roomGrid;
    public float collisionOffset; // Adjust based on your sprite size

    private Rigidbody2D rb;
    private Vector2 movementDirection = Vector2.zero;
    private SpriteRenderer spriteRenderer;
    private Vector3Int lastCellPos;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Auto-calculate offset based on sprite size if not set
        if (collisionOffset <= 0 && spriteRenderer != null)
        {
            collisionOffset = spriteRenderer.bounds.extents.x;
        }

        if (roomGrid != null)
        {
            lastCellPos = roomGrid.collisionTilemap.WorldToCell(transform.position);
        }
    }

    void FixedUpdate()
    {
        if (movementDirection != Vector2.zero)
        {
            // Calculate target position with offset checking
            Vector2 targetPosition = rb.position + movementDirection * moveSpeed * Time.fixedDeltaTime;

            // Check multiple points around the player for better collision
            if (IsPositionWalkable(targetPosition))
            {
                // Move to the target position
                rb.MovePosition(targetPosition);

                Vector3Int currentCellPos = roomGrid.collisionTilemap.WorldToCell(rb.position);
                if (currentCellPos != lastCellPos)
                {
                    StatsCollector.Instance?.RecordStep();
                    lastCellPos = currentCellPos;
                }
            }
        }
    }

    bool IsPositionWalkable(Vector2 position)
    {
        // Convert to cell position
        Vector3Int cellPos = roomGrid.collisionTilemap.WorldToCell(position);

        DoorDetector door = roomGrid.GetDoorAtCell(cellPos);
        if (door != null)
        {
            // trigger the door teleport
            door.OnPlayerEnter();

            // Return false so the player doesn’t "walk into" the door tile physically
            return false;
        }

        // Check center point
        if (!roomGrid.IsWalkable(position))
        {
            return false;
        }

        if (roomGrid.IsEncounterTile(position, out Vector3Int encounterCell))
        {
            Debug.Log($"Encounter triggered at {encounterCell}");

            // Trigger combat
            FindFirstObjectByType<CombatHandler>().BeginEncounter(roomGrid.roomGridID);

            // Reset tile to walkable after use
            roomGrid.ResetEncounterTile(encounterCell);
        }

        // Check edges based on movement direction
        if (movementDirection.x > 0) // Moving right
        {
            if (!roomGrid.IsWalkable(position + Vector2.right * collisionOffset))
                return false;
        }
        else if (movementDirection.x < 0) // Moving left
        {
            if (!roomGrid.IsWalkable(position + Vector2.left * collisionOffset))
                return false;
        }

        if (movementDirection.y > 0) // Moving up
        {
            if (!roomGrid.IsWalkable(position + Vector2.up * collisionOffset))
                return false;
        }
        else if (movementDirection.y < 0) // Moving down
        {
            if (!roomGrid.IsWalkable(position + Vector2.down * (collisionOffset + 0.2f)))
                return false;
        }

        //Debug.Log($"Player entered cell {cellPos}");
        return true;
    }

    // Movement methods for buttons (keep these the same)
    public void MoveUp() { movementDirection = Vector2.up; }
    public void MoveDown() { movementDirection = Vector2.down; }
    public void MoveLeft() { movementDirection = Vector2.left; }
    public void MoveRight() { movementDirection = Vector2.right; }
    public void StopMovement() { movementDirection = Vector2.zero; }
}