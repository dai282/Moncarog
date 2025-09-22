using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    //Instance of PlayerController, BoardManager, and Moncarog here
    //remember to assign instances of these classes in the inspector window (once we combine scenes)
    public Player player;
    public GameObject startingMoncargPrefab;
    public MapManager mapManager;

    public BoardManager board;

    public MovementUI moveUI;

    private MapManager.RoomInfo currentRoom;
    private List<MapManager.RoomInfo> nextRooms;
    private RoomGrid currentRoomGrid;

    public MoncargDatabase moncargDatabase;

    //static instance that stores reference to the GameManager. public get and private set
    public static GameManager Instance { get; private set; }

    [SerializeField] private CombatHandler combatHandler;

    //private bool waitingForPlayerToEquip = false;


    //Awake is called before Start when the GameObject is created
    private void Awake()
    {
        // Singleton pattern to ensure only one instance of GameManager exists
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        //initialize the game components here

        //initialize player
        player.Init();

        //reset moncarg database
        moncargDatabase.resetMoncargDatabase();

        mapManager.Init();

        (currentRoom, nextRooms) = mapManager.GetCurrentRoomInfo();

        Debug.Log($"Current Room: {currentRoom.roomName} ({currentRoom.numDoors} doors),");

        foreach (var nextRoom in nextRooms)
        {
            Debug.Log($"Next Room(s): {nextRoom.roomName}");
        }

        //Generate the room based on current room info
        currentRoomGrid = board.GenerateRoom(currentRoom);
        //assign the room grid to the player so that they can move around
        player.GetComponent<PlayerMovement>().roomGrid = currentRoomGrid;

        //Start moncarg selection process to fight against enemy
        //we're passing in the enemy moncarg prefab here, after merge, this should be called inside BoardManager when player encounters a moncarg
        //combatHandler.BeginEncounter(enemyMoncargPrefab);

        //spawn the player (does he already have a moncarg with him?)
        //the moncarog encounter trigger code should be within the board manager script, not here?


    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            combatHandler.BeginEncounter();
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log($"Current Room: {currentRoom.roomName} ({currentRoom.numDoors} doors),");
            foreach (var nextRoom in nextRooms)
            {
                Debug.Log($"Next Room(s): {nextRoom.roomName}");
            }
        }

    }

    public void PlayerEnteredDoor(DoorDetector door)
    {
        Debug.Log($"PlayerEnteredDoor Called!");

        int doorIndex = door.doorIndex;

        // Get the next room based on which door was entered
        if (nextRooms != null && doorIndex < nextRooms.Count)
        {
            MapManager.RoomInfo nextRoom = nextRooms[doorIndex];
            //move to next node in maptraversal overlay
            mapManager.traversalOverlay.Move(doorIndex);
            //load new room
            LoadNextRoom(nextRoom);
        }
    }

    private void LoadNextRoom(MapManager.RoomInfo nextRoom)
    {
        // Destroy current room
        if (currentRoomGrid != null)
        {
            Destroy(currentRoomGrid.gameObject);
        }

        // Generate new room
        currentRoomGrid = board.GenerateRoom(nextRoom);

        // Update player's room grid reference
        player.GetComponent<PlayerMovement>().roomGrid = currentRoomGrid;

        // Position player at entrance of new room
        Vector3 spawnPosition = GetSpawnPositionForDoor(); // You'll need to implement this
        player.transform.position = spawnPosition;

        Debug.Log($"Updating current room and next rooms!");
        // Update current room info for next transition
        (currentRoom, nextRooms) = mapManager.GetCurrentRoomInfo();
    }

    private Vector3 GetSpawnPositionForDoor()
    {
        return new Vector3(4, -15, -2); 
    }
}
