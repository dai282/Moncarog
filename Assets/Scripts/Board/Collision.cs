using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class RoomGrid : MonoBehaviour
{
    public bool Event = false;
    public Tilemap collisionTilemap;
    public Tilemap decorations;
    public Tilemap floor;
    public int roomGridID;
    private Dictionary<Vector3Int, int> encounterGroups = new Dictionary<Vector3Int, int>();
    public int groupsToPlace = 5;
    public int tilesPerGroup = 5;

    public enum CellType
    {
        Walkable,
        Unwalkable,
        Door,
        Encounter,
        Event
    }

    private Dictionary<Vector3Int, CellType> cellData = new Dictionary<Vector3Int, CellType>();
    private Dictionary<Vector3Int, DoorDetector> doors = new Dictionary<Vector3Int, DoorDetector>();
    private HashSet<Vector3Int> encounterTiles = new HashSet<Vector3Int>();

    void Awake()
    {
        GenerateCellData();
    }

    void GenerateCellData()
    {
        BoundsInt bounds = collisionTilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                if (collisionTilemap.HasTile(pos))
                {
                    cellData[pos] = CellType.Unwalkable;
                }
                else if (decorations.HasTile(pos))
                {
                    cellData[pos] = CellType.Walkable;
                }
                else if (floor.HasTile(pos))
                {
                    cellData[pos] = CellType.Walkable;
                }
                else
                {
                    cellData[pos] = CellType.Unwalkable;
                }
            }
        }
       
    }

    public void PlaceEncounterTiles(int roomID)
    {
        roomGridID = roomID;
        // Collect all walkable cells
        List<Vector3Int> walkableCells = new List<Vector3Int>();
        foreach (var kvp in cellData)
        {
            if (kvp.Value == CellType.Walkable)
                walkableCells.Add(kvp.Key);
        }

        //if boss or mini boss room, only 1 encounter tile at the middle of the room
        if( roomID == -10 || roomID == -11)
        {
            if (Event)
            {
                Vector3Int eventTilePos = new Vector3Int(4, -3, 0);

                if (cellData.ContainsKey(eventTilePos) && cellData[eventTilePos] == CellType.Walkable)
                {
                    cellData[eventTilePos] = CellType.Event;

                    Debug.Log($"Event tile placed at {eventTilePos}");
                }
                else
                {
                    Debug.LogWarning($"Event tile position {eventTilePos} not valid or not walkable.");
                }
            }

        }
        else if (roomID < 0)
        {
            BoundsInt bounds = collisionTilemap.cellBounds;

            int x = (bounds.xMin + bounds.xMax) / 2;
            int y = (bounds.yMin + bounds.yMax) / 2;
            Vector3Int centerPos = new Vector3Int(x, y, 0);
            if (cellData.ContainsKey(centerPos) && cellData[centerPos] == CellType.Walkable)
            {
                cellData[centerPos] = CellType.Encounter;
                encounterTiles.Add(centerPos);
                //Debug.Log($"Boss/ Mini Boss Encounter tile placed at {centerPos}");
            }
            else
            {
                //Debug.LogWarning($"Center position {centerPos} is not walkable, cannot place encounter tile.");
            }
        }
        else
        {
            //no encounter in first room
            if (roomID != 1)
            {
                // Randomly pick encounter tiles
                for (int group = 0; group < groupsToPlace; group++)
                {
                    if (walkableCells.Count == 0)
                        break;

                    int tilesAdded = 0;

                    for (int i = 0; i < tilesPerGroup && walkableCells.Count > 0; i++)
                    {
                        int randIndex = Random.Range(0, walkableCells.Count);
                        Vector3Int chosenCell = walkableCells[randIndex];
                        walkableCells.RemoveAt(randIndex);

                        AddEncounterTile(chosenCell, group);
                        tilesAdded++;
                    }

                    //Debug.Log($"Encounter group {group} placed with {tilesAdded} tiles.");
                }
            }
            else
            {
                //Debug.Log($"No encounter cells in the first room");
            }

        }
        
    }

    private void AddEncounterTile(Vector3Int pos, int groupID)
    {
        cellData[pos] = CellType.Encounter;
        encounterTiles.Add(pos);
        encounterGroups[pos] = groupID;
    }

    private List<Vector3Int> GetNearbyWalkableCells(Vector3Int center, List<Vector3Int> candidates, int radius)
    {
        List<Vector3Int> nearby = new List<Vector3Int>();

        foreach (Vector3Int cell in candidates)
        {
            if (Mathf.Abs(cell.x - center.x) <= radius && Mathf.Abs(cell.y - center.y) <= radius)
            {
                nearby.Add(cell);
            }
        }

        return nearby;
    }

    public bool IsWalkable(Vector3 worldPos)
    {
        Vector3Int cellPos = collisionTilemap.WorldToCell(worldPos);

        if (cellData.TryGetValue(cellPos, out CellType type))
        {
            return type != CellType.Unwalkable;
        }

        return true;
    }

    public bool IsEncounterTile(Vector3 worldPos, out Vector3Int cellPos)
    {
        cellPos = collisionTilemap.WorldToCell(worldPos);
        return encounterTiles.Contains(cellPos);
    }

    public bool IsEventTile(Vector3 worldPos, out Vector3Int cellPos)
    {
        cellPos = collisionTilemap.WorldToCell(worldPos);

        if (cellData.TryGetValue(cellPos, out CellType type))
        {
            return type == CellType.Event;
        }

        return false;
    }

    public void ResetEncounterTile(Vector3Int cellPos)
    {
        if (!encounterTiles.Contains(cellPos))
            return;

        if (!encounterGroups.TryGetValue(cellPos, out int groupID))
            groupID = -1;

        // Find all tiles in the same group
        List<Vector3Int> tilesToRemove = new List<Vector3Int>();
        foreach (var kvp in encounterGroups)
        {
            if (kvp.Value == groupID)
                tilesToRemove.Add(kvp.Key);
        }

        // Reset all tiles in the group
        foreach (Vector3Int tile in tilesToRemove)
        {
            cellData[tile] = CellType.Walkable;
            encounterTiles.Remove(tile);
            encounterGroups.Remove(tile);
        }

        //Debug.Log($"Encounter group {groupID} cleared ({tilesToRemove.Count} tiles reset to Walkable).");
    }

    public void ResetEventTile(Vector3Int cellPos)
    {
        if (cellData.TryGetValue(cellPos, out CellType type) && type == CellType.Event)
        {
            // Remove or reset event type
            cellData[cellPos] = CellType.Walkable;

            // Optional: Clear the tile visually if your tilemap uses event tiles
            if (collisionTilemap != null)
            {
                collisionTilemap.SetTile(cellPos, null);
            }

            Debug.Log($"Removed event tile at {cellPos}");
        }
        else
        {
            Debug.LogWarning($"No event tile found at {cellPos} to remove.");
        }
    }

    void PrintUnwalkableTiles()
    {
        foreach (var pos in collisionTilemap.cellBounds.allPositionsWithin)
        {
            if (collisionTilemap.HasTile(pos))
            {
                Vector3 worldPos = collisionTilemap.CellToWorld(pos);
                //Debug.Log($"Unwalkable Tile at Cell: {pos} | World Position: {worldPos}");
            }
        }
    }

    public void PrintDoorTiles()
        {
            foreach (var kvp in cellData)
            {
                Vector3Int cellPos = kvp.Key;
                CellType type = kvp.Value;

                if (type == CellType.Door)
                {
                    DoorDetector door = GetDoorAtCell(cellPos);
                    string doorName = door != null ? door.gameObject.name : "NoDoorObject";
                    //Debug.Log($"Door tile at cell: {cellPos} | Door object: {doorName}");
                }
            }
        }

    public void RegisterDoor(Vector3Int cellPos, DoorDetector door)
    {
        cellData[cellPos] = CellType.Door;
        doors[cellPos] = door;
        //Debug.Log($"Registered door at cell {cellPos} (world {collisionTilemap.GetCellCenterWorld(cellPos)})");
    }

    public void RegisterUnwalkable(Vector3Int cellPos)
    {
        if (cellData.ContainsKey(cellPos))
        {
            cellData[cellPos] = CellType.Unwalkable;
            //Debug.Log($"Registered collision at cell {cellPos} (world {collisionTilemap.GetCellCenterWorld(cellPos)})");
        }
        else
        {
            cellData[cellPos] = CellType.Unwalkable;
            //Debug.LogWarning($"Cell {cellPos} was not in cellData; added as Unwalkable.");
        }
    }

    public DoorDetector GetDoorAtCell(Vector3Int cellPos)
    {
        doors.TryGetValue(cellPos, out DoorDetector door);
        return door;
    }

    private void OnDrawGizmos()
    {
        if (encounterTiles == null || encounterTiles.Count == 0)
            return;

        // Distinguish groups by color
        Color[] groupColors =
        {
            Color.red, Color.blue, Color.green, Color.yellow, Color.magenta,
            new Color(1f, 0.5f, 0f), // orange
            new Color(0f, 1f, 1f),   // cyan
            new Color(0.6f, 0.3f, 1f) // purple-ish
        };

        foreach (var tile in encounterTiles)
        {
            int groupID = -1;
            if (encounterGroups != null && encounterGroups.TryGetValue(tile, out int id))
                groupID = id;

            // Pick color for group (wrap if more groups than colors)
            Color gizmoColor = groupColors[Mathf.Abs(groupID) % groupColors.Length];

            Gizmos.color = gizmoColor;
            Vector3 worldPos = collisionTilemap.CellToWorld(tile) + new Vector3(0.5f, 0.5f, 0); // center of tile

            // Draw a solid cube for the tile
            Gizmos.DrawCube(worldPos, new Vector3(0.8f, 0.8f, 0.1f));

            // Optional: draw group label above tile
    #if UNITY_EDITOR
            UnityEditor.Handles.color = gizmoColor;
            UnityEditor.Handles.Label(worldPos + Vector3.up * 0.4f, $"G{groupID}");
    #endif
        }
    }
}