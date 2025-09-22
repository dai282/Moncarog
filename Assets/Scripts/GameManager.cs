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

    // ADDED: Game Over System
    [SerializeField] private LoseScreenUI loseScreenUI;

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

        // Debug existing inventory
        if (PlayerInventory.Instance != null)
        {
            int moncargCount = PlayerInventory.Instance.GetCurrentMoncargCount();
            int equippedCount = PlayerInventory.Instance.StoredMoncargs.Count(m => m?.Details != null && m.IsEquipped);
            
            Debug.Log($"Existing Moncargs in inventory: {moncargCount}");
            Debug.Log($"Equipped Moncargs: {equippedCount}");
            
            // List all Moncargs
            for (int i = 0; i < PlayerInventory.Instance.StoredMoncargs.Count; i++)
            {
                var moncarg = PlayerInventory.Instance.StoredMoncargs[i];
                if (moncarg?.Details != null)
                {
                    Debug.Log($"Moncarg {i}: {moncarg.Details.FriendlyName}, Equipped: {moncarg.IsEquipped}");
                }
            }
            
            // Only add starting Moncarg if player has no Moncargs
            if (moncargCount == 0)
            {
                Debug.Log("No Moncargs found, adding starting Moncarg");
                AddStartingMoncarg();
            }
            else
            {
                Debug.Log("Existing Moncargs found, skipping starting Moncarg");
            }
        }

        //initalize the map and get current room
        mapManager.Init();

        (currentRoom, nextRooms) = mapManager.GetCurrentRoomInfo();

        //Generate the room based on current room info
        currentRoomGrid = board.GenerateRoom(currentRoom);
        //assign the room grid to the player so that they can move around
        player.GetComponent<PlayerMovement>().roomGrid = currentRoomGrid;

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
            
            // Add starting Moncarg
            AddStartingMoncarg();
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

    // ADDED: Add starting Moncarg for new game
    private void AddStartingMoncarg()
    {
        if (startingMoncargPrefab != null)
        {
            GameObject startingMoncargObj = Instantiate(startingMoncargPrefab);
            
            // Convert to player-owned
            Moncarg startingMoncarg = startingMoncargObj.GetComponent<Moncarg>();
            if (startingMoncarg != null)
            {
                startingMoncarg.role = Moncarg.moncargRole.PlayerOwned;
                
                // Reset stats to full
                startingMoncarg.InitStats();
                startingMoncarg.health = startingMoncarg.maxHealth;
                startingMoncarg.mana = startingMoncarg.maxMana;
                
                // Add to inventory
                StoredMoncarg storedMoncarg = startingMoncargObj.GetComponent<StoredMoncarg>();
                if (storedMoncarg != null)
                {
                    PlayerInventory.Instance.AddMoncargToInventory(storedMoncarg.Details, true); // Auto-equip
                }
            }
            
            // Destroy the temporary object
            DestroyImmediate(startingMoncargObj);
            
            Debug.Log("Added starting Moncarg to player inventory");
        }
        else
        {
            Debug.LogWarning("No starting Moncarg prefab assigned!");
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