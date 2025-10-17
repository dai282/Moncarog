using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Data.Common;

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
    private Dictionary<int, MapGenerator.MapNode> loadedNodesById = new Dictionary<int, MapGenerator.MapNode>();

    private HashSet<MapGenerator.MapNode> visitedNodes = new HashSet<MapGenerator.MapNode>();
    private Dictionary<MapGenerator.MapNode, Vector3> nodePositions = new Dictionary<MapGenerator.MapNode, Vector3>();

    public bool isReady { get; private set; }

    public void CleanupMap()
    {
        visitedNodes.Clear();
        nodeToGameObjectMap.Clear();
        nodePositions.Clear();

        if (traversalOverlay != null)
        {
            for (int i = traversalOverlay.transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = traversalOverlay.transform.GetChild(i).gameObject;
                if (Application.isPlaying) Destroy(child); else DestroyImmediate(child);
            }
        }

        // fallback cleanup for objects named "Room (...)"
        // FIXED: Use the correct method signature for newer Unity versions
        #if UNITY_2022_1_OR_NEWER
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        #else
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        #endif
        foreach (GameObject obj in allObjects)
        {
            if (obj == null) continue;
            if (obj.name.Contains("Room") && obj.name.Contains("("))
            {
                if (Application.isPlaying) Destroy(obj); else DestroyImmediate(obj);
            }
        }

        isReady = false;
    }

    // FIXED: Make sure Init method exists and is public
    public void Init()
    {
        CleanupMap();

        mapGenerator = new MapGenerator();
        startNode = mapGenerator.GenerateMap();

        Debug.Log("Map generation complete! Now displaying the map in the scene.");

        if (normalPrefab == null || grassPrefab == null || waterPrefab == null ||
            firePrefab == null || miniBossPrefab == null || finalBossPrefab == null ||
            linePrefab == null)
        {
            Debug.LogError("One or more prefabs are not assigned in MapManager!");
        }

        // Draw map and initialize overlay
        DisplayMap(startNode, new Vector3(0, -14, 0));

        if (traversalOverlay != null)
        {
            traversalOverlay.Initialize(startNode, nodeToGameObjectMap);
        }
        else
        {
            Debug.LogError("TraversalOverlay reference is missing in MapManager!");
        }

        isReady = true;
    }

    private void DisplayMap(MapGenerator.MapNode node, Vector3 origin)
    {
        if (node == null) return;
        if (visitedNodes.Contains(node)) return;
        visitedNodes.Add(node);

        Vector3 position = origin + new Vector3(node.Position.x, node.Position.y, 0);
        nodePositions[node] = position;

        GameObject selectedPrefab = GetPrefabForRoomType(node.Room.Type);
        GameObject roomObj = Instantiate(selectedPrefab, position, Quaternion.identity, traversalOverlay != null ? traversalOverlay.transform : null);
        roomObj.name = $"Room {node.Room.Name} ({node.Room.Type})";

        // Save reference for traversalOverlay
        if (!nodeToGameObjectMap.ContainsKey(node)) nodeToGameObjectMap[node] = roomObj;

        // draw connections and recurse
        foreach (var exit in node.Exits)
        {
            if (exit == null) continue;
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
        if (linePrefab == null) return;
        GameObject lineObj = Instantiate(linePrefab, traversalOverlay != null ? traversalOverlay.transform : null);
        LineRenderer lr = lineObj.GetComponent<LineRenderer>();
        if (lr != null)
        {
            lr.positionCount = 2;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
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

    public (RoomInfo currentRoom, List<RoomInfo> nextRooms) GetCurrentRoomInfo()
    {
        if (traversalOverlay == null) return (null, null);

        var currentNode = traversalOverlay.GetCurrentNode();
        if (currentNode == null) return (null, null);

        RoomInfo currentRoomInfo = new RoomInfo();
        currentRoomInfo.roomName = currentNode.Room.Name.ToString();
        currentRoomInfo.numDoors = currentNode.Exits.Count;

        if (currentNode.Exits.Count == 1)
        {
            currentRoomInfo.doorSingle = (int)currentNode.Exits[0].Room.Type;
            currentRoomInfo.doorLeft = 0;
            currentRoomInfo.doorRight = 0;
        }
        else if (currentNode.Exits.Count == 2)
        {
            currentRoomInfo.doorLeft = (int)currentNode.Exits[0].Room.Type;
            currentRoomInfo.doorRight = (int)currentNode.Exits[1].Room.Type;
            currentRoomInfo.doorSingle = 0;
        }
        else
        {
            currentRoomInfo.doorSingle = 0;
            currentRoomInfo.doorLeft = 0;
            currentRoomInfo.doorRight = 0;
        }

        List<RoomInfo> nextRooms = new List<RoomInfo>();
        foreach (var exit in currentNode.Exits)
        {
            if (exit == null) continue;
            RoomInfo nextRoom = new RoomInfo();
            nextRoom.roomName = exit.Room.Name.ToString();
            nextRoom.numDoors = exit.Exits.Count;

            if (exit.Exits.Count == 1)
            {
                nextRoom.doorSingle = (int)exit.Exits[0].Room.Type;
                nextRoom.doorLeft = 0;
                nextRoom.doorRight = 0;
            }
            else if (exit.Exits.Count == 2)
            {
                nextRoom.doorLeft = (int)exit.Exits[0].Room.Type;
                nextRoom.doorRight = (int)exit.Exits[1].Room.Type;
                nextRoom.doorSingle = 0;
            }
            else
            {
                nextRoom.doorSingle = 0;
                nextRoom.doorLeft = 0;
                nextRoom.doorRight = 0;
            }

            nextRooms.Add(nextRoom);
        }

        return (currentRoomInfo, nextRooms);
    }

    [System.Serializable]
    public class RoomInfo
    {
        public string roomName;
        public int numDoors;
        public int doorSingle;
        public int doorLeft;
        public int doorRight;
    }

    public int currentRoomId;
    public int getCurrentRoomId() { return currentRoomId; }

// In MapManager.cs

// REPLACE the existing GetSerializedMapData method with this new version.
public List<SerializableMapNode> GetSerializedMapData(MapGenerator.MapNode activeNode, out int outActiveNodeId)
{
    outActiveNodeId = 1; // Default to the start node's ID in case of failure.
    if (startNode == null)
    {
        Debug.LogError("Cannot save map: startNode is null.");
        return new List<SerializableMapNode>();
    }

    List<SerializableMapNode> serializableNodes = new List<SerializableMapNode>();
    HashSet<MapGenerator.MapNode> visited = new HashSet<MapGenerator.MapNode>();
    Queue<MapGenerator.MapNode> queue = new Queue<MapGenerator.MapNode>();
    Dictionary<MapGenerator.MapNode, int> nodeToId = new Dictionary<MapGenerator.MapNode, int>();
    int nextId = 1;

    // Start BFS from start node
    queue.Enqueue(startNode);
    visited.Add(startNode);
    nodeToId[startNode] = nextId++;

    while (queue.Count > 0)
    {
        var currentNode = queue.Dequeue();
        int currentId = nodeToId[currentNode];

            // *** ADDED LOGIC ***
            // Check if the node we are processing is the player's active node.
            if (currentNode == activeNode)
            {
                outActiveNodeId = currentId; // If it is, store its generated ID.
            }
        
        var serializedNode = new SerializableMapNode
        {
            roomId = currentId,
            originalRoomName = currentNode.Room.Name, // Save the crucial original name
            roomType = (int)currentNode.Room.Type,
            position = currentNode.Position
        };

        foreach (var exitNode in currentNode.Exits)
        {
            if (exitNode == null) continue;
            
            if (!nodeToId.ContainsKey(exitNode))
            {
                nodeToId[exitNode] = nextId++;
                visited.Add(exitNode);
                queue.Enqueue(exitNode);
            }
            
            int exitId = nodeToId[exitNode];
            serializedNode.exitRoomIds.Add(exitId);
        }

        serializableNodes.Add(serializedNode);
    }
    
    return serializableNodes;
}
    public List<int> GetTraversalPath()
    {
        return traversalOverlay != null ? traversalOverlay.GetTraversalPath() : new List<int>();
    }


    public void LoadMapFromData(List<SerializableMapNode> mapData)
    {
        CleanupMap();
        loadedNodesById.Clear(); // Clear data from any previous load.

        if (mapData == null || mapData.Count == 0)
        {
            Debug.LogError("LoadMapFromData: mapData is null or empty.");
            return;
        }

        // Use 'createdNodes' just as a temporary variable inside this method.
        Dictionary<int, MapGenerator.MapNode> createdNodes = new Dictionary<int, MapGenerator.MapNode>();
        Dictionary<int, GameObject> createdVisuals = new Dictionary<int, GameObject>();

        // FIRST PASS: create all nodes
        foreach (var nodeData in mapData)
        {
            var newNode = new MapGenerator.MapNode();
            newNode.Exits = new List<MapGenerator.MapNode>();
            newNode.Parents = new List<MapGenerator.MapNode>();
            newNode.Position = nodeData.position;

            // --- START MODIFICATION ---
            // Create room - use the saved original name for gameplay logic.
            var roomObj = new MapGenerator.Room();
            roomObj.Name = nodeData.originalRoomName; // Restore the original name here!
            roomObj.Type = (MapGenerator.RoomType)nodeData.roomType;
            newNode.Room = roomObj;
            // --- END MODIFICATION ---

            createdNodes[nodeData.roomId] = newNode;
            loadedNodesById[nodeData.roomId] = newNode;

            // Create visual
            Vector3 position = new Vector3(nodeData.position.x, nodeData.position.y, 0) + new Vector3(0, -14, 0);
            GameObject selectedPrefab = GetPrefabForRoomType(newNode.Room.Type);
            GameObject roomObjVis = Instantiate(selectedPrefab, position, Quaternion.identity, traversalOverlay != null ? traversalOverlay.transform : null);
            roomObjVis.name = $"Room {nodeData.roomId} (Type:{newNode.Room.Type})";

            createdVisuals[nodeData.roomId] = roomObjVis;

            if (nodeData.roomId == 1)
            {
                startNode = newNode;
            }
        }

    // SECOND PASS: connect nodes using the saved exitRoomIds
    foreach (var nodeData in mapData)
    {
        if (!createdNodes.ContainsKey(nodeData.roomId)) continue;
        
        var currentNode = createdNodes[nodeData.roomId];
        
        // Clear any temporary connections
        currentNode.Exits.Clear();
        currentNode.Parents.Clear();

        foreach (int exitId in nodeData.exitRoomIds)
        {
            if (!createdNodes.ContainsKey(exitId)) 
            {
                Debug.LogWarning($"Exit node {exitId} not found for node {nodeData.roomId}");
                continue;
            }
            
            var exitNode = createdNodes[exitId];
            
            // Add exit connection (current -> exit)
            if (!currentNode.Exits.Contains(exitNode))
            {
                currentNode.Exits.Add(exitNode);
            }
            
            // Add parent connection (exit -> current)
            if (!exitNode.Parents.Contains(currentNode))
            {
                exitNode.Parents.Add(currentNode);
            }
            
            // Draw connection line
            DrawLine(createdVisuals[nodeData.roomId].transform.position, createdVisuals[exitId].transform.position);
        }
    }

    // THIRD PASS: Initialize traversal
    if (startNode != null)
    {
        nodeToGameObjectMap = createdNodes.ToDictionary(kvp => kvp.Value, kvp => createdVisuals[kvp.Key]);
        traversalOverlay.Initialize(startNode, nodeToGameObjectMap);
        
        isReady = true;
        Debug.Log($"Map loading complete: {createdNodes.Count} nodes loaded");
        
        // Log the loaded connection structure for verification
        Debug.Log("Loaded Connection Structure:");
        foreach (var node in mapData)
        {
            Debug.Log($"Room {node.roomId} -> {string.Join(", ", node.exitRoomIds)}");
        }
    }
    else
    {
        Debug.LogError("Failed to rebuild map: Start node not found.");
    }
}


    [System.Serializable]
    public class SerializableMapNode
    {
        public int roomId; // The structural ID for connecting nodes.
        public int originalRoomName; // The functional name used by the board generator (e.g., 2-20).
        public int roomType;
        public Vector2 position;
        public List<int> exitRoomIds = new List<int>();
    }
}