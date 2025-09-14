using UnityEngine;
using Elementals; //for elemental types
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

public class CombatHandler
{
    private CombatHandlerUI combatUI;
    private MoncargSelectionUI moncargSelectionUI;

    private Moncarg player;
    private Moncarg enemy;
    private Moncarg currentTurn;
    private Moncarg other;

    //pass in UI document from GameManager. Also passing in MoncargSelectionUI for the switch function
    public CombatHandler(CombatHandlerUI uiHandler, MoncargSelectionUI selectionUI)
    {
        combatUI = uiHandler;
        moncargSelectionUI = selectionUI;
        moncargSelectionUI.Hide();

        SubscribeToUIEvents();
    }

    private void SubscribeToUIEvents()
    {
        combatUI.OnAttackClicked += OnAttackClicked;
        combatUI.OnFleeClicked += OnFleeClicked;
        combatUI.OnCatchClicked += OnCatchClicked;
        combatUI.OnCancelCatchClicked += OnCancelCatchClicked;
        combatUI.OnInventoryClicked += OnInventoryClicked;
        combatUI.OnSwitchClicked += OnSwitchClicked;

        moncargSelectionUI.OnSelectionCancelled += OnSelectionCancelled;
        moncargSelectionUI.OnMoncargSelected += OnMoncargSelected;
    }

    //START EVENT DRIVEN BEGIN ENCOUNTER
    public void BeginEncounter(Moncarg ours, Moncarg enemyMoncarg)
    {
        player = ours;
        enemy = enemyMoncarg;

        combatUI.ShowCombatUI(true);
        combatUI.UpdateMoncargStats(player, enemy);

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
            OnEnemyDefeated();
            return;
        }

        combatUI.UpdateMoncargStats(player, enemy);

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
                // UI buttons are active, waiting for player click
            }
        }
        else
        {
            Debug.Log("Enemy's turn!");
            EnemyTurn();
        }
    }

    private void OnAttackClicked(int attackOption)
    {
        if (currentTurn != player) return;

        Debug.Log("Player chose Attack!");

        SkillDefinition attackChoice = player.skillset[attackOption - 1];

        if (attackChoice.name == "Rest")
        {
            Rest(player);
            return;
        }
        else
        {
            ExecuteAttack(player, enemy, attackChoice);
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
        // List of moves with weights
        List<(SkillDefinition skill, float weight)> movePool = new List<(SkillDefinition, float)>();

        // Add Ultimate if available
        if (enemy.mana >= enemy.skillset[3].manaCost)
            movePool.Add((enemy.skillset[3], 0.2f)); // 20%

        // Add Elemental if available
        if (enemy.mana >= enemy.skillset[2].manaCost)
            movePool.Add((enemy.skillset[2], 0.6f)); // 60%

        // Add Basic (always available, fallback)
        movePool.Add((enemy.skillset[1], 0.2f)); // 20%

        // If no mana at all for any move except basic chance to rest
        if (enemy.mana < enemy.skillset[2].manaCost && enemy.mana < enemy.skillset[3].manaCost)
        {
            float restRoll = Random.value;
            if (restRoll < 0.3f) // 30% chance to rest early
            {
                Debug.Log(enemy.moncargName + " is low on mana and decides to Rest...");
                Rest(enemy);
                return;
            }
        }

        // Roll weighted random choice
        float totalWeight = 0f;
        foreach (var move in movePool)
            totalWeight += move.weight;

        float roll = Random.value * totalWeight;
        float cumulative = 0f;
        SkillDefinition chosenSkill = movePool[0].skill; // fallback to first

        foreach (var move in movePool)
        {
            cumulative += move.weight;
            if (roll <= cumulative)
            {
                chosenSkill = move.skill;
                break;
            }
        }

        // Execute chosen move
        Debug.Log(enemy.moncargName + " chose " + chosenSkill.name);
        ExecuteAttack(enemy, player, chosenSkill);

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


    //END EVENT DRIVEN BEGIN ENCOUNTER

    #region Attack Execution
    public void ExecuteAttack(Moncarg attacker, Moncarg defender, SkillDefinition attackChoice)
    {
        if (TryDodge(defender))
        {
            Debug.Log($"{defender.moncargName} dodged the attack!");
            return;
        }

        // Deduct mana cost
        attacker.mana -= attackChoice.manaCost;

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



        // Check if defender is defeated
        if (defender.health <= 0)
        {
            defender.health = 0;
            defender.active = false;
            Debug.Log(defender.moncargName + " has been defeated!");
        }

        combatUI.UpdateMoncargStats(player, enemy);

    }

    public bool TryDodge(Moncarg defender)
    {
        float roll = Random.value; // random number between 0.0 and 1.0
        return roll < defender.dodgeChance;
    }

    public float checkElemental(Moncarg attacker, Moncarg defender, SkillDefinition attackChoice, float damage)
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

    #endregion

    private void Rest(Moncarg moncarg)
    {
        int manaRecovered = moncarg.maxMana / 4; // Recover 25% of max mana
        moncarg.mana += manaRecovered;
        //in case of overheal
        if (moncarg.mana > moncarg.maxMana)
        {
            moncarg.mana = moncarg.maxMana;
        }

        combatUI.UpdateMoncargStats(player, enemy);

        Debug.Log(moncarg.moncargName + " rested and recovered " + manaRecovered + " mana.");
        EndTurn();
    }

    private void OnInventoryClicked()
    {
        PlayerInventory.Instance.ShowInventory();
    }

    private void OnSwitchClicked()
    {
        var equippedMoncargs = PlayerInventory.Instance.StoredMoncargs
            .Where(m => m.IsEquipped)
            .Select(m => m.Details)
            .ToList();

        // Show selection UI for multiple equipped moncargs
        moncargSelectionUI.Show(equippedMoncargs);
    }



    private void OnEnemyDefeated()
    {
        combatUI.ShowCatchPanel();

        //Experience gaining logic
        /*
        Debug.Log("You won the battle!");
        // Reward player with experience points
        int expGained = enemy.exp;
        player.exp += expGained;
        Debug.Log(player.moncargName + " gained " + expGained + " experience points!");

        // Check for level up
        if (player.exp >= player.level * 100) // Example leveling formula
        {
            player.level++;
            player.exp = 0; // Reset experience or carry over excess
            player.maxHealth += 10; // Increase stats on level up
            player.attack += 5;
            player.defense += 5;
            player.speed += 2;
            player.maxMana += 5;
            player.health = player.maxHealth; // Heal to full on level up
            player.mana = player.maxMana; // Restore mana on level up

            Debug.Log(player.moncargName + " leveled up to level " + player.level + "!");
        }

        Cleanup();
        combatUI.rootVisualElement.style.display = DisplayStyle.None;
        // Return to map or previous state
        */
    }

    private void OnCatchClicked()
    {

        Debug.Log("Attempting to catch " + enemy.moncargName + "...");

        float catchRoll = Random.value; // random number between 0.0 and 1.0

        if (catchRoll < enemy.catchChance)
        {
            OnCatchSussess();
        }
        else
        {
            Debug.Log("Failed to catch " + enemy.moncargName + "!");
            Cleanup();
        }
    }

    private void OnCatchSussess()
    {
        Debug.Log("Successfully caught " + enemy.moncargName + "!");
        enemy.role = Moncarg.moncargRole.PlayerOwned;

        //Retrieve enemy moncarg game object and StoredMoncarg component
        GameObject enemyGO = enemy.gameObject;
        StoredMoncarg enemyStoredMoncarg = enemyGO.GetComponent<StoredMoncarg>();
        enemyStoredMoncarg.Details.moncargData.reset(); //reset health, mana and status

        //Add to inventory
        enemyStoredMoncarg.AddToInventory();

        Cleanup();
    }

    private void OnCancelCatchClicked()
    {
        Debug.Log("Catch cancelled.");
        Cleanup();
    }


    private void Cleanup()
    {
        combatUI.Cleanup();
        combatUI.ShowCombatUI(false);

        //Destroy moncarg game objects to prevent duplicates
        GameObject.Destroy(player.gameObject);
        GameObject.Destroy(enemy.gameObject);
    }

}

    

// Get the StoredMoncarg component from the Moncarg GameObject
//StoredMoncarg storedMoncarg = moncargGameObject.GetComponent<StoredMoncarg>();
//if (storedMoncarg != null)
//{
//    storedMoncarg.AddToInventory();
//}