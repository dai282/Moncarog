using UnityEngine;
using System;

public class BoardManager : MonoBehaviour
{
    public GameObject[] roomPrefabs;
    private GameObject currentRoom;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public RoomGrid GenerateRoom(MapManager.RoomInfo room)
    {
        int roomID = Int32.Parse(room.roomName);

        if (roomID > 0) {
            currentRoom = Instantiate(roomPrefabs[roomID], Vector3.zero, Quaternion.identity);
        }
        else
        {
            //boss room
            if (roomID == -99)
            {
                currentRoom = Instantiate(roomPrefabs[roomPrefabs.Length - 1], Vector3.zero, Quaternion.identity);
            }
            //mini boss room 1 (grass)
            if (roomID == -1)
            {
                currentRoom = Instantiate(roomPrefabs[roomPrefabs.Length - 4], Vector3.zero, Quaternion.identity);
            }
            // mini boss room 2 (water)
            if (roomID == -2)
            {
                currentRoom = Instantiate(roomPrefabs[roomPrefabs.Length - 3], Vector3.zero, Quaternion.identity);
            }
            // mini boss room 2 (fire)
            if (roomID == -3)
            {
                currentRoom = Instantiate(roomPrefabs[roomPrefabs.Length - 2], Vector3.zero, Quaternion.identity);
            }
        }

        RoomDoorManager roomDoorManager = currentRoom.GetComponent<RoomDoorManager>();
        roomDoorManager.SpawnDoors(room.numDoors, room.doorSingle, room.doorLeft, room.doorRight);

        //get room grid to return so the player can walk on it
        RoomGrid roomGrid = currentRoom.GetComponent<RoomGrid>();

        return roomGrid;
    }
}
