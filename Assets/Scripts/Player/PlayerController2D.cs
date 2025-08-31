using UnityEngine;

using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;        // Speed of movement
    public float gridSize = 1f;         // Size of each step (1 tile = 1 unit)
    public LayerMask obstacleLayer;     // Assign "Obstacles" layer in Inspector

    private bool isMoving = false;
    private Vector3 targetPos;

    void Start()
    {
        targetPos = transform.position; // Ensure aligned to grid
    }

    void Update()
    {
        if (!isMoving)
        {
            Vector2 input = Vector2.zero;

            // Input (WASD / Arrows)
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                input = Vector2.up;
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                input = Vector2.down;
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                input = Vector2.left;
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                input = Vector2.right;

            if (input != Vector2.zero)
            {
                // Check if the next tile is blocked
                Vector3 destination = transform.position + (Vector3)(input * gridSize);
                if (!IsBlocked(destination))
                {
                    StartCoroutine(MoveToGrid(destination));
                }
            }
        }
    }

    private IEnumerator MoveToGrid(Vector3 destination)
    {
        isMoving = true;

        while ((destination - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = destination;
        isMoving = false;
    }

    private bool IsBlocked(Vector3 targetPos)
    {
        // Cast a small box to check for collisions at the destination
        Collider2D hit = Physics2D.OverlapBox(targetPos, Vector2.one * 0.4f, 0f, obstacleLayer);
        return hit != null;
    }

    // Debug visualization in Scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(targetPos, Vector3.one * 0.4f);
    }
}
