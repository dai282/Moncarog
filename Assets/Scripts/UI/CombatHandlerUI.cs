using UnityEngine;
using UnityEngine.UIElements;
using Elementals;

public class CombatHandlerUI : MonoBehaviour
{
    [SerializeField] private UIDocument combatUI;

    private VisualElement root;
    private VisualElement optionsContainer;
    private VisualElement fightContainer;
    private VisualElement catchContainer;

    private Button fightButton;
    private Button fleeButton;
    private Button inventoryButton;
    private Button switchButton;
    private Button catchButton;
    private Button cancelCatchButton;
    private Button backButton;
    private Button[] skillButtons = new Button[4];

    private ProgressBar playerHealth;
    private ProgressBar playerMana;
    private ProgressBar enemyHealth;
    private ProgressBar enemyMana;

    // Events for UI interactions
    public System.Action<int> OnAttackClicked;
    public System.Action OnFleeClicked;
    public System.Action OnCatchClicked;
    public System.Action OnCancelCatchClicked;
    public System.Action OnInventoryClicked;
    public System.Action OnSwitchClicked;
    public System.Action OnBackClicked;

    private void Awake()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        if (combatUI == null) return;

        root = combatUI.rootVisualElement;
        root.style.display = DisplayStyle.None;

        optionsContainer = root.Q<VisualElement>("OptionsContainer");
        fightContainer = root.Q<VisualElement>("FightContainer");
        catchContainer = root.Q<VisualElement>("CatchContainer");

        // Get buttons
        fightButton = root.Q<Button>("FightButton");
        fleeButton = root.Q<Button>("FleeButton");
        inventoryButton = root.Q<Button>("InventoryButton");
        switchButton = root.Q<Button>("SwitchButton");
        catchButton = root.Q<Button>("CatchButton");
        cancelCatchButton = root.Q<Button>("CancelCatchButton");
        backButton = root.Q<Button>("BackButton");

        skillButtons[0] = root.Q<Button>("Move0");
        skillButtons[1] = root.Q<Button>("Move1");
        skillButtons[2] = root.Q<Button>("Move2");
        skillButtons[3] = root.Q<Button>("Move3");

        // Get progress bars
        playerHealth = root.Q<ProgressBar>("PlayerHealth");
        playerMana = root.Q<ProgressBar>("PlayerMana");
        enemyHealth = root.Q<ProgressBar>("EnemyHealth");
        enemyMana = root.Q<ProgressBar>("EnemyMana");

        // Set up colors
        SetupProgressBarColors();

        // Register callbacks
        RegisterCallbacks();
    }

    private void SetupProgressBarColors()
    {
        var playerHealthProgress = playerHealth.Q(className: "unity-progress-bar__progress");
        playerHealthProgress.style.backgroundColor = new StyleColor(Color.green);

        var enemyHealthProgress = enemyHealth.Q(className: "unity-progress-bar__progress");
        enemyHealthProgress.style.backgroundColor = new StyleColor(Color.green);

        var playerManaProgress = playerMana.Q(className: "unity-progress-bar__progress");
        playerManaProgress.style.backgroundColor = new StyleColor(Color.blue);

        var enemyManaProgress = enemyMana.Q(className: "unity-progress-bar__progress");
        enemyManaProgress.style.backgroundColor = new StyleColor(Color.blue);
    }

    private void RegisterCallbacks()
    {
        fightButton.clicked += () => ShowFightPanel();
        backButton.clicked += () => ShowOptionsPanel();
        fleeButton.clicked += () => OnFleeClicked?.Invoke();
        inventoryButton.clicked += () => OnInventoryClicked?.Invoke();
        switchButton.clicked += () => OnSwitchClicked?.Invoke();
        catchButton.clicked += () => OnCatchClicked?.Invoke();
        cancelCatchButton.clicked += () => OnCancelCatchClicked?.Invoke();

        for (int i = 0; i < 4; i++)
        {
            int index = i; // Capture the index
            skillButtons[i].clicked += () => OnAttackClicked?.Invoke(index + 1);
        }
    }

    // Public methods to update UI
    public void ShowCombatUI(bool show)
    {
        root.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public void UpdateMoncargStats(Moncarg player, Moncarg enemy)
    {
        // Update health bars
        playerHealth.highValue = player.maxHealth;
        playerHealth.value = player.health;
        playerHealth.title = $"HP: {player.health} / {player.maxHealth}";

        enemyHealth.highValue = enemy.maxHealth;
        enemyHealth.value = enemy.health;
        enemyHealth.title = $"HP: {enemy.health} / {enemy.maxHealth}";

        // Update mana bars
        playerMana.highValue = player.maxMana;
        playerMana.value = player.mana;
        playerMana.title = $"Mana: {player.mana} / {player.maxMana}";

        enemyMana.highValue = enemy.maxMana;
        enemyMana.value = enemy.mana;
        enemyMana.title = $"Mana: {enemy.mana} / {enemy.maxMana}";

        // Update skill buttons
        for (int i = 0; i < 4; i++)
        {
            if (i < player.skillset.Length)
            {
                skillButtons[i].text = player.skillset[i].name;
                skillButtons[i].SetEnabled(player.mana >= player.skillset[i].manaCost);
            }
        }
    }

    public void ShowFightPanel()
    {
        optionsContainer.style.display = DisplayStyle.None;
        fightContainer.style.display = DisplayStyle.Flex;
    }

    public void ShowOptionsPanel()
    {
        fightContainer.style.display = DisplayStyle.None;
        optionsContainer.style.display = DisplayStyle.Flex;
    }

    public void ShowCatchPanel()
    {
        optionsContainer.style.display = DisplayStyle.None;
        fightContainer.style.display = DisplayStyle.None;
        catchContainer.style.display = DisplayStyle.Flex;
    }

    public void Cleanup()
    {
        // Unregister all callbacks to prevent memory leaks
        fightButton.clicked -= () => ShowFightPanel();
        backButton.clicked -= () => ShowOptionsPanel();
        fleeButton.clicked -= () => OnFleeClicked?.Invoke();
        inventoryButton.clicked -= () => OnInventoryClicked?.Invoke();
        switchButton.clicked -= () => OnSwitchClicked?.Invoke();
        catchButton.clicked -= () => OnCatchClicked?.Invoke();
        cancelCatchButton.clicked -= () => OnCancelCatchClicked?.Invoke();

        for (int i = 0; i < 4; i++)
        {
            int index = i;
            skillButtons[i].clicked -= () => OnAttackClicked?.Invoke(index + 1);
        }
    }
}