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

    public bool isReady { get; private set; }

    public void CleanupMap()
    {
        Debug.Log("Cleaning up existing map data");
        
        // Clear all tracking collections
        visitedNodes.Clear();
        nodeToGameObjectMap.Clear();
        nodePositions.Clear();
        
        // Destroy all existing map objects
        if (traversalOverlay != null)
        {
            // Destroy all children of the traversal overlay (rooms and lines)
            for (int i = traversalOverlay.transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = traversalOverlay.transform.GetChild(i).gameObject;
                if (Application.isPlaying)
                {
                    Destroy(child);
                }
                else
                {
                    DestroyImmediate(child);
                }
            }
        }
        
        // Find any objects with "Room" in the name (fallback cleanup)
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Room") && obj.name.Contains("("))
            {
                if (Application.isPlaying)
                {
                    Destroy(obj);
                }
                else
                {
                    DestroyImmediate(obj);
                }
            }
        }
        
        isReady = false;
    }

    public void Init()
    {
        // Clean up any existing map data first
        CleanupMap();

        if (normalPrefab == null || grassPrefab == null || waterPrefab == null ||
            firePrefab == null || miniBossPrefab == null || finalBossPrefab == null ||
            linePrefab == null)
        {
            Debug.LogError("One or more prefabs are not assigned in MapManager!");
            //return;
        }

        mapGenerator = new MapGenerator();
        startNode = mapGenerator.GenerateMap();

        Debug.Log("Map generation complete! Now displaying the map in the scene.");

        // Draw map
        DisplayMap(startNode, new Vector3(-20, -18, 0));

        // Initialize traversalOverlay AFTER map is displayed
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

    // Inside MapManager.cs
    private void DisplayMap(MapGenerator.MapNode node, Vector3 origin)
    {
        if (visitedNodes.Contains(node)) return;
        visitedNodes.Add(node);

        Vector3 position = origin + new Vector3(node.Position.x, node.Position.y, 0);
        nodePositions[node] = position;

        GameObject selectedPrefab = GetPrefabForRoomType(node.Room.Type);
        // Instantiate the room object as a child of the traversal overlay
        GameObject roomObj = Instantiate(selectedPrefab, position, Quaternion.identity, traversalOverlay.transform);
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
        // Pass the traversal overlay transform as the parent
        GameObject lineObj = Instantiate(linePrefab, traversalOverlay.transform);
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


    public (RoomInfo currentRoom, List<RoomInfo> nextRooms) GetCurrentRoomInfo()
    {
        if (traversalOverlay == null)
        {
            Debug.LogError("TraversalOverlay not initialized!");
            return (null, null);
        }

        // Get current node from traversal overlay
        var currentNode = traversalOverlay.GetCurrentNode();

        // Create RoomInfo for current room
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
            // left is first exit, right is second exit
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

        // Create list of next available rooms
        List<RoomInfo> nextRooms = new List<RoomInfo>();
        foreach (var exit in currentNode.Exits)
        {
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
                // left is first exit, right is second exit
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

    // Add this class definition outside of MapManager class but in the same file
    [System.Serializable]
    public class RoomInfo
    {
        public string roomName;
        public int numDoors;
        //the ID of room specifies what room type they are
        public int doorSingle;
        public int doorLeft;
        public int doorRight;
    }
}