using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MapGenerator
{
    // The total number of rooms between the start and mini-bosses.
    private const int roomsToMiniBoss = 6;
    private const int roomsToFinalBoss = 5;

    // Room type definitions
    public enum RoomType { Normal, Grass, Water, Fire, MiniBoss, FinalBoss }

    private int currentY = 0;
    private float horizontalSpacing = 2f;
    private float verticalSpacing = 2f;


    // A class to represent a single room.
    public class Room
    {
        public int Name { get; set; }
        public RoomType Type { get; set; }
    }

    // A class to represent a node in the map graph.
    public class MapNode
    {
        public Room Room { get; set; }
        public List<MapNode> Exits { get; set; } = new List<MapNode>();
        public Vector2 Position { get; set; } // Added for position tracking
    }

    private int roomCounter = 1;

    public MapGenerator()
    {
    }

    /// <summary>
    /// Generates the complete map graph from start to finish.
    /// </summary>
    /// <returns>The starting MapNode of the generated map.</returns>
    public MapNode GenerateMap()
    {
        // 1. Create the starting room.
        MapNode startNode = new MapNode { Room = GetNewRoom(RoomType.Normal) };

        Debug.Log("Generating map with forced initial split.");

        // We will keep a list of the end nodes of all active paths.
        List<MapNode> currentPathEnds = new List<MapNode>();

        // 2. Forced initial split into two paths.
        MapNode path1Start = new MapNode { Room = GetNewRoom() };
        MapNode path2Start = new MapNode { Room = GetNewRoom() };
        startNode.Exits.Add(path1Start);
        startNode.Exits.Add(path2Start);

        currentPathEnds.Add(path1Start);
        currentPathEnds.Add(path2Start);

        // 3. Iteratively build the divergent paths.
        for (int i = 2; i <= roomsToMiniBoss; i++)
        {
            currentPathEnds = GenerateNextLevel(currentPathEnds);
        }

        // Capture all node positions just before mini-boss level
        List<Vector2> preMiniBossNodePositions = currentPathEnds.Select(node => node.Position).ToList();

        // 4. Create the mini-boss nodes.
        MapNode miniBoss1 = new MapNode { Room = GetNewRoom(RoomType.MiniBoss) };
        MapNode miniBoss2 = new MapNode { Room = GetNewRoom(RoomType.MiniBoss) };
        MapNode miniBoss3 = new MapNode { Room = GetNewRoom(RoomType.MiniBoss) };

        // 5. Connect the ends of the divergent paths to the mini-bosses.
        ConnectToMiniBossHub(currentPathEnds, new List<MapNode> { miniBoss1, miniBoss2, miniBoss3 });

        // 6. Build the final, converging path to the final boss.
        BuildFinalPath(new List<MapNode> { miniBoss1, miniBoss2, miniBoss3 });

        CenterMap(startNode);
        return startNode;
    }

    /// <summary>
    /// Generates the next level of the map based on the current path ends.
    /// </summary>
    private List<MapNode> GenerateNextLevel(List<MapNode> pathEnds)
    {
        List<MapNode> newPathEnds = new List<MapNode>();
        Dictionary<MapNode, int> mergeCounts = new Dictionary<MapNode, int>();



        // Step 1: Roll each path to determine its intent.
        var intents = new List<string>();
        foreach (var node in pathEnds)
        {
            float roll = Random.Range(0f, 1f);
            if (roll < 0.33f)
                intents.Add("Split");
            else if (roll < 0.66f)
                intents.Add("Merge");
            else
                intents.Add("Continue");
        }

        // Step 2: Process each node with context-aware rules.
        for (int i = 0; i < pathEnds.Count; i++)
        {
            var currentNode = pathEnds[i];
            string intent = intents[i];

            // Look at neighbor intent for possible merging
            MapNode mergeTarget = null;

            if (intent == "Merge")
            {
                if (i > 0 && intents[i - 1] == "Continue" && pathEnds[i - 1].Exits.Count > 0)
                {
                    // Use previous node's last child as merge target
                    mergeTarget = pathEnds[i - 1].Exits.Last();
                }
                else if (i < intents.Count - 1 && intents[i + 1] == "Merge")
                {
                    // Both this and next node want to merge — create a shared node
                    mergeTarget = new MapNode { Room = GetNewRoom() };
                    currentNode.Exits.Add(mergeTarget);
                    pathEnds[i + 1].Exits.Add(mergeTarget);
                    newPathEnds.Add(mergeTarget);
                    i++; // Skip next node, already processed
                    continue;
                }
                else
                {
                    // No merge target — fallback to continue
                    intent = "Continue";
                }
            }

            if (intent == "Split")
            {
                MapNode left = new MapNode { Room = GetNewRoom() };
                MapNode right = new MapNode { Room = GetNewRoom() };
                currentNode.Exits.Add(left);
                currentNode.Exits.Add(right);
                newPathEnds.Add(left);
                newPathEnds.Add(right);
                Debug.Log($"Room {currentNode.Room.Name} split.");
            }
            else if (intent == "Continue")
            {
                MapNode newNode = new MapNode { Room = GetNewRoom() };
                currentNode.Exits.Add(newNode);
                newPathEnds.Add(newNode);
                Debug.Log($"Room {currentNode.Room.Name} continued.");
            }
            else if (intent == "Merge" && mergeTarget != null)
            {
                currentNode.Exits.Add(mergeTarget);
                newPathEnds.Add(mergeTarget);
                Debug.Log($"Room {currentNode.Room.Name} merged into room {mergeTarget.Room.Name}.");
            }
        }
        // Center-align X positions of newPathEnds
        float startX = -(newPathEnds.Count - 1) * 0.5f * horizontalSpacing;
        for (int i = 0; i < newPathEnds.Count; i++)
        {
            newPathEnds[i].Position = new Vector2(startX + i * horizontalSpacing, currentY * -verticalSpacing);
        }
        currentY++;

        return newPathEnds;
    }


    /// <summary>
    /// Connects the end of a divergent path to the mini-boss hub.
    /// </summary>
    private void ConnectToMiniBossHub(List<MapNode> pathEndNodes, List<MapNode> miniBosses)
{
    // If there are only two paths, force one to split to get a third path.
    if (pathEndNodes.Count < 3)
    {
        MapNode pathToEnd = new MapNode { Room = GetNewRoom() };
        pathEndNodes[0].Exits.Add(pathToEnd);
        pathEndNodes.Add(pathToEnd);
    }

    // Calculate center X based on previous level
    float averageX = pathEndNodes.Average(n => n.Position.x);
    float y = currentY * -verticalSpacing;

    // Spread mini-bosses slightly around center
    float spacing = horizontalSpacing;
    miniBosses[0].Position = new Vector2(averageX, y);
    miniBosses[1].Position = new Vector2(averageX + spacing, y);
    miniBosses[2].Position = new Vector2(averageX + (spacing*200), y);
    currentY++;

    // Connect each path end to a mini-boss in round robin
    int bossIndex = 0;
    foreach (var endNode in pathEndNodes)
    {
        MapNode bossToConnect = miniBosses[bossIndex];
        endNode.Exits.Add(bossToConnect);
        Debug.Log($"Path ending at room {endNode.Room.Name} connected to mini-boss {bossToConnect.Room.Name}.");
        bossIndex = (bossIndex + 1) % miniBosses.Count;
    }
}


    /// <summary>
    /// Builds a single path from the mini-bosses to the final boss.
    /// </summary>
    private void BuildFinalPath(List<MapNode> miniBosses)
    {
        // The start of the final path is the mini-bosses.
        List<MapNode> currentPathEnds = new List<MapNode>(miniBosses);

        // Build the next 5 rooms.
        for (int i = 0; i < roomsToFinalBoss; i++)
        {
            currentPathEnds = GenerateNextLevel(currentPathEnds);
        }

        // Connect all the last nodes to a single final boss room.
        MapNode finalBossNode = new MapNode { Room = GetNewRoom(RoomType.FinalBoss) };
        foreach (var endNode in currentPathEnds)
        {
            endNode.Exits.Add(finalBossNode);
        }
        // Calculate position
        float averageX = currentPathEnds.Average(n => n.Position.x);
        float y = currentY * -verticalSpacing;
        finalBossNode.Position = new Vector2(averageX, y);
        currentY++;

        foreach (var endNode in currentPathEnds)
        {
            endNode.Exits.Add(finalBossNode);
        }

    }

    /// <summary>
    /// Gets a new room with a random type.
    /// </summary>
    private Room GetNewRoom(RoomType forcedType = RoomType.Normal)
    {
        Room newRoom = new Room { Name = roomCounter };
        roomCounter++;

        if (forcedType == RoomType.Normal)
        {
            int totalTypes = 4;
            int randomType = Random.Range(0, totalTypes);
            switch (randomType)
            {
                case 0: newRoom.Type = RoomType.Normal; break;
                case 1: newRoom.Type = RoomType.Grass; break;
                case 2: newRoom.Type = RoomType.Water; break;
                case 3: newRoom.Type = RoomType.Fire; break;
            }
        }
        else
        {
            newRoom.Type = forcedType;
        }
        return newRoom;
    }
    
    private void CenterMap(MapNode root)
    {
        List<MapNode> allNodes = new List<MapNode>();
        HashSet<MapNode> visited = new HashSet<MapNode>();

        void Traverse(MapNode node)
        {
            if (visited.Contains(node)) return;
            visited.Add(node);
            allNodes.Add(node);
            foreach (var exit in node.Exits)
                Traverse(exit);
        }

        Traverse(root);

        // Get bounds
        float minX = allNodes.Min(n => n.Position.x);
        float maxX = allNodes.Max(n => n.Position.x);
        float centerX = (minX + maxX) / 2f;

        // Shift all nodes so center is at 0
        foreach (var node in allNodes)
        {
            node.Position = new Vector2(node.Position.x - centerX, node.Position.y);
        }
    }
}