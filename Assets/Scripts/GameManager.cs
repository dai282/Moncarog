using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Linq;

public class GameManager : MonoBehaviour
{
    //Instance of PlayerController, BoardManager, and Moncarog here
    //remember to assign instances of these classes in the inspector window (once we combine scenes)
    public Player player;
    //public BoardManager boardManager;
    public GameObject startingMoncargPrefab;
    public GameObject enemyMoncargPrefab;

    //[SerializeField] public MapManager mapManager;


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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            combatHandler.BeginEncounter(enemyMoncargPrefab);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        //initialize the game components here

        //initialize player
        player.Init();

        //initialize UI elements
        //combatHandler = new CombatHandler(combatHandlerUI, moncargSelectionUI, forceEquipPromptUI);

        //Start moncarg selection process to fight against enemy
        //we're passing in the enemy moncarg prefab here, after merge, this should be called inside BoardManager when player encounters a moncarg
        //mapManager.Start();

        //combatHandler.BeginEncounter(enemyMoncargPrefab);


        //initialize the board (BoardManager.Init()

        //spawn the player (does he already have a moncarg with him?)
        //the moncarog encounter trigger code should be within the board manager script, not here?


    }

    
}
