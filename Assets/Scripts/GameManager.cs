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

    //static instance that stores reference to the GameManager. public get and private set
    public static GameManager Instance { get; private set; }

    //create UI document and combat handler variables  here
    [SerializeField] private CombatHandlerUI combatHandlerUI;
    [SerializeField] private MoncargSelectionUI moncargSelectionUI;
    [SerializeField] private ForceEquipPromptUI forceEquipPrompt;
    private CombatHandler combatHandler;

    private bool waitingForPlayerToEquip = false;


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

        //initialize player
        player.Init();

        //initialize UI elements | Combat UI Document should be part of the CombatHandlerUI (but we have not made it yet), make it the same as MoncargSelectionUI and ForceEquipPromptUI
        combatHandler = new CombatHandler(combatHandlerUI);

        //initialize starting Moncarg and test encounter

        // Subscribe moncargSelectionUI to events
        if (moncargSelectionUI != null)
        {
            moncargSelectionUI.OnMoncargSelected += OnMoncargSelected;
            moncargSelectionUI.OnSelectionCancelled += OnSelectionCancelled;
        }

        //auto equip moncargs if none are equipped
        //AutoEquipMoncargs();

        //Start moncarg selection process
        StartCoroutine(StartGameWithMoncargSelection());


        //initialize the board (BoardManager.Init()

        //spawn the player (does he already have a moncarg with him?)
        //the moncarog encounter trigger code should be within the board manager script, not here?


    }

    #region Moncarg Selection

    private void AutoEquipMoncargs()
    {
        if (PlayerInventory.Instance != null)
        {
            // Equip first 3 moncargs by default (or all if less than 3)
            int equippedCount = 0;
            foreach (var storedMoncarg in PlayerInventory.Instance.StoredMoncargs)
            {
                if (equippedCount < 3)
                {
                    storedMoncarg.IsEquipped = true;
                    equippedCount++;
                    Debug.Log($"Auto-equipped: {storedMoncarg.Details.FriendlyName}");
                }
                else
                {
                    break;
                }
            }

            PlayerInventory.Instance.UpdateMoncargEquippedCount();
        }
    }

    //hello world
    private IEnumerator StartGameWithMoncargSelection()
    {
        // Wait for inventory to be initialized
        yield return new WaitUntil(() => PlayerInventory.Instance != null && PlayerInventory.Instance.m_IsInventoryReady);

        // Get equipped moncargs
        var equippedMoncargs = PlayerInventory.Instance.StoredMoncargs
            .Where(m => m.IsEquipped)
            .Select(m => m.Details)
            .ToList();

        if (equippedMoncargs.Count == 0)
        {
            Debug.LogWarning("No equipped Moncargs found! Opening Inventory to equip one");
            ForcePlayerToEquipMoncarg();
        }
        else if (equippedMoncargs.Count == 1)
        {
            // If only one equipped, use it automatically
            OnMoncargSelected(equippedMoncargs[0]);
        }
        else
        {
            // Show selection UI for multiple equipped moncargs
            moncargSelectionUI.Show(equippedMoncargs);
        }
    }

    private void ForcePlayerToEquipMoncarg()
    {
        waitingForPlayerToEquip = true;

        forceEquipPrompt.ShowPrompt("Please equip at least one Moncarg from your inventory to continue!");

        // Show inventory and prompt player to equip a moncarg
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.ShowInventory();
            PlayerInventory.Instance.SwitchToMoncargMode();

            // You might want to show a message to the player here
            Debug.Log("Please equip at least one Moncarg to continue!");

            // Start checking for equipped moncargs
            StartCoroutine(WaitForPlayerToEquip());
        }
    }

    private IEnumerator WaitForPlayerToEquip()
    {
        // Wait until player equips at least one moncarg
        yield return new WaitUntil(() =>
            PlayerInventory.Instance.StoredMoncargs.Any(m => m.IsEquipped) ||
            !waitingForPlayerToEquip);

        if (waitingForPlayerToEquip)
        {
            // Player equipped a moncarg, continue with selection
            var equippedMoncargs = PlayerInventory.Instance.StoredMoncargs
                .Where(m => m.IsEquipped)
                .Select(m => m.Details)
                .ToList();

            if (equippedMoncargs.Count == 1)
            {
                OnMoncargSelected(equippedMoncargs[0]);
            }
            else
            {
                moncargSelectionUI.Show(equippedMoncargs);
            }

            waitingForPlayerToEquip = false;
        }
    }

    private void OnMoncargSelected(MoncargInventoryAdapter selectedMoncarg)
    {
        Debug.Log($"Selected Moncarg: {selectedMoncarg.FriendlyName}");

        // Hide inventory if it's open
        if (PlayerInventory.Instance != null && PlayerInventory.Instance.IsInventoryVisible())
        {
            PlayerInventory.Instance.HideInventory();
        }

        if (forceEquipPrompt != null)
        {
            forceEquipPrompt.HidePrompt();
        }

        // Spawn the selected moncarg for battle
        GameObject playerMoncargObj = selectedMoncarg.CreateMoncargGameObject();
        Moncarg playerMoncarg = playerMoncargObj.GetComponent<Moncarg>();
        playerMoncarg.InitStats();

        // Spawn enemy Moncarg
        GameObject enemyObj = Instantiate(enemyMoncargPrefab);
        Moncarg enemy = enemyObj.GetComponent<Moncarg>();
        enemy.InitStats();

        // Begin encounter
        combatHandler.BeginEncounter(playerMoncarg, enemy);
    }

    private void OnSelectionCancelled()
    {
        // If selection was cancelled but we have equipped moncargs, use the first one
        var equippedMoncargs = PlayerInventory.Instance.StoredMoncargs
            .Where(m => m.IsEquipped)
            .Select(m => m.Details)
            .ToList();

        if (equippedMoncargs.Count > 0)
        {
            Debug.Log("Selection cancelled, but equipped Moncargs found. Using the first one.");
            OnMoncargSelected(equippedMoncargs[0]);
        }
        else
        {
            // If no equipped moncargs after cancellation, force equip
            ForcePlayerToEquipMoncarg();
        }
    }

    private void OnDestroy()
    {
        if (moncargSelectionUI != null)
        {
            moncargSelectionUI.OnMoncargSelected -= OnMoncargSelected;
            moncargSelectionUI.OnSelectionCancelled -= OnSelectionCancelled;
        }
    }

    // Public method to cancel the equip waiting (if player closes inventory without equipping)
    public void CancelEquipWaiting()
    {
        waitingForPlayerToEquip = false;
    }

    #endregion
}
