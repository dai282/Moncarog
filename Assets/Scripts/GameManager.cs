using UnityEngine;

public class GameManager : MonoBehaviour
{
    //Instance of PlayerController, BoardManager, and Moncarog here
    //remember to assign instances of these classes in the inspector window (once we combine scenes)
    //public PlayerController playerController;
    //public BoardManager boardManager;
    //public Moncarog moncarog;
    //public combatHandler combatHandler;

    //static instance that stores reference to the GameManager. public get and private set
    public static GameManager Instance { get; private set; }


    //Awakeis called before Start when the GameObject is created
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
        //initialize UI elements
        //initialize the board (BoardManager.Init())
        //spawn the player (does he already have a moncarg with him?)
        //the moncarog encounter trigger code should be within the board manager script, not here?


    }
}
