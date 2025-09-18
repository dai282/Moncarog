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
        SpawnDoors();
        //roomGrid.PrintDoorTiles();
    }

    void SpawnDoors()
    {
        switch (setup)
        {

            case DoorSetup.Single:
                Instantiate(fillerLeft, slotLeft.position, Quaternion.identity, slotLeft);

                GameObject door = Instantiate(doorPrefabs[chosenDoorIndex], slotMiddle.position, Quaternion.identity, slotMiddle);

                Instantiate(fillerRight, slotRight.position, Quaternion.identity, slotRight);

                Vector3Int doorPos = roomGrid.collisionTilemap.WorldToCell(slotMiddle.position);

                Vector3Int bottomMiddleLeft  = doorPos + new Vector3Int(10, -8, 0);
                Vector3Int bottomMiddle      = doorPos + new Vector3Int(9, -8, 0);
                Vector3Int bottomMiddleRight = doorPos + new Vector3Int(8, -8, 0);

                DoorDetector doorDetector = door.GetComponent<DoorDetector>();

                roomGrid.RegisterDoor(bottomMiddleLeft, doorDetector);
                roomGrid.RegisterDoor(bottomMiddle, doorDetector);
                roomGrid.RegisterDoor(bottomMiddleRight, doorDetector);
                break;

            case DoorSetup.Double:
                GameObject leftDoor = Instantiate(doorPrefabs[chosenDoorIndexLeft], slotLeft.position, Quaternion.identity, slotLeft);
                Vector3Int leftDoorPos = roomGrid.collisionTilemap.WorldToCell(slotLeft.position);
                DoorDetector leftDetector = leftDoor.GetComponent<DoorDetector>();
                roomGrid.RegisterDoor(leftDoorPos + new Vector3Int(10, -8, 0), leftDetector);
                roomGrid.RegisterDoor(leftDoorPos + new Vector3Int(9, -8, 0), leftDetector);
                roomGrid.RegisterDoor(leftDoorPos + new Vector3Int(8, -8, 0), leftDetector);

                GameObject rightDoor = Instantiate(doorPrefabs[chosenDoorIndexRight], slotRight.position, Quaternion.identity, slotRight);
                Vector3Int rightDoorPos = roomGrid.collisionTilemap.WorldToCell(slotRight.position);
                DoorDetector rightDetector = rightDoor.GetComponent<DoorDetector>();
                roomGrid.RegisterDoor(rightDoorPos + new Vector3Int(10, -8, 0), rightDetector);
                roomGrid.RegisterDoor(rightDoorPos + new Vector3Int(9, -8, 0), rightDetector);
                roomGrid.RegisterDoor(rightDoorPos + new Vector3Int(8, -8, 0), rightDetector);

                Instantiate(fillerMiddle, slotMiddle.position, Quaternion.identity, slotMiddle);
                break;
        }
    }
}