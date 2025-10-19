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

    [SerializeField] private AudioClip loseSoundFX;
    [SerializeField] private AudioClip winSoundFX;

    private int roomLevel = 1;

    public MoncargDatabase moncargDatabase;

    //static instance that stores reference to the GameManager. public get and private set
    public static GameManager Instance { get; private set; }

    [SerializeField] private CombatHandler combatHandler;

    // ADDED: Game Over System
    [SerializeField] private LoseScreenUI loseScreenUI;
    [SerializeField] private GameObject victoryScreen;

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
        // MODIFIED: Initialize game normally without clearing inventory
        InitializeGame();
    }

    // ADDED: Normal game initialization without clearing progress
    private void InitializeGame()
    {
        Debug.Log("Initializing game...");

        //initialize player
        player.Init();

        //reset moncarg database
        moncargDatabase.resetMoncargDatabase();


        //initalize the map and get current room
        mapManager.Init();

        (currentRoom, nextRooms) = mapManager.GetCurrentRoomInfo();

        //Generate the room based on current room info
        currentRoomGrid = board.GenerateRoom(currentRoom);
        //assign the room grid to the player so that they can move around
        player.GetComponent<PlayerMovement>().roomGrid = currentRoomGrid;

        // ADDED: Reset player position to starting position
        Vector3 startPosition = GetSpawnPositionForDoor();
        player.transform.position = startPosition;

        // Enable movement UI
        if (moveUI != null)
        {
            moveUI.EnableAllButtons();
        }

        Debug.Log("Game initialized successfully!");
    }

    // ADDED: Public method for UI button to call
    public void NewGame()
    {
        Debug.Log("New Game button pressed via UI");
        
        // Hide the lose screen
        if (loseScreenUI != null)
        {
            loseScreenUI.HideGameOverScreen();
        }
        
        StartNewGame();
    }

    // ADDED: Move initialization code from Start() to this method
    public void StartNewGame()
    {
        Debug.Log("Starting new game...");

        //initialize the game components here

        //initialize player
        player.Init();

        //reset moncarg database
        moncargDatabase.resetMoncargDatabase();

        // ADDED: Reset inventory
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.StoredMoncargs.Clear();
            PlayerInventory.Instance.StoredItems.Clear();
            
            // Refresh UI after clearing
            PlayerInventory.Instance.RefreshAfterClear();
            
        }

        //initalize the map and get current room
        mapManager.Init();

        (currentRoom, nextRooms) = mapManager.GetCurrentRoomInfo();

        //Generate the room based on current room info
        currentRoomGrid = board.GenerateRoom(currentRoom);
        //assign the room grid to the player so that they can move around
        player.GetComponent<PlayerMovement>().roomGrid = currentRoomGrid;

        // ADDED: Enable movement UI
        if (moveUI != null)
        {
            moveUI.EnableAllButtons();
        }

        Debug.Log("New game started successfully!");
    }



    // ADDED: Handle game over trigger
    public void TriggerGameOver()
    {
        Debug.Log("GameManager.TriggerGameOver() called!");
        SoundFxManager.Instance.PlaySoundFXClip(loseSoundFX, transform, 1f);
        
        // Disable movement
        if (moveUI != null)
        {
            moveUI.DisableAllButtons();
            Debug.Log("Movement UI disabled");
        }
        
        // Show game over screen
        if (loseScreenUI != null)
        {
            Debug.Log("Calling loseScreenUI.ShowGameOverScreen()");
            loseScreenUI.ShowGameOverScreen();
        }
        else
        {
            Debug.LogError("LoseScreenUI not assigned in GameManager!");
        }
    }
    public void TriggerVictory()
    {
        Debug.Log("GameManager.TriggerVictory() called!");
        SoundFxManager.Instance.PlaySoundFXClip(winSoundFX, transform, 1f);

        // Disable movement
        if (moveUI != null)
        {
            moveUI.DisableAllButtons();
        }

        // Show victory screen
        if (victoryScreen != null)
        {
            victoryScreen.SetActive(true);
        }
        else
        {
            Debug.LogError("VictoryScreenUI not assigned in GameManager!");
        }
    }


    void Update()
    {
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
        // Update enemy levels for the new room
        MoncargDatabase.Instance.SetRoomLevel(roomLevel++);

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
        Vector3 spawnPosition = GetSpawnPositionForDoor();
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