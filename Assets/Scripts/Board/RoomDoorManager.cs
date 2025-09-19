using UnityEngine;

public class RoomDoorManager : MonoBehaviour
{
    [Header("Slot References")]
    public Transform slotLeft;
    public Transform slotMiddle;
    public Transform slotRight;

    [Header("Door Prefabs")]
    public GameObject[] doorPrefabs;

    [Header("Filler Prefabs")]
    public GameObject fillerLeft;
    public GameObject fillerMiddle;
    public GameObject fillerRight;

    [Header("Room Grid Reference")]
    public RoomGrid roomGrid;

    public enum DoorSetup
    {
        None,
        Single,
        Double
    }

    [Header("Setup")]
    public DoorSetup setup = DoorSetup.None;

    [Tooltip("Used for Single door setup (0–4)")]
    public int chosenDoorIndex = 0;

    [Tooltip("Used for Double door setup (0–4 each)")]
    public int chosenDoorIndexLeft = 0;
    public int chosenDoorIndexRight = 0;

    void Start()
    {
        //SpawnDoors(1, 0, 0, 0);
        //roomGrid.PrintDoorTiles();
    }

    public void SpawnDoors(int numDoors, int doorSingle, int doorLeft, int doorRight)
    {
        switch (numDoors)
        {

            case 1:
                Instantiate(fillerLeft, slotLeft.position, Quaternion.identity, slotLeft);

                GameObject door = Instantiate(doorPrefabs[doorSingle], slotMiddle.position, Quaternion.identity, slotMiddle);
                //For detecting doors
                DoorDetector doorDetector = door.GetComponent<DoorDetector>();
                doorDetector.doorIndex = 0; // Single door is index 0


                Instantiate(fillerRight, slotRight.position, Quaternion.identity, slotRight);

                Vector3Int doorPos = roomGrid.collisionTilemap.WorldToCell(slotMiddle.position);

                Vector3Int bottomMiddleLeft  = doorPos + new Vector3Int(10, -8, 0);
                Vector3Int bottomMiddle      = doorPos + new Vector3Int(9, -8, 0);
                Vector3Int bottomMiddleRight = doorPos + new Vector3Int(8, -8, 0);

                roomGrid.RegisterDoor(bottomMiddleLeft, doorDetector);
                roomGrid.RegisterDoor(bottomMiddle, doorDetector);
                roomGrid.RegisterDoor(bottomMiddleRight, doorDetector);
                break;

            case 2:
                GameObject leftDoor = Instantiate(doorPrefabs[doorLeft], slotLeft.position, Quaternion.identity, slotLeft);
                //For detecting doors
                DoorDetector leftDetector = leftDoor.GetComponent<DoorDetector>();
                leftDetector.doorIndex = 0; // Left door is index 0

                Vector3Int leftDoorPos = roomGrid.collisionTilemap.WorldToCell(slotLeft.position);
                roomGrid.RegisterDoor(leftDoorPos + new Vector3Int(10, -8, 0), leftDetector);
                roomGrid.RegisterDoor(leftDoorPos + new Vector3Int(9, -8, 0), leftDetector);
                roomGrid.RegisterDoor(leftDoorPos + new Vector3Int(8, -8, 0), leftDetector);

                GameObject rightDoor = Instantiate(doorPrefabs[doorRight], slotRight.position, Quaternion.identity, slotRight);
                //For detecting doors
                DoorDetector rightDetector = rightDoor.GetComponent<DoorDetector>();
                rightDetector.doorIndex = 1; // Right door is index 1

                Vector3Int rightDoorPos = roomGrid.collisionTilemap.WorldToCell(slotRight.position);
                roomGrid.RegisterDoor(rightDoorPos + new Vector3Int(10, -8, 0), rightDetector);
                roomGrid.RegisterDoor(rightDoorPos + new Vector3Int(9, -8, 0), rightDetector);
                roomGrid.RegisterDoor(rightDoorPos + new Vector3Int(8, -8, 0), rightDetector);

                Instantiate(fillerMiddle, slotMiddle.position, Quaternion.identity, slotMiddle);
                break;
        }
    }
}