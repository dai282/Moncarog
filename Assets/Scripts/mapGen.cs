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
    private float verticalSpacing = -2f;

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

    // Generates the complete map graph from start to finish.
    // <returns>The starting MapNode of the generated map.
    public MapNode GenerateMap()
    {
        // 1. Create the starting room (always Normal)
        MapNode startNode = new MapNode { Room = GetNewRoom(RoomType.Normal) };
        startNode.Position = new Vector2(0, 0);
        currentY++;

        Debug.Log("Generating map with forced initial split.");

        // We will keep a list of the end nodes of all active paths.
        List<MapNode> currentPathEnds = new List<MapNode>();

        // 2. Forced initial split into two paths.
        MapNode path1Start = new MapNode { Room = GetNewRoom() };
        MapNode path2Start = new MapNode { Room = GetNewRoom() };
        startNode.Exits.Add(path1Start);
        startNode.Exits.Add(path2Start);

        // Position the initial split nodes
        path1Start.Position = new Vector2(-horizontalSpacing, -verticalSpacing);
        path2Start.Position = new Vector2(horizontalSpacing, -verticalSpacing);
        currentY++;

        currentPathEnds.Add(path1Start);
        currentPathEnds.Add(path2Start);

        // 3. Iteratively build the divergent paths.
        for (int i = 2; i <= roomsToMiniBoss; i++)
        {
            currentPathEnds = GenerateNextLevel(currentPathEnds);
        }

        // 4. Ensure we have exactly 3 paths before creating mini-bosses
        while (currentPathEnds.Count < 3)
        {
            // If we have less than 3 paths, split one of them
            int indexToSplit = Random.Range(0, currentPathEnds.Count);
            MapNode nodeToSplit = currentPathEnds[indexToSplit];

            MapNode newPath1 = new MapNode { Room = GetNewRoom() };
            MapNode newPath2 = new MapNode { Room = GetNewRoom() };
            nodeToSplit.Exits.Add(newPath1);
            nodeToSplit.Exits.Add(newPath2);

            // Position the new nodes
            float baseX = nodeToSplit.Position.x;
            newPath1.Position = new Vector2(baseX - horizontalSpacing / 2, currentY * -verticalSpacing);
            newPath2.Position = new Vector2(baseX + horizontalSpacing / 2, currentY * -verticalSpacing);

            // Remove the split node and add the new ones
            currentPathEnds.RemoveAt(indexToSplit);
            currentPathEnds.Add(newPath1);
            currentPathEnds.Add(newPath2);

            currentY++;
        }

        // 5. Create the mini-boss nodes.
        MapNode miniBoss1 = new MapNode { Room = GetNewRoom(RoomType.MiniBoss) };
        MapNode miniBoss2 = new MapNode { Room = GetNewRoom(RoomType.MiniBoss) };
        MapNode miniBoss3 = new MapNode { Room = GetNewRoom(RoomType.MiniBoss) };

        // 6. Connect the ends of the divergent paths to the mini-bosses.
        ConnectToMiniBossHub(currentPathEnds, new List<MapNode> { miniBoss1, miniBoss2, miniBoss3 });

        // 7. Build the final, converging path to the final boss.
        BuildFinalPath(new List<MapNode> { miniBoss1, miniBoss2, miniBoss3 });

        CenterMap(startNode);
        return startNode;
    }

    // Generates the next level of the map based on the current path ends.
    private List<MapNode> GenerateNextLevel(List<MapNode> pathEnds)
    {
        List<MapNode> newPathEnds = new List<MapNode>();
        Dictionary<MapNode, int> mergeCounts = new Dictionary<MapNode, int>();

        // Step 1: Roll each path to determine its intent.
        var intents = new List<string>();
        foreach (var node in pathEnds)
        {
            float roll = Random.Range(0f, 1f);
            Debug.Log($"testing");
            if (node.Exits.Count >= 2) intents.Add("Merge"); // Prevent over-splitting

            else if (roll < 0.33f)
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
                    // Merge into previous node's last child
                    mergeTarget = pathEnds[i - 1].Exits.Last();
                }
                else if (i < intents.Count - 1 && intents[i + 1] == "Merge")
                {
                    // Shared new node for two merges
                    mergeTarget = new MapNode { Room = GetNewRoom() };
                    currentNode.Exits.Add(mergeTarget);
                    pathEnds[i + 1].Exits.Add(mergeTarget);
                    newPathEnds.Add(mergeTarget);
                    i++;
                    continue;
                }

                // Fallback: if no mergeTarget, treat as Continue
                if (mergeTarget == null)
                {
                    MapNode newNode = new MapNode { Room = GetNewRoom() };
                    currentNode.Exits.Add(newNode);
                    newPathEnds.Add(newNode);
                    continue; // <-- critical: skip the Split/Continue code below
                }
                else
                {
                    currentNode.Exits.Add(mergeTarget);
                    newPathEnds.Add(mergeTarget);
                    continue; // <-- also critical
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
        if (newPathEnds.Count > 0)
        {
            float startX = -(newPathEnds.Count - 1) * 0.5f * horizontalSpacing;
            for (int i = 0; i < newPathEnds.Count; i++)
            {
                newPathEnds[i].Position = new Vector2(startX + i * horizontalSpacing, currentY * -verticalSpacing);
            }
        }
        currentY++;

        return newPathEnds;
    }


    // Connects the end of a divergent path to the mini-boss hub.

    private void ConnectToMiniBossHub(List<MapNode> pathEndNodes, List<MapNode> miniBosses)
    {
        // Calculate positions based on path end nodes
        float minX = pathEndNodes.Min(n => n.Position.x);
        float maxX = pathEndNodes.Max(n => n.Position.x);
        float centerX = (minX + maxX) / 2f;
        float y = currentY * -verticalSpacing;

        // Position mini-bosses based on the path structure
        Debug.Log(minX);
        Debug.Log(maxX);
        Debug.Log(centerX);
        miniBosses[0].Position = new Vector2(minX, y);
        miniBosses[1].Position = new Vector2(centerX, y);
        miniBosses[2].Position = new Vector2(maxX, y);
        currentY++;

        // Connect each path end to the closest mini-boss
        foreach (var endNode in pathEndNodes)
        {
            // Find the closest mini-boss
            MapNode closestBoss = miniBosses
                .OrderBy(boss => Mathf.Abs(boss.Position.x - endNode.Position.x))
                .First();

            endNode.Exits.Add(closestBoss);
            Debug.Log($"Path ending at room {endNode.Room.Name} connected to mini-boss {closestBoss.Room.Name}.");
        }
    }


    // Builds a single path from the mini-bosses to the final boss.

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

        // Calculate position based on the current path ends
        // float averageX = currentPathEnds.Average(n => n.Position.x);
        float y = currentY * -verticalSpacing;
        finalBossNode.Position = new Vector2(0, y);
        currentY++;

        foreach (var endNode in currentPathEnds)
        {
            endNode.Exits.Add(finalBossNode);
        }
    }

    // Gets a new room with a random type.

    private Room GetNewRoom(RoomType? forcedType = null)
    {
        Room newRoom = new Room { Name = roomCounter++ };

        if (forcedType.HasValue)
        {
            newRoom.Type = forcedType.Value;
        }
        else
        {
            int totalTypes = 4; // Normal, Grass, Water, Fire
            int randomType = Random.Range(0, totalTypes);
            switch (randomType)
            {
                case 0: newRoom.Type = RoomType.Normal; break;
                case 1: newRoom.Type = RoomType.Grass; break;
                case 2: newRoom.Type = RoomType.Water; break;
                case 3: newRoom.Type = RoomType.Fire; break;
            }
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