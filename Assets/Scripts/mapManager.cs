using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;

public class MapManager : MonoBehaviour
{
    // Prefabs for different room types. Assign these in the Unity Inspector.
    public GameObject normalPrefab;
    public GameObject grassPrefab;
    public GameObject waterPrefab;
    public GameObject firePrefab;
    public GameObject miniBossPrefab;
    public GameObject finalBossPrefab;

    // The prefab to use for the lines connecting rooms.
    public GameObject linePrefab;

    // The generated map's starting node.
    private MapGenerator.MapNode startNode;

    // The MapGenerator instance.
    private MapGenerator mapGenerator;

    public MapTraversalOverlay traversalOverlay;

    // Spacing between rooms for visual layout.
    private const float xSpacing = 2f;
    private const float ySpacing = 2f;

    // Tracks visited nodes to prevent infinite loops on a non-tree graph.
    private HashSet<MapGenerator.MapNode> visitedNodes = new HashSet<MapGenerator.MapNode>();
    private Dictionary<MapGenerator.MapNode, Vector3> nodePositions = new Dictionary<MapGenerator.MapNode, Vector3>();

    void Start()
    {
        // Check if all required prefabs have been assigned in the inspector.
        if (normalPrefab == null || grassPrefab == null || waterPrefab == null || firePrefab == null || miniBossPrefab == null || finalBossPrefab == null || linePrefab == null)
        {
            Debug.LogError("One or more prefabs are not assigned! Please assign them in the Unity Inspector.");
            return;
        }
        // Create an instance of the MapGenerator.
        mapGenerator = new MapGenerator();

        // Generate the map and store the starting node.
        startNode = mapGenerator.GenerateMap();

        Debug.Log("Map generation complete! Now displaying the map in the scene.");


        DisplayMap(startNode, new Vector3(0, -15, 0));

        if (traversalOverlay != null)
        {
            Vector3 mapOffset = new Vector3(0, -15, 0);

            traversalOverlay.Initialize(startNode, mapOffset);
        }
        else
        {
            Debug.LogError("TraversalOverlay reference is missing in MapManager!");
        }
    }

    // Recursively displays the map by instantiating GameObjects for each room.
    private void DisplayMap(MapGenerator.MapNode node, Vector3 origin)
    {
        if (visitedNodes.Contains(node))
            return;

        visitedNodes.Add(node);

        // Use the node's generated position, shifted by origin
        Vector3 position = origin + new Vector3(node.Position.x, node.Position.y, 0);
        nodePositions[node] = position;

        GameObject selectedPrefab = GetPrefabForRoomType(node.Room.Type);
        GameObject roomObj = Instantiate(selectedPrefab, position, Quaternion.identity);
        roomObj.name = $"Room {node.Room.Name} ({node.Room.Type})";

        foreach (var exit in node.Exits)
        {
            Vector3 nextPos = origin + new Vector3(exit.Position.x, exit.Position.y, 0);

            if (nodePositions.ContainsKey(exit))
            {
                DrawLine(position, nodePositions[exit]);
            }
            else
            {
                DrawLine(position, nextPos);
            }

            DisplayMap(exit, origin);
        }
    }

    
    private void DrawLine(Vector3 start, Vector3 end)
    {
        GameObject lineObj = Instantiate(linePrefab, transform);
        LineRenderer lineRenderer = lineObj.GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
        }
        else
        {
            Vector3 midpoint = (start + end) / 2;
            lineObj.transform.position = midpoint;
            lineObj.transform.LookAt(end);
            lineObj.transform.localScale = new Vector3(lineObj.transform.localScale.x, lineObj.transform.localScale.y, Vector3.Distance(start, end));
        }
    }

    /// Returns the appropriate prefab based on the room's type.
    private GameObject GetPrefabForRoomType(MapGenerator.RoomType type)
    {
        switch (type)
        {
            case MapGenerator.RoomType.Normal:
                return normalPrefab;
            case MapGenerator.RoomType.Grass:
                return grassPrefab;
            case MapGenerator.RoomType.Water:
                return waterPrefab;
            case MapGenerator.RoomType.Fire:
                return firePrefab;
            case MapGenerator.RoomType.MiniBoss:
                return miniBossPrefab;
            case MapGenerator.RoomType.FinalBoss:
                return finalBossPrefab;
            default:
                return normalPrefab;
        }
    }
}
