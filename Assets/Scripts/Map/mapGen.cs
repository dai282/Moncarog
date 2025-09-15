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
        public List<MapNode> Parents { get; set; } = new List<MapNode>();
        public Vector2 Position { get; set; }
    }

    public MapGenerator() { }

    public MapNode GenerateMap()
    {
        // Start node forced to ID 1 and Normal type.
        MapNode startNode = new MapNode { Room = new Room { Name = 1, Type = RoomType.Normal } };
        startNode.Position = new Vector2(0, 0);
        currentY++;

        List<MapNode> currentPathEnds = new List<MapNode>();

        // Forced initial split (two children)
        MapNode path1Start = CreatePlaceholderNode();
        MapNode path2Start = CreatePlaceholderNode();
        startNode.Exits.Add(path1Start);
        startNode.Exits.Add(path2Start);
        path1Start.Parents.Add(startNode);
        path2Start.Parents.Add(startNode);

        path1Start.Position = new Vector2(-horizontalSpacing / 2, currentY * -verticalSpacing);
        path2Start.Position = new Vector2(horizontalSpacing / 2, currentY * -verticalSpacing);
        currentY++;

        currentPathEnds.Add(path1Start);
        currentPathEnds.Add(path2Start);

        // Build levels up to miniboss layer
        for (int i = 2; i <= roomsToMiniBoss; i++)
            currentPathEnds = GenerateNextLevel(currentPathEnds);

        // Ensure at least 3 end paths before minibosses
        while (currentPathEnds.Count < 3)
        {
            // Safeguard to prevent an infinite loop in edge cases
            if (currentPathEnds.Count == 0 || currentY > roomsToMiniBoss + 5) break;

            List<MapNode> newPathEnds = new List<MapNode>();

            // Randomly pick one of the current paths to split
            int indexToSplit = Random.Range(0, currentPathEnds.Count);

            // Iterate through all nodes on the current last layer
            for (int i = 0; i < currentPathEnds.Count; i++)
            {
                MapNode currentNode = currentPathEnds[i];

                if (i == indexToSplit)
                {
                    // This is the node we're splitting. Create two children in the next layer.
                    MapNode childA = CreatePlaceholderNode();
                    MapNode childB = CreatePlaceholderNode();
                    
                    currentNode.Exits.Add(childA);
                    currentNode.Exits.Add(childB);
                    childA.Parents.Add(currentNode);
                    childB.Parents.Add(currentNode);
                    
                    newPathEnds.Add(childA);
                    newPathEnds.Add(childB);
                }
                else
                {
                    // All other nodes just continue forward. Create one child in the next layer.
                    MapNode continuationChild = CreatePlaceholderNode();

                    currentNode.Exits.Add(continuationChild);
                    continuationChild.Parents.Add(currentNode);

                    newPathEnds.Add(continuationChild);
                }
            }

            // Now, position all the newly created nodes on the next layer
            float startX = -(newPathEnds.Count - 1) * 0.5f * horizontalSpacing;
            for (int i = 0; i < newPathEnds.Count; i++)
            {
                newPathEnds[i].Position = new Vector2(startX + i * horizontalSpacing, currentY * -verticalSpacing);
            }

            // The new nodes are now our current path ends, and we advance to the next Y-level
            currentPathEnds = newPathEnds;
            currentY++;
        }

        // Create miniboss placeholders (special types assigned later)
        MapNode miniBoss1 = new MapNode { Room = new Room { Name = -1, Type = RoomType.MiniBoss } };
        MapNode miniBoss2 = new MapNode { Room = new Room { Name = -2, Type = RoomType.MiniBoss } };
        MapNode miniBoss3 = new MapNode { Room = new Room { Name = -3, Type = RoomType.MiniBoss } };

        ConnectToMiniBossHub(currentPathEnds, new List<MapNode> { miniBoss1, miniBoss2, miniBoss3 });

        // Build final path levels after minibosses
        BuildFinalPath(new List<MapNode> { miniBoss1, miniBoss2, miniBoss3 });

        AssignRoomIDs(startNode);

        CenterMap(startNode);
        return startNode;
    }

    // Create a placeholder node (no assigned room ID yet)
    private MapNode CreatePlaceholderNode()
    {
        return new MapNode { Room = new Room { Name = 0, Type = RoomType.Normal } };
    }

    private List<MapNode> GenerateNextLevel(List<MapNode> pathEnds)
    {
        List<MapNode> newPathEnds = new List<MapNode>();
        List<MapNode> tempPathEnds = new List<MapNode>(pathEnds);

        // This while loop ensures that we process all nodes from the previous layer, even if we skip one during a merge.
        while (tempPathEnds.Count > 0)
        {
            MapNode currentNode = tempPathEnds[0];
            tempPathEnds.RemoveAt(0);
            float roll = Random.Range(0f, 1f);

            // Split (33% chance)
            if (roll < 0.33f)
            {
                MapNode left = CreatePlaceholderNode();
                MapNode right = CreatePlaceholderNode();
                currentNode.Exits.Add(left);
                currentNode.Exits.Add(right);
                left.Parents.Add(currentNode);
                right.Parents.Add(currentNode);
                newPathEnds.Add(left);
                newPathEnds.Add(right);
            }
            // Merge (33% chance)
            else if (roll < 0.66f && tempPathEnds.Count > 0)
            {
                // Find a partner to merge with (e.g, the next node in the list)
                MapNode mergePartner = tempPathEnds[0];
                tempPathEnds.RemoveAt(0);

                MapNode mergedNode = CreatePlaceholderNode();
                currentNode.Exits.Add(mergedNode);
                mergePartner.Exits.Add(mergedNode);
                mergedNode.Parents.Add(currentNode);
                mergedNode.Parents.Add(mergePartner);
                newPathEnds.Add(mergedNode);
            }
            // Continue (34% chance, or as a fallback)
            else
            {
                MapNode single = CreatePlaceholderNode();
                currentNode.Exits.Add(single);
                single.Parents.Add(currentNode);
                newPathEnds.Add(single);
            }
        }

        // Position nodes for this level
        if (newPathEnds.Count > 0)
        {
            // Re-center nodes horizontally
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
            // attach each path end to the closest miniboss
            MapNode closestBoss = miniBosses.OrderBy(b => Mathf.Abs(b.Position.x - endNode.Position.x)).First();
            endNode.Exits.Add(closestBoss);
            closestBoss.Parents.Add(endNode);
        }
    }

    private void BuildFinalPath(List<MapNode> miniBosses)
    {
        List<MapNode> currentPathEnds = new List<MapNode>(miniBosses);

        for (int i = 0; i < roomsToFinalBoss; i++)
            currentPathEnds = GenerateNextLevel(currentPathEnds);

        MapNode finalBossNode = new MapNode { Room = new Room { Name = -99, Type = RoomType.FinalBoss } };
        float y = currentY * -verticalSpacing;
        finalBossNode.Position = new Vector2(0, y);
        currentY++;

        foreach (var endNode in currentPathEnds)
        {
            endNode.Exits.Add(finalBossNode);
            finalBossNode.Parents.Add(endNode);
        }
    }

    private void AssignRoomIDs(MapNode root)
    {
        // collect all nodes
        List<MapNode> all = new List<MapNode>();
        HashSet<MapNode> seen = new HashSet<MapNode>();

        void Traverse(MapNode n)
        {
            if (n == null || seen.Contains(n)) return;
            seen.Add(n);
            all.Add(n);
            foreach (var e in n.Exits) Traverse(e);
        }
        Traverse(root);

        // build indegree (parents count) and processed count
        var pendingParents = new Dictionary<MapNode, int>();
        var processedParents = new Dictionary<MapNode, int>();
        foreach (var n in all)
        {
            pendingParents[n] = n.Parents.Count;
            processedParents[n] = 0;
        }

        // usedIDs per node = set of IDs present on any path from start to that node (including node once assigned)
        var usedIds = new Dictionary<MapNode, HashSet<int>>();

        var q = new Queue<MapNode>();
        // enqueue nodes with zero parents (start expected)
        foreach (var n in all) if (pendingParents[n] == 0) q.Enqueue(n);

        while (q.Count > 0)
        {
            var node = q.Dequeue();

            // union of parents' used sets
            var union = new HashSet<int>();
            foreach (var p in node.Parents)
            {
                if (usedIds.TryGetValue(p, out var set)) union.UnionWith(set);
            }

            // Assign ID if placeholder (Name == 0). Start and miniboss/final keep their preassigned names (<0 or 1)
            if (node.Room == null) node.Room = new Room { Name = 0, Type = RoomType.Normal };

            if (node.Room.Name == 0)
            {
                // choose from 2..20 excluding union
                var candidates = Enumerable.Range(2, 19).Where(x => !union.Contains(x)).ToList();
                int pick = candidates.Count > 0 ? candidates[Random.Range(0, candidates.Count)] : Random.Range(2, 21);
                node.Room.Name = pick;
                node.Room.Type = TypeFromId(pick);
            }

            // build used set for this node
            var myUsed = new HashSet<int>(union);
            if (node.Room.Name > 0) myUsed.Add(node.Room.Name);
            usedIds[node] = myUsed;

            // notify children
            foreach (var child in node.Exits)
            {
                processedParents[child]++;
                if (processedParents[child] >= pendingParents[child])
                    q.Enqueue(child);
            }
        }
    }

    private RoomType TypeFromId(int id)
    {
        if (id <= 5) return RoomType.Normal;
        if (id <= 10) return RoomType.Grass;
        if (id <= 15) return RoomType.Water;
        return RoomType.Fire;
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
            node.Position = new Vector2(node.Position.x - centerX, node.Position.y);
    }
}
