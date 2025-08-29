using UnityEngine;

public class CombatHandler
{
    public CombatHandler()
    {
        // Constructor logic if needed
    }

    public void BeginEncounter(Moncarg ours, Moncarg enemy)
    {
        int option = 0;

        // Decide who goes first
        Moncarg currentTurn = (ours.speed >= enemy.speed) ? ours : enemy;
        Moncarg other = (currentTurn == ours) ? enemy : ours;

        while (ours.active && enemy.active && option != 4)
        {
            if (currentTurn == ours)
            {
                // --- Player Turn ---

                //poll for option, default attack for now
                //option = GameManager.Instance.playerController.PollForEncounterInput();
                option = 1;

                switch (option)
                {
                    case 1: // Attack
                        //default basic attack for now
                        //int attackOption = GameManager.Instance.playerController.PollForAttackInput();
                        int attackOption = 1;
                        ExecuteAttack(ours, enemy, ours.skillset[attackOption - 1]);
                        break;

                    case 2: // Switch
                            // TODO: switch logic
                        break;

                    case 3: // Item
                            // TODO: item logic
                        break;
                }
            }
            else
            {
                // --- Enemy Turn ---
                // TODO: expand to randomize enemy actions
                ExecuteAttack(enemy, ours, enemy.skillset[0]);
            }

            // Swap turn
            Moncarg temp = currentTurn;
            currentTurn = other;
            other = temp;
        }

        //Check whether player fleed or one of the moncargs was defeated
        if (option == 4)
        {
            Debug.Log("You fled the battle!");
        }

        if (!ours.active)
        {
            //poll for switch
            Debug.Log("You need to switch Moncargs!");
            //Moncarg substitute = GameManager.Instance.playerController.PollForSwitchInput();
            //BeginEncounter(substitute, enemy);
        }
        if (!enemy.active)
        {
            Debug.Log("You won the battle!");
        }


    }

    public void ExecuteAttack(Moncarg attacker, Moncarg defender, Skill attackChoice)
    {
        if (attacker.mana < attackChoice.manaCost)
        {
            Debug.Log(attacker.moncargName + " does not have enough mana to use " + attackChoice.name + "!");
            return;
        }

        // Deduct mana cost
        attacker.mana -= attackChoice.manaCost;

        // Calculate damage
        float damage = attackChoice.damage + attacker.attack - defender.defense;

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
    }
}

// Experience and Leveling System (draft)
/*
if (defender.role == Moncarg.moncargRole.Wild && attacker.role == Moncarg.moncargRole.PlayerOwned)
{
    // Award experience points
    attacker.exp += defender.exp;
    Debug.Log(attacker.moncargName + " gained " + defender.exp + " EXP!");

    // Check for level up
    if (attacker.exp >= attacker.level * 100) // Example leveling formula
    {
        attacker.level++;
        attacker.exp = 0; // Reset EXP after leveling up
        attacker.maxHealth += 10; // Example stat increase
        attacker.attack += 5;
        attacker.defense += 5;
        attacker.mana += 5;
        attacker.health = attacker.maxHealth; // Heal to full on level up
        Debug.Log(attacker.moncargName + " leveled up to level " + attacker.level + "!");
    }
}
*/