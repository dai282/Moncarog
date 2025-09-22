using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// Handles traversing the generated map with A (left) and D (right).
// Highlights the current node in green and previously visited nodes in gray.
// Works alongside MapManager without destroying or duplicating anything.

public class MapTraversalOverlay : MonoBehaviour
{
    [Header("Colors")]
    [SerializeField] private Color visitedColor = Color.gray;
    [SerializeField] private Color currentColor = Color.green;
    [SerializeField] private Color defaultColor = Color.white;

    private MapGenerator.MapNode currentNode;
    public GameObject mapRoot;

    // Tracks all visited nodes
    private HashSet<MapGenerator.MapNode> visitedNodes = new HashSet<MapGenerator.MapNode>();

    // Dictionary to link map nodes to their spawned GameObjects (populated by MapManager)
    private Dictionary<MapGenerator.MapNode, GameObject> nodeVisuals = new Dictionary<MapGenerator.MapNode, GameObject>();

    // Called by MapManager after the map is fully generated and displayed
    public void Initialize(MapGenerator.MapNode startNode, Dictionary<MapGenerator.MapNode, GameObject> visuals)
    {
        currentNode = startNode;
        nodeVisuals = visuals;

        visitedNodes.Clear();
        visitedNodes.Add(currentNode);

        UpdateNodeColors();
        mapRoot.SetActive(false);
    }

    private void Update()
    {
        #if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Keyboard.current.aKey.wasPressedThisFrame)
            Move(0);
        else if (UnityEngine.InputSystem.Keyboard.current.dKey.wasPressedThisFrame)
            Move(1);
        #else
        if (Input.GetKeyDown(KeyCode.A))
            Move(0);
        else if (Input.GetKeyDown(KeyCode.D))
            Move(1);
        #endif
    }

    public void Move(int direction)
    {
        if (currentNode.Exits.Count == 0) return;

        MapGenerator.MapNode nextNode = null;

        if (currentNode.Exits.Count == 1)
        {
            // Only one exit → always go there
            nextNode = currentNode.Exits[0];
        }
        else if (currentNode.Exits.Count == 2)
        {
            // Two exits → A = index 0, D = index 1
            nextNode = (direction == 0) ? currentNode.Exits[0] : currentNode.Exits[1];
        }

        if (nextNode != null)
        {
            currentNode = nextNode;
            visitedNodes.Add(currentNode);
            UpdateNodeColors();
        }
    }


    private void UpdateNodeColors()
    {
        foreach (var kvp in nodeVisuals)
        {
            var sr = kvp.Value.GetComponent<SpriteRenderer>();
            if (sr == null) continue;

            if (kvp.Key == currentNode)
                sr.color = currentColor;
            else if (visitedNodes.Contains(kvp.Key))
                sr.color = visitedColor;
            else
                sr.color = defaultColor;
        }
    }

    // Add this to MapTraversalOverlay.cs
    public MapGenerator.MapNode GetCurrentNode()
    {
        return currentNode;
    }
}
