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
    public GameObject eventPrefab;

    [Header("Line Prefab")]
    public GameObject linePrefab;

    [Header("References")]
    public MapTraversalOverlay traversalOverlay;
    public RectTransform mapContainer; // Assign your "MapBackdrop" GameObject to this in the Inspector

    private MapGenerator mapGenerator;
    private MapGenerator.MapNode startNode;

    // We'll store the calculated map bounds here
    private Vector2 minBounds;
    private Vector2 maxBounds;

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

        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        
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
        NewDisplayMap(startNode, new Vector3(0, -14, 0));

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

    #region New DisplayMap Logic
    private void NewDisplayMap(MapGenerator.MapNode node, Vector3 origin)
    {
        if (node == null) return;

        // --- New Logic ---
        // 1. First, traverse the map to find its boundaries
        minBounds = Vector2.positiveInfinity;
        maxBounds = Vector2.negativeInfinity;
        FindMapBounds(node, new HashSet<MapGenerator.MapNode>());

        // 2. Get the size of the container we want to fit the map into
        Rect containerRect = mapContainer.rect;
        float containerWidth = containerRect.width;
        float containerHeight = containerRect.height;

        // 3. Calculate the scale factor to make the map fit
        float mapWidth = maxBounds.x - minBounds.x;
        float mapHeight = maxBounds.y - minBounds.y;

        // Add a small padding so nodes aren't right on the edge
        float padding = 50f;
        float scaleX = (containerWidth - padding) / (mapWidth > 0 ? mapWidth : 1);
        float scaleY = (containerHeight - padding) / (mapHeight > 0 ? mapHeight : 1);
        float scale = Mathf.Min(scaleX, scaleY); // Use the smaller scale to maintain aspect ratio

        // 4. Instantiate and position all nodes with the new scale and offset
        InstantiateMapObjects(node, new HashSet<MapGenerator.MapNode>(), scale);
    }

    // New helper function to find the extents of the map
    private void FindMapBounds(MapGenerator.MapNode node, HashSet<MapGenerator.MapNode> visited)
    {
        if (node == null || visited.Contains(node)) return;
        visited.Add(node);

        if (node.Position.x < minBounds.x) minBounds.x = node.Position.x;
        if (node.Position.y < minBounds.y) minBounds.y = node.Position.y;
        if (node.Position.x > maxBounds.x) maxBounds.x = node.Position.x;
        if (node.Position.y > maxBounds.y) maxBounds.y = node.Position.y;

        foreach (var exit in node.Exits)
        {
            FindMapBounds(exit, visited);
        }
    }

    // New function to create the actual GameObjects
    private void InstantiateMapObjects(MapGenerator.MapNode node, HashSet<MapGenerator.MapNode> visited, float scale)
    {
        if (node == null || visited.Contains(node)) return;
        visited.Add(node);

        // --- Main Change: Positioning Logic ---
        // Convert original map position to a scaled UI position
        float xPos = (node.Position.x - minBounds.x) * scale;
        float yPos = (node.Position.y - minBounds.y) * scale;

        // Center the map within the container
        float mapWidthScaled = (maxBounds.x - minBounds.x) * scale;
        float mapHeightScaled = (maxBounds.y - minBounds.y) * scale;
        float xOffset = (mapContainer.rect.width - mapWidthScaled) / 2f;
        float yOffset = (mapContainer.rect.height - mapHeightScaled) / 2f;

        // We subtract half the container width/height to move the origin (0,0) to the center,
        // then we position relative to the bottom-left.
        Vector2 anchoredPosition = new Vector2(xPos + xOffset, yPos + yOffset);


        GameObject selectedPrefab = GetPrefabForRoomType(node.Room.Type);
        // The parent is now traversalOverlay.transform as before
        GameObject roomObj = Instantiate(selectedPrefab, traversalOverlay.transform);
        roomObj.name = $"Room {node.Room.Name} ({node.Room.Type})";

        // --- Use RectTransform for positioning ---
        RectTransform rectTransform = roomObj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // Set anchor to the bottom left for predictable positioning
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.anchoredPosition = anchoredPosition;
        }
        else
        {
            Debug.LogWarning($"Prefab for room type {node.Room.Type} is missing a RectTransform component. Make sure it's a UI object.");
        }


        if (!nodeToGameObjectMap.ContainsKey(node)) nodeToGameObjectMap[node] = roomObj;

        // Recurse and draw connections
        foreach (var exit in node.Exits)
        {
            if (exit == null) continue;

            // Make sure we have the exit's GameObject to draw the line to it
            if (!visited.Contains(exit))
            {
                InstantiateMapObjects(exit, visited, scale);
            }

            if (nodeToGameObjectMap.ContainsKey(node) && nodeToGameObjectMap.ContainsKey(exit))
            {
                //Vector3 startPos = nodeToGameObjectMap[node].GetComponent<RectTransform>().anchoredPosition;
                //Vector3 endPos = nodeToGameObjectMap[exit].GetComponent<RectTransform>().anchoredPosition;
                //DrawLine(startPos, endPos);

                // Use localPosition, which is relative to the parent's pivot (center)
                Vector3 startPos = nodeToGameObjectMap[node].transform.localPosition;
                Vector3 endPos = nodeToGameObjectMap[exit].transform.localPosition;
                DrawLine(startPos, endPos);
            }
        }
    }

    #endregion

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

        //GameObject lineObj = Instantiate(linePrefab, traversalOverlay != null ? traversalOverlay.transform : null);
        GameObject lineObj = Instantiate(linePrefab, traversalOverlay.transform);
        lineObj.name = "ConnectionLine";

        LineRenderer lr = lineObj.GetComponent<LineRenderer>();

        // --- Important for LineRenderer in a Canvas ---
        // Make sure the LineRenderer is using local space
        lr.useWorldSpace = false;

        //if (lr != null)
        //{
        //    lr.positionCount = 2;
        //    lr.SetPosition(0, start);
        //    lr.SetPosition(1, end);
        //}

        if (lr != null)
        {
            lr.positionCount = 2;
            // The Z-coordinate should be 0 for UI
            lr.SetPosition(0, new Vector3(start.x, start.y, 0));
            lr.SetPosition(1, new Vector3(end.x, end.y, 0));
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
            case MapGenerator.RoomType.Event: return eventPrefab;
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


    public void LoadMapFromData_Old(List<SerializableMapNode> mapData)
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

        // 1. Create all nodes
        foreach (var nodeData in mapData)
        {
            var newNode = new MapGenerator.MapNode();
            newNode.Exits = new List<MapGenerator.MapNode>();
            newNode.Parents = new List<MapGenerator.MapNode>();
            newNode.Position = nodeData.position;

            // Create room - use the saved original name for gameplay logic.
            var roomObj = new MapGenerator.Room();
            roomObj.Name = nodeData.originalRoomName;
            roomObj.Type = (MapGenerator.RoomType)nodeData.roomType;
            newNode.Room = roomObj;

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

        // 2. Connect nodes using the saved exitRoomIds
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

        // 3. Initialize traversal
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

    // In MapManager.cs

    public void LoadMapFromData(List<SerializableMapNode> mapData)
    {
        CleanupMap();
        loadedNodesById.Clear();

        if (mapData == null || mapData.Count == 0)
        {
            Debug.LogError("LoadMapFromData: mapData is null or empty.");
            return;
        }

        // A temporary dictionary to hold the rebuilt MapNode data objects.
        Dictionary<int, MapGenerator.MapNode> createdNodes = new Dictionary<int, MapGenerator.MapNode>();

        // --- Step 1: Rebuild the in-memory map structure (NO visuals yet) ---
        foreach (var nodeData in mapData)
        {
            var newNode = new MapGenerator.MapNode();
            newNode.Exits = new List<MapGenerator.MapNode>();
            newNode.Parents = new List<MapGenerator.MapNode>();
            newNode.Position = nodeData.position;

            var roomObj = new MapGenerator.Room
            {
                Name = nodeData.originalRoomName,
                Type = (MapGenerator.RoomType)nodeData.roomType
            };
            newNode.Room = roomObj;

            createdNodes[nodeData.roomId] = newNode;
            loadedNodesById[nodeData.roomId] = newNode;

            // Find the start node for later
            if (nodeData.roomId == 1)
            {
                startNode = newNode;
            }
        }

        // Connect the rebuilt nodes to each other
        foreach (var nodeData in mapData)
        {
            if (!createdNodes.ContainsKey(nodeData.roomId)) continue;

            var currentNode = createdNodes[nodeData.roomId];
            foreach (int exitId in nodeData.exitRoomIds)
            {
                if (createdNodes.TryGetValue(exitId, out var exitNode))
                {
                    if (!currentNode.Exits.Contains(exitNode)) currentNode.Exits.Add(exitNode);
                    if (!exitNode.Parents.Contains(currentNode)) exitNode.Parents.Add(currentNode);
                }
            }
        }

        // --- Step 2: Calculate boundaries and scale (just like in DisplayMap) ---
        if (startNode == null)
        {
            Debug.LogError("Failed to rebuild map: Start node (ID 1) not found in saved data.");
            return;
        }

        minBounds = Vector2.positiveInfinity;
        maxBounds = Vector2.negativeInfinity;
        FindMapBounds(startNode, new HashSet<MapGenerator.MapNode>()); // Re-use our helper function

        Rect containerRect = mapContainer.rect;
        float padding = 50f;
        float scaleX = (containerRect.width - padding) / ((maxBounds.x - minBounds.x) > 0 ? (maxBounds.x - minBounds.x) : 1);
        float scaleY = (containerRect.height - padding) / ((maxBounds.y - minBounds.y) > 0 ? (maxBounds.y - minBounds.y) : 1);
        float scale = Mathf.Min(scaleX, scaleY);

        float mapWidthScaled = (maxBounds.x - minBounds.x) * scale;
        float mapHeightScaled = (maxBounds.y - minBounds.y) * scale;
        float xOffset = (containerRect.width - mapWidthScaled) / 2f;
        float yOffset = (containerRect.height - mapHeightScaled) / 2f;

        // --- Step 3: Instantiate and position the visual GameObjects ---
        nodeToGameObjectMap.Clear(); // Clear the main dictionary before populating it
        foreach (var kvp in createdNodes)
        {
            var node = kvp.Value;

            // Convert original position to scaled UI position
            float xPos = (node.Position.x - minBounds.x) * scale + xOffset;
            float yPos = (node.Position.y - minBounds.y) * scale + yOffset;
            Vector2 anchoredPosition = new Vector2(xPos, yPos);

            // Instantiate and name the prefab
            GameObject selectedPrefab = GetPrefabForRoomType(node.Room.Type);
            GameObject roomObjVis = Instantiate(selectedPrefab, traversalOverlay.transform);
            roomObjVis.name = $"Room {kvp.Key} (Type:{node.Room.Type})";

            // Position it using RectTransform
            RectTransform rectTransform = roomObjVis.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.zero;
            rectTransform.anchoredPosition = anchoredPosition;

            // Store the visual for the traversal system and for drawing lines
            nodeToGameObjectMap[node] = roomObjVis;
        }

        // --- Step 4: Draw the connection lines ---
        foreach (var node in createdNodes.Values)
        {
            foreach (var exitNode in node.Exits)
            {
                // Get the positions from the newly created visuals
                Vector3 startPos = nodeToGameObjectMap[node].transform.localPosition;
                Vector3 endPos = nodeToGameObjectMap[exitNode].transform.localPosition;
                DrawLine(startPos, endPos);
            }
        }

        // --- Step 5: Initialize the traversal system ---
        traversalOverlay.Initialize(startNode, nodeToGameObjectMap);
        isReady = true;
        Debug.Log($"Map loading complete: {createdNodes.Count} nodes loaded and displayed.");
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


