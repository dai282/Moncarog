using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class RoomGrid : MonoBehaviour
{
    public Tilemap collisionTilemap;

    private Dictionary<Vector3Int, bool> cellData = new Dictionary<Vector3Int, bool>();

    void Awake()
    {
        GenerateCellData();
        //Test whether the tiles are being added as unwalkable
        //PrintUnwalkableTiles();
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
                    cellData[pos] = false;
                }
                else
                {
                    cellData[pos] = true;
                }
            }
        }

    }

    public bool IsWalkable(Vector3 worldPos)
    {
        Vector3Int cellPos = collisionTilemap.WorldToCell(worldPos);

        if (cellData.TryGetValue(cellPos, out bool walkable))
        {
            return walkable;
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
}
