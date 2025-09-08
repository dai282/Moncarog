using UnityEngine;
using Elementals; //for elemental types
using UnityEngine.UIElements;

public class CombatHandler
{
    private UIDocument combatUI;
    private UIDocument inventoryUI;

    private bool callbacksRegistered = false;

    private VisualElement optionsContainer;
    private VisualElement fightContainer;

    private Button fightButton;
    private Button fleeButton;
    private Button inventoryButton;
    private Button switchButton;
    private Button skill1Button;
    private Button skill2Button;
    private Button skill3Button;
    private Button skill4Button;
    private Button[] skillButtons = new Button[4];
    private Button backButton;
    private ProgressBar playerHealth;
    private ProgressBar playerMana;
    private ProgressBar enemyHealth;
    private ProgressBar enemyMana;

    private Moncarg player;
    private Moncarg enemy;
    private Moncarg currentTurn;
    private Moncarg other;

    public CombatHandler()
    {
        // Constructor logic if needed
    }

    //setter for UI
    public void SetUI(UIDocument CombatUiDoc, Player player)
    {
        combatUI = CombatUiDoc;

        var combatRoot = combatUI.rootVisualElement;

        //do not display until combat starts
        combatRoot.style.display = DisplayStyle.None;

        optionsContainer = combatRoot.Q<VisualElement>("OptionsContainer");
        fightContainer = combatRoot.Q<VisualElement>("FightContainer");

        fightButton = combatRoot.Q<Button>("FightButton");
        fleeButton = combatRoot.Q<Button>("FleeButton");
        inventoryButton = combatRoot.Q<Button>("InventoryButton");
        switchButton = combatRoot.Q<Button>("SwitchButton");
        skill1Button = combatRoot.Q<Button>("Move0");
        skill2Button = combatRoot.Q<Button>("Move1");
        skill3Button = combatRoot.Q<Button>("Move2");
        skill4Button = combatRoot.Q<Button>("Move3");
        backButton = combatRoot.Q<Button>("BackButton");

        skillButtons[0] = skill1Button;
        skillButtons[1] = skill2Button;
        skillButtons[2] = skill3Button;
        skillButtons[3] = skill4Button;

        playerHealth = combatRoot.Q<ProgressBar>("PlayerHealth");
        playerMana = combatRoot.Q<ProgressBar>("PlayerMana");
        enemyHealth = combatRoot.Q<ProgressBar>("EnemyHealth");
        enemyMana = combatRoot.Q<ProgressBar>("EnemyMana");

        //assigning colours
        var playerHealthProgress = playerHealth.Q(className: "unity-progress-bar__progress");
        playerHealthProgress.style.backgroundColor = new StyleColor(Color.green);

        var enemyHealthProgress = enemyHealth.Q(className: "unity-progress-bar__progress");
        enemyHealthProgress.style.backgroundColor = new StyleColor(Color.green);

        var playerManaProgress = playerMana.Q(className: "unity-progress-bar__progress");
        playerManaProgress.style.backgroundColor = new StyleColor(Color.blue);

        var enemyManaProgress = enemyMana.Q(className: "unity-progress-bar__progress");
        enemyManaProgress.style.backgroundColor = new StyleColor(Color.blue);

        // Register UI callbacks
        if (!callbacksRegistered)
        {
            //fightButton.clicked += OnAttackClicked;
            fightButton.clicked += ShowFightPanel;
            backButton.clicked += ShowOptionsPanel;

            fleeButton.clicked += OnFleeClicked;

            //wrapping the call in a lambda to pass the attack option
            skill1Button.clicked += () => OnAttackClicked(1);
            skill2Button.clicked += () => OnAttackClicked(2);
            skill3Button.clicked += () => OnAttackClicked(3);
            skill4Button.clicked += () => OnAttackClicked(4);

            inventoryButton.clicked += () => player.ViewInventory();

            callbacksRegistered = true;

        }
    }


    //START EVENT DRIVEN BEGIN ENCOUNTER
    public void BeginEncounter(Moncarg ours, Moncarg enemyMoncarg)
    {
        combatUI.rootVisualElement.style.display = DisplayStyle.Flex;
        player = ours;
        enemy = enemyMoncarg;

        //setup health display
        playerHealth.highValue = player.maxHealth;
        playerHealth.value = player.health;
        playerHealth.title = $"HP: {player.health} / {player.maxHealth}";

        enemyHealth.highValue = enemy.maxHealth;
        enemyHealth.value = enemy.health;
        enemyHealth.title = $"HP: {enemy.health} / {enemy.maxHealth}";

        //setup mana display
        playerMana.highValue = player.maxMana;
        playerMana.value = player.mana;
        playerMana.title = $"Mana: {player.mana} / {player.maxMana}";

        enemyMana.highValue = enemy.maxMana;
        enemyMana.value = enemy.mana;
        enemyMana.title = $"Mana: {enemy.mana} / {enemy.maxMana}";

        //Set skill button labels
        skill1Button.text = player.skillset[0].name;
        skill2Button.text = player.skillset[1].name;
        skill3Button.text = player.skillset[2].name;
        skill4Button.text = player.skillset[3].name;


        // Decide who goes first
        currentTurn = (player.speed >= enemy.speed) ? player : enemy;
        other = (currentTurn == player) ? enemy : player;

        // Start first turn
        NextTurn();
    }

    private void NextTurn()
    {
        if (!player.active)
        {
            Debug.Log("You need to switch Moncargs!");
            return;
        }
        if (!enemy.active)
        {
            Debug.Log("You won the battle!");
            return;
        }

        if (currentTurn == player)
        {

            //automatic resting
            if (player.mana <=0 )
            {
                Debug.Log(player.moncargName + " ran out of mana! Automatic resting...");
                Rest(player);
            }
            else
            {
                Debug.Log("Your turn! Choose an action.");
                for (int i = 0; i < 4; i++ )
                {
                    if(player.mana < player.skillset[i].manaCost)
                    {
                        skillButtons[i].SetEnabled(false);
                    }
                    else
                    {
                        skillButtons[i].SetEnabled(true);
                    }
                }
                // UI buttons are active, waiting for player click
            }
        }
        else
        {
            Debug.Log("Enemy's turn!");
            EnemyTurn();
        }
    }

    //Hides options display and just shows fight moves
    private void ShowFightPanel()
    {
        optionsContainer.style.display = DisplayStyle.None;
        fightContainer.style.display = DisplayStyle.Flex;

    }

    //Hides fight moves and just shows combat encounter options
    private void ShowOptionsPanel()
    {
        fightContainer.style.display = DisplayStyle.None;
        optionsContainer.style.display = DisplayStyle.Flex;
    }

    private void OnAttackClicked(int attackOption)
    {
        if (currentTurn != player) return;

        Debug.Log("Player chose Attack!");

        Skill attackChoice = player.skillset[attackOption - 1];

        if (attackChoice.name == "Rest")
        {
            Rest(player);
            return;
        }
        else
        {
            if (TryDodge(enemy))
            {
                Debug.Log($"{enemy.moncargName} dodged the attack!");
            }
            else
            {
                ExecuteAttack(player, enemy, attackChoice);
            }
        }
        EndTurn();
    }

    private void OnFleeClicked()
    {
        if (currentTurn != player) return;

        Debug.Log("You fled the battle!");
        // Cleanup if needed
        Cleanup();

        //go back to the map
    }

    private void EnemyTurn()
    {
        if (enemy.mana <= 0)
        {
            Debug.Log(enemy.moncargName + " ran out of mana! Automatic resting...");
            Rest(enemy);
            return;
        }

        // For now, enemy always uses basic attack
        ExecuteAttack(enemy, player, enemy.skillset[1]);
        EndTurn();
    }

    private void EndTurn()
    {
        // Swap turn
        Moncarg temp = currentTurn;
        currentTurn = other;
        other = temp;

        NextTurn();
    }

    private void Cleanup()
    {
        if (callbacksRegistered)
        {
            // Unregister callbacks so buttons don’t stack
            skill1Button.clicked -= () => OnAttackClicked(1);
            skill2Button.clicked -= () => OnAttackClicked(2);
            skill3Button.clicked -= () => OnAttackClicked(3);
            fleeButton.clicked -= OnFleeClicked;
            fightButton.clicked += ShowFightPanel;
            backButton.clicked += ShowOptionsPanel;

            callbacksRegistered = false;
        }
        
    }


    //END EVENT DRIVEN BEGIN ENCOUNTER

    public void ExecuteAttack(Moncarg attacker, Moncarg defender, Skill attackChoice)
    {
        if (attacker.mana < attackChoice.manaCost)
        {
            Debug.Log(attacker.moncargName + " does not have enough mana to use " + attackChoice.name + "!");
            return;
        }

        // Deduct mana cost
        attacker.mana -= attackChoice.manaCost;
        //update mana display
        playerMana.value = player.mana;
        enemyMana.value = enemy.mana;
        playerMana.title = $"Mana: {player.mana} / {player.maxMana}";
        enemyMana.title = $"Mana: {enemy.mana} / {enemy.maxMana}";

        // Calculate base damage
        float damage = attackChoice.damage + attacker.attack - defender.defense;

        damage = checkElemental(attacker, defender, attackChoice, damage);

        // Ensure damage is not negative
        if (damage < 0)
        {
            damage = 0;
        }

        // Apply damage to defender
        defender.health -= damage;

        Debug.Log(attacker.moncargName + " used " + attackChoice.name + " on " + defender.moncargName + " for " + damage + " damage!");

        //update health display
        playerHealth.value = player.health;
        enemyHealth.value = enemy.health;
        playerHealth.title = $"HP: {player.health} / {player.maxHealth}";
        enemyHealth.title = $"HP: {enemy.health} / {enemy.maxHealth}";

        // Check if defender is defeated
        if (defender.health <= 0)
        {
            defender.health = 0;
            defender.active = false;
            Debug.Log(defender.moncargName + " has been defeated!");
        }
    }

    public bool TryDodge(Moncarg defender)
    {
        float roll = Random.value; // random number between 0.0 and 1.0
        return roll < defender.dodgeChance;
    }

    public float checkElemental(Moncarg attacker, Moncarg defender, Skill attackChoice, float damage)
    {
        //Check for elemental effectiveness | REFACTOR THIS INTO ANOTHER METHOD LATER
        if (attackChoice.type == ElementalType.Fire && defender.type == ElementalType.Plant)
        {
            damage *= 1.2f; // Fire is strong against Plant
            Debug.Log("Fire vs Plant, " + attacker.moncargName + " deals +20% damage!");
        }
        else if (attackChoice.type == ElementalType.Water && defender.type == ElementalType.Fire)
        {
            damage *= 1.2f; // Water is strong against Fire
            Debug.Log("Water vs Fire, " + attacker.moncargName + " deals +20% damage!");
        }
        else if (attackChoice.type == ElementalType.Plant && defender.type == ElementalType.Water)
        {
            damage *= 1.2f; // Plant is strong against Water
            Debug.Log("Plant vs Water, " + attacker.moncargName + " deals +20% damage!");
        }
        else if (attackChoice.type == ElementalType.Fire && defender.type == ElementalType.Water)
        {
            damage *= 0.8f; // Fire is weak against Water
            Debug.Log("Fire vs Water, " + attacker.moncargName + " damage is decreased by 20%!");
        }
        else if (attackChoice.type == ElementalType.Water && defender.type == ElementalType.Plant)
        {
            damage *= 0.8f; // Water is weak against Plant
            Debug.Log("Water vs Plant, " + attacker.moncargName + " damage is decreased by 20%!");
        }
        else if (attackChoice.type == ElementalType.Plant && defender.type == ElementalType.Fire)
        {
            damage *= 0.8f; // Plant is weak against Fire
            Debug.Log("Plant vs Fire, " + attacker.moncargName + " damage is decreased by 20%!");
        }

        return damage;
    }

    private void Rest(Moncarg moncarg)
    {
        int manaRecovered = moncarg.maxMana / 4; // Recover 25% of max mana
        moncarg.mana += manaRecovered;
        //in case of overheal
        if (moncarg.mana > moncarg.maxMana)
        {
            moncarg.mana = moncarg.maxMana;
        }

        playerMana.value = player.mana;
        enemyMana.value = enemy.mana;
        playerMana.title = $"Mana: {player.mana} / {player.maxMana}";
        enemyMana.title = $"Mana: {enemy.mana} / {enemy.maxMana}";

        Debug.Log(moncarg.moncargName + " rested and recovered " + manaRecovered + " mana.");
        EndTurn();
    }
}