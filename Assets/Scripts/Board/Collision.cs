using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class RoomGrid : MonoBehaviour
{
    public Tilemap collisionTilemap;
    public Tilemap decorations;
    public Tilemap floor;
    public int roomGridID;

    public enum CellType
    {
        Walkable,
        Unwalkable,
        Door,
        Encounter
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
        //PlaceEncounterTiles();
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
        if (roomID < 0)
        {
            BoundsInt bounds = collisionTilemap.cellBounds;

            int x = (bounds.xMin + bounds.xMax) / 2;
            int y = (bounds.yMin + bounds.yMax) / 2;
            Vector3Int centerPos = new Vector3Int(x, y, 0);
            if (cellData.ContainsKey(centerPos) && cellData[centerPos] == CellType.Walkable)
            {
                cellData[centerPos] = CellType.Encounter;
                encounterTiles.Add(centerPos);
                Debug.Log($"Boss/ Mini Boss Encounter tile placed at {centerPos}");
            }
            else
            {
                Debug.LogWarning($"Center position {centerPos} is not walkable, cannot place encounter tile.");
            }
        }
        else
        {
            //no encounter in first room
            if (roomID != 1)
            {
                // Randomly pick encounter tiles
                for (int i = 0; i < 10 && walkableCells.Count > 0; i++)
                {
                    int randIndex = Random.Range(0, walkableCells.Count);
                    Vector3Int chosen = walkableCells[randIndex];
                    walkableCells.RemoveAt(randIndex);

                    cellData[chosen] = CellType.Encounter;
                    encounterTiles.Add(chosen);

                    Debug.Log($"Encounter tile placed at {chosen}");
                }
            }
            else
            {
                Debug.Log($"No encounter cells in the first room");
            }

        }
        
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

    public void ResetEncounterTile(Vector3Int cellPos)
    {
        if (encounterTiles.Contains(cellPos))
        {
            cellData[cellPos] = CellType.Walkable;
            encounterTiles.Remove(cellPos);
            Debug.Log($"Encounter tile at {cellPos} reset to Walkable");
        }
    }

    void PrintUnwalkableTiles()
    {
        foreach (var pos in collisionTilemap.cellBounds.allPositionsWithin)
        {
            if (collisionTilemap.HasTile(pos))
            {
                Vector3 worldPos = collisionTilemap.CellToWorld(pos);
                Debug.Log($"Unwalkable Tile at Cell: {pos} | World Position: {worldPos}");
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
                    Debug.Log($"Door tile at cell: {cellPos} | Door object: {doorName}");
                }
            }
        }

    public void RegisterDoor(Vector3Int cellPos, DoorDetector door)
    {
        cellData[cellPos] = CellType.Door;
        doors[cellPos] = door;
        Debug.Log($"Registered door at cell {cellPos} (world {collisionTilemap.GetCellCenterWorld(cellPos)})");
    }

    public DoorDetector GetDoorAtCell(Vector3Int cellPos)
    {
        doors.TryGetValue(cellPos, out DoorDetector door);
        return door;
    }
}