using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MapGenerator
{
    private const int roomsToMiniBoss = 6;
    private const int roomsToFinalBoss = 5;

    public enum RoomType { Normal, Grass, Water, Fire, MiniBoss, FinalBoss }

    private int currentY = 0;
    private float horizontalSpacing = 2f;
    private float verticalSpacing = -2f;

    public class Room
    {
        public int Name { get; set; }
        public RoomType Type { get; set; }
    }

    public class MapNode
    {
        public Room Room { get; set; }
        public List<MapNode> Exits { get; set; } = new List<MapNode>();
        public Vector2 Position { get; set; }
        public MapNode Parent { get; set; }
    }

    public MapGenerator() { }

    public MapNode GenerateMap()
    {
        MapNode startNode = new MapNode { Room = new Room { Name = 1, Type = RoomType.Normal } };
        startNode.Position = new Vector2(0, 0);
        currentY++;

        List<MapNode> currentPathEnds = new List<MapNode>();

        MapNode path1Start = new MapNode { Room = GetNewRoom(null, startNode), Parent = startNode };
        MapNode path2Start = new MapNode { Room = GetNewRoom(null, startNode), Parent = startNode };
        startNode.Exits.Add(path1Start);
        startNode.Exits.Add(path2Start);

        path1Start.Position = new Vector2(-horizontalSpacing, -verticalSpacing);
        path2Start.Position = new Vector2(horizontalSpacing, -verticalSpacing);
        currentY++;

        currentPathEnds.Add(path1Start);
        currentPathEnds.Add(path2Start);

        for (int i = 2; i <= roomsToMiniBoss; i++)
        {
            currentPathEnds = GenerateNextLevel(currentPathEnds);
        }

        while (currentPathEnds.Count < 3)
        {
            int indexToSplit = Random.Range(0, currentPathEnds.Count);
            MapNode nodeToSplit = currentPathEnds[indexToSplit];

            MapNode newPath1 = new MapNode { Room = GetNewRoom(null, nodeToSplit), Parent = nodeToSplit };
            MapNode newPath2 = new MapNode { Room = GetNewRoom(null, nodeToSplit), Parent = nodeToSplit };
            nodeToSplit.Exits.Add(newPath1);
            nodeToSplit.Exits.Add(newPath2);

            float baseX = nodeToSplit.Position.x;
            newPath1.Position = new Vector2(baseX - horizontalSpacing / 2, currentY * -verticalSpacing);
            newPath2.Position = new Vector2(baseX + horizontalSpacing / 2, currentY * -verticalSpacing);

            currentPathEnds.RemoveAt(indexToSplit);
            currentPathEnds.Add(newPath1);
            currentPathEnds.Add(newPath2);

            currentY++;
        }

        MapNode miniBoss1 = new MapNode { Room = new Room { Name = -1, Type = RoomType.MiniBoss } };
        MapNode miniBoss2 = new MapNode { Room = new Room { Name = -2, Type = RoomType.MiniBoss } };
        MapNode miniBoss3 = new MapNode { Room = new Room { Name = -3, Type = RoomType.MiniBoss } };

        ConnectToMiniBossHub(currentPathEnds, new List<MapNode> { miniBoss1, miniBoss2, miniBoss3 });

        BuildFinalPath(new List<MapNode> { miniBoss1, miniBoss2, miniBoss3 });

        CenterMap(startNode);
        return startNode;
    }

    private List<MapNode> GenerateNextLevel(List<MapNode> pathEnds)
    {
        List<MapNode> newPathEnds = new List<MapNode>();
        var intents = new List<string>();

        foreach (var node in pathEnds)
        {
            float roll = Random.Range(0f, 1f);
            if (node.Exits.Count >= 2) intents.Add("Merge");
            else if (roll < 0.33f) intents.Add("Split");
            else if (roll < 0.66f) intents.Add("Merge");
            else intents.Add("Continue");
        }

        for (int i = 0; i < pathEnds.Count; i++)
        {
            var currentNode = pathEnds[i];
            string intent = intents[i];
            MapNode mergeTarget = null;

            if (intent == "Merge")
            {
                if (i > 0 && intents[i - 1] == "Continue" && pathEnds[i - 1].Exits.Count > 0)
                {
                    mergeTarget = pathEnds[i - 1].Exits.Last();
                }
                else if (i < intents.Count - 1 && intents[i + 1] == "Merge")
                {
                    mergeTarget = new MapNode { Room = GetNewRoom(null, currentNode), Parent = currentNode };
                    currentNode.Exits.Add(mergeTarget);
                    pathEnds[i + 1].Exits.Add(mergeTarget);
                    newPathEnds.Add(mergeTarget);
                    i++;
                    continue;
                }

                if (mergeTarget == null)
                {
                    MapNode newNode = new MapNode { Room = GetNewRoom(null, currentNode), Parent = currentNode };
                    currentNode.Exits.Add(newNode);
                    newPathEnds.Add(newNode);
                    continue;
                }
                else
                {
                    currentNode.Exits.Add(mergeTarget);
                    newPathEnds.Add(mergeTarget);
                    continue;
                }
            }

            if (intent == "Split")
            {
                int remaining = 2 - currentNode.Exits.Count;
                if (remaining <= 0)
                {
                    MapNode fallback = new MapNode { Room = GetNewRoom(null, currentNode), Parent = currentNode };
                    currentNode.Exits.Add(fallback);
                    newPathEnds.Add(fallback);
                }
                else if (remaining == 1)
                {
                    MapNode onlyChild = new MapNode { Room = GetNewRoom(null, currentNode), Parent = currentNode };
                    currentNode.Exits.Add(onlyChild);
                    newPathEnds.Add(onlyChild);
                }
                else
                {
                    MapNode left = new MapNode { Room = GetNewRoom(null, currentNode), Parent = currentNode };
                    MapNode right = new MapNode { Room = GetNewRoom(null, currentNode), Parent = currentNode };
                    currentNode.Exits.Add(left);
                    currentNode.Exits.Add(right);
                    newPathEnds.Add(left);
                    newPathEnds.Add(right);
                }
            }
            else if (intent == "Continue")
            {
                MapNode newNode = new MapNode { Room = GetNewRoom(null, currentNode), Parent = currentNode };
                currentNode.Exits.Add(newNode);
                newPathEnds.Add(newNode);
            }
            else if (intent == "Merge" && mergeTarget != null)
            {
                currentNode.Exits.Add(mergeTarget);
                newPathEnds.Add(mergeTarget);
            }
        }

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

    private void ConnectToMiniBossHub(List<MapNode> pathEndNodes, List<MapNode> miniBosses)
    {
        float minX = pathEndNodes.Min(n => n.Position.x);
        float maxX = pathEndNodes.Max(n => n.Position.x);
        float centerX = (minX + maxX) / 2f;
        float y = currentY * -verticalSpacing;

        miniBosses[0].Position = new Vector2(minX, y);
        miniBosses[1].Position = new Vector2(centerX, y);
        miniBosses[2].Position = new Vector2(maxX, y);
        currentY++;

        foreach (var endNode in pathEndNodes)
        {
            MapNode closestBoss = miniBosses
                .OrderBy(boss => Mathf.Abs(boss.Position.x - endNode.Position.x))
                .First();

            endNode.Exits.Add(closestBoss);
        }
    }

    private void BuildFinalPath(List<MapNode> miniBosses)
    {
        List<MapNode> currentPathEnds = new List<MapNode>(miniBosses);

        for (int i = 0; i < roomsToFinalBoss; i++)
        {
            currentPathEnds = GenerateNextLevel(currentPathEnds);
        }

        MapNode finalBossNode = new MapNode { Room = new Room { Name = -99, Type = RoomType.FinalBoss } };
        float y = currentY * -verticalSpacing;
        finalBossNode.Position = new Vector2(0, y);
        currentY++;

        foreach (var endNode in currentPathEnds)
        {
            endNode.Exits.Add(finalBossNode);
        }
    }

    private Room GetNewRoom(RoomType? forcedType = null, MapNode currentNode = null)
    {
        if (forcedType.HasValue)
        {
            return new Room { Name = -Random.Range(100, 1000), Type = forcedType.Value };
        }

        HashSet<int> usedRooms = new HashSet<int>();
        MapNode node = currentNode;
        while (node != null)
        {
            usedRooms.Add(node.Room.Name);
            node = node.Parent;
        }

        int candidate;
        do
        {
            candidate = Random.Range(2, 21); // exclude 1 so starter never repeats
        } while (usedRooms.Contains(candidate));

        Room newRoom = new Room { Name = candidate };

        if (candidate <= 5) newRoom.Type = RoomType.Normal;
        else if (candidate <= 10) newRoom.Type = RoomType.Grass;
        else if (candidate <= 15) newRoom.Type = RoomType.Water;
        else newRoom.Type = RoomType.Fire;

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

        float minX = allNodes.Min(n => n.Position.x);
        float maxX = allNodes.Max(n => n.Position.x);
        float centerX = (minX + maxX) / 2f;

        foreach (var node in allNodes)
        {
            node.Position = new Vector2(node.Position.x - centerX, node.Position.y);
        }
    }
}