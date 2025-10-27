using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System;

public class GameManager : MonoBehaviour
{
    public Player player;
    public GameObject startingMoncargPrefab;
    public MapManager mapManager;
    public BoardManager board;
    public MovementUI moveUI;
    public static Action<float, float> OnTimeTick;
    private MapManager.RoomInfo currentRoom;
    private List<MapManager.RoomInfo> nextRooms;
    private RoomGrid currentRoomGrid;


    [SerializeField] private AudioClip loseSoundFX;
    [SerializeField] private AudioClip winSoundFX;
    [SerializeField] private AudioClip doorEnterSoundFX;

    private int roomLevel = 1;

    public MoncargDatabase moncargDatabase;

    public static GameManager Instance { get; private set; }

    [SerializeField] private CombatHandler combatHandler;

    [SerializeField] private LoseScreenUI loseScreenUI;
    [SerializeField] private GameObject victoryScreen;

    // Awake is called before Start when the GameObject is created
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
        //InitializeGame();
    }
    
    // In GameManager.cs

    public void Game()
    {
        Debug.Log("Saving game data...");
        
        // 1. Save the current run data FIRST (while session stats are still full)
        SaveManager.Instance?.SaveRun(); 
        
        // 2. NOW, merge those stats into lifetime and reset the session.
        StatsCollector.Instance?.SaveStats(); 
        
        Debug.Log("Game Saved.");
    }
    // Normal game initialization without clearing progress
    private void InitializeGame()
    {
        Debug.Log("Initializing game...");

        //initialize player
        player.Init();

        // Reset moncarg database
        moncargDatabase.resetMoncargDatabase();


        // Initalize the map and get current room
        mapManager.Init();

        (currentRoom, nextRooms) = mapManager.GetCurrentRoomInfo();

        // Generate the room based on current room info
        currentRoomGrid = board.GenerateRoom(currentRoom);
        // Assign the room grid to the player so that they can move around
        player.GetComponent<PlayerMovement>().roomGrid = currentRoomGrid;

        // Reset player position to starting position
        Vector3 startPosition = GetSpawnPositionForDoor();
        player.transform.position = startPosition;

        // Enable movement UI
        if (moveUI != null)
        {
            moveUI.EnableAllButtons();
        }

        Debug.Log("Game initialized successfully!");
    }

    // Public method for UI button to call
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

    // Move initialization code from Start() to this method
    public void StartNewGame()
    {
        Debug.Log("Starting new game...");

        // Initialize player
        player.Init();

        Time.timeScale = 1f;
        // Reset moncarg database
        moncargDatabase.resetMoncargDatabase();

        StatsCollector.Instance?.ResetSessionStats();

        // Reset inventory
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.StoredMoncargs.Clear();
            PlayerInventory.Instance.StoredItems.Clear();
            
            // Refresh UI after clearing
            PlayerInventory.Instance.RefreshAfterClear();
            
        }

        // Initalize the map and get current room
        mapManager.Init();

        (currentRoom, nextRooms) = mapManager.GetCurrentRoomInfo();

        // Generate the room based on current room info
        currentRoomGrid = board.GenerateRoom(currentRoom);
        // Assign the room grid to the player so that they can move around
        player.GetComponent<PlayerMovement>().roomGrid = currentRoomGrid;

        if (moveUI != null)
        {
            moveUI.EnableAllButtons();
        }

        Debug.Log("New game started successfully!");
    }

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
        // Check if anyone is subscribed before invoking
        if (OnTimeTick != null)
        {
            // Pass Time.deltaTime and Time.timeScale to the subscribers
            OnTimeTick.Invoke(Time.deltaTime, Time.timeScale);
        }
    }

    public void PlayerEnteredDoor(DoorDetector door)
    {
        Debug.Log($"PlayerEnteredDoor Called!");
        SoundFxManager.Instance.PlaySoundFXClip(doorEnterSoundFX, transform, 1f);

        int doorIndex = door.doorIndex;

        // Get the next room based on which door was entered
        if (nextRooms != null && doorIndex < nextRooms.Count)
        {
            MapManager.RoomInfo nextRoom = nextRooms[doorIndex];
            // Move to next node in maptraversal overlay
            mapManager.traversalOverlay.Move(doorIndex);
            // Load new room
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

    public void ContinueGame()
    {
        Debug.Log("Continue button pressed.");
        Time.timeScale = 1f;
        RunData data = SaveManager.Instance.LoadRun();
        if (data != null)
        {
            StartCoroutine(LoadGameFromData(data));
        }
        else
        {
            Debug.LogWarning("No save data found, starting a new game instead.");
            StartNewGame();
        }
    }

    // This coroutine orchestrates the entire loading process
    private IEnumerator LoadGameFromData(RunData data)
    {
        Debug.Log("Loading game from saved data...");

        // Disable player controls during load
        if (moveUI != null) moveUI.DisableAllButtons();

        StatsCollector.Instance?.SetCurrentSessionStats(data.sessionStats);

        // **WAIT FOR RESOURCEDB TO BE READY**
        yield return new WaitUntil(() => ResourceDB.Instance != null && ResourceDB.Instance.IsReady);
        Debug.Log("ResourceDB is ready");

        // 1. Restore inventory
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.LoadInventory(data.items, data.moncargs);
            Debug.Log($"Loaded inventory: {data.items.Count} items, {data.moncargs.Count} moncargs");
        }
        else
        {
            Debug.LogError("PlayerInventory.Instance is null during load!");
        }

        // 1.5 Reset Moncarg Database
        moncargDatabase.resetMoncargDatabase();

        // 2. Then rebuild the map 
        mapManager.LoadMapFromData(data.mapNodes);

        // Wait until the MapManager confirms it has finished rebuilding the visuals.
        yield return new WaitUntil(() => mapManager.isReady);

        // 3. Restore the traversal state before generating the room
        if (data.traversalPath != null && data.traversalPath.Count > 0)
        {
            Debug.Log($"Restoring traversal path with {data.traversalPath.Count} moves");
            mapManager.traversalOverlay.SetTraversalPath(data.traversalPath);
        }
        else
        {
            Debug.Log("No traversal path to restore");
        }

        // 4. Set current room ID and get room info
        mapManager.currentRoomId = data.currentRoomId;
        (currentRoom, nextRooms) = mapManager.GetCurrentRoomInfo();

        if (currentRoom == null)
        {
            Debug.LogError("Failed to get current room info after loading map");
            yield break;
        }

        // 5. Generate the specific room the player was in
        currentRoomGrid = board.GenerateRoom(currentRoom);
        player.GetComponent<PlayerMovement>().roomGrid = currentRoomGrid;

        // 6. Place the player at their saved position
        player.transform.position = data.playerPosition;


        // 7. Re-enable controls
        if (moveUI != null) moveUI.EnableAllButtons();

        Debug.Log($"Game load complete. Current room: {currentRoom.roomName}, Player position: {data.playerPosition}");
    }
}