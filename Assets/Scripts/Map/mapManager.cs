using UnityEngine;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    [Header("Room Prefabs")]
    public GameObject normalPrefab;
    public GameObject grassPrefab;
    public GameObject waterPrefab;
    public GameObject firePrefab;
    public GameObject miniBossPrefab;
    public GameObject finalBossPrefab;

    [Header("Line Prefab")]
    public GameObject linePrefab;

    [Header("References")]
    public MapTraversalOverlay traversalOverlay;

    private MapGenerator mapGenerator;
    private MapGenerator.MapNode startNode;

    // Store prefab instances so traversalOverlay can recolor them
    private Dictionary<MapGenerator.MapNode, GameObject> nodeToGameObjectMap = new Dictionary<MapGenerator.MapNode, GameObject>();

    private HashSet<MapGenerator.MapNode> visitedNodes = new HashSet<MapGenerator.MapNode>();
    private Dictionary<MapGenerator.MapNode, Vector3> nodePositions = new Dictionary<MapGenerator.MapNode, Vector3>();

    public void Start()
    {
        if (normalPrefab == null || grassPrefab == null || waterPrefab == null ||
            firePrefab == null || miniBossPrefab == null || finalBossPrefab == null ||
            linePrefab == null)
        {
            Debug.LogError("One or more prefabs are not assigned in MapManager!");
            return;
        }

        mapGenerator = new MapGenerator();
        startNode = mapGenerator.GenerateMap();

        Debug.Log("Map generation complete! Now displaying the map in the scene.");

        // Draw map
        DisplayMap(startNode, new Vector3(0, -15, 0));

        // Initialize traversalOverlay AFTER map is displayed
        if (traversalOverlay != null)
        {
            traversalOverlay.Initialize(startNode, nodeToGameObjectMap);
        }
        else
        {
            Debug.LogError("TraversalOverlay reference is missing in MapManager!");
        }
    }

    private void DisplayMap(MapGenerator.MapNode node, Vector3 origin)
    {
        if (visitedNodes.Contains(node)) return;
        visitedNodes.Add(node);

        Vector3 position = origin + new Vector3(node.Position.x, node.Position.y, 0);
        nodePositions[node] = position;

        GameObject selectedPrefab = GetPrefabForRoomType(node.Room.Type);
        GameObject roomObj = Instantiate(selectedPrefab, position, Quaternion.identity);
        roomObj.name = $"Room {node.Room.Name} ({node.Room.Type})";

        // Save reference for traversalOverlay
        nodeToGameObjectMap[node] = roomObj;

        foreach (var exit in node.Exits)
        {
            Vector3 nextPos = origin + new Vector3(exit.Position.x, exit.Position.y, 0);

            if (nodePositions.ContainsKey(exit))
                DrawLine(position, nodePositions[exit]);
            else
                DrawLine(position, nextPos);

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
    }

    private GameObject GetPrefabForRoomType(MapGenerator.RoomType type)
    {
        switch (type)
        {
            case MapGenerator.RoomType.Normal: return normalPrefab;
            case MapGenerator.RoomType.Grass: return grassPrefab;
            case MapGenerator.RoomType.Water: return waterPrefab;
            case MapGenerator.RoomType.Fire: return firePrefab;
            case MapGenerator.RoomType.MiniBoss: return miniBossPrefab;
            case MapGenerator.RoomType.FinalBoss: return finalBossPrefab;
            default: return normalPrefab;
        }
    }
}
