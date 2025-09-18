using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class RoomGrid : MonoBehaviour
{
    public Tilemap collisionTilemap;

    public enum CellType
    {
        Walkable,
        Unwalkable,
        Door
    }

    private Dictionary<Vector3Int, CellType> cellData = new Dictionary<Vector3Int, CellType>();

    private Dictionary<Vector3Int, DoorDetector> doors = new Dictionary<Vector3Int, DoorDetector>();

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
                else
                {
                    cellData[pos] = CellType.Walkable;
                }
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