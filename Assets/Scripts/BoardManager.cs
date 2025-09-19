using UnityEngine;
using System;

public class BoardManager : MonoBehaviour
{
    public GameObject[] roomPrefabs;

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
        GameObject currentRoom = Instantiate(roomPrefabs[Int32.Parse(room.roomName)], Vector3.zero, Quaternion.identity);
        RoomDoorManager roomDoorManager = currentRoom.GetComponent<RoomDoorManager>();
        roomDoorManager.SpawnDoors(room.numDoors, room.doorSingle, room.doorLeft, room.doorRight);

        //get room grid to return so the player can walk on it
        RoomGrid roomGrid = currentRoom.GetComponent<RoomGrid>();

        return roomGrid;
    }
}
