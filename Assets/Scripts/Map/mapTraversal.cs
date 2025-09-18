using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MapTraversalOverlay : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject playerIconPrefab;
    public GameObject visitedOverlayPrefab; // e.g. a gray circle sprite

    [Header("Colors")]
    public Color currentColor = Color.green;
    public Color visitedColor = Color.gray;

    private MapGenerator.MapNode currentNode;
    private HashSet<MapGenerator.MapNode> visitedNodes = new HashSet<MapGenerator.MapNode>();

    private GameObject playerIcon;

    private Vector3 mapOffset;


    public void Initialize(MapGenerator.MapNode startNode, Vector3 mapOffset)
{
    // Store the offset for later use in the Move method
    this.mapOffset = mapOffset;
    currentNode = startNode;
    visitedNodes.Clear();

    // Change this line to apply the offset to the starting position
    if (playerIconPrefab != null)
    {
        playerIcon = Instantiate(playerIconPrefab, new Vector3(currentNode.Position.x, currentNode.Position.y, 0) + this.mapOffset, Quaternion.identity);
        var sr = playerIcon.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = currentColor;
    }
    MarkVisited(currentNode);
}


    private void Update()
    {
        if (currentNode == null || currentNode.Exits.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.A)) Move(-1);
        else if (Input.GetKeyDown(KeyCode.D)) Move(1);
    }

    private void Move(int direction)
    {
        var sortedExits = currentNode.Exits.OrderBy(e => e.Position.x).ToList();
        if (sortedExits.Count == 0) return;

        MapGenerator.MapNode nextNode;
        if (sortedExits.Count == 1)
            nextNode = sortedExits[0];
        else
            nextNode = (direction == -1) ? sortedExits[0] : sortedExits[sortedExits.Count - 1];

        currentNode = nextNode;

        // Move the player icon
        if (playerIcon != null)
    {
        playerIcon.transform.position = new Vector3(nextNode.Position.x, nextNode.Position.y, 0) + mapOffset;
    }

        // Mark visited
        MarkVisited(currentNode);
    }

    private void MarkVisited(MapGenerator.MapNode node)
    {
        if (visitedNodes.Contains(node)) return;

        visitedNodes.Add(node);

        if (visitedOverlayPrefab != null)
        {
            var overlay = Instantiate(visitedOverlayPrefab, node.Position, Quaternion.identity);
            var sr = overlay.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = visitedColor;
        }
    }
}
