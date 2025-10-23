using UnityEngine;
using Elementals; //for elemental types
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class CombatHandler: MonoBehaviour
{
    [SerializeField] private CombatHandlerUI combatUI;
    [SerializeField] private MoncargSelectionUI selectionUI;
    [SerializeField] private ForceEquipPromptUI forceEquipUI;

    [Header("References")]
    public GameObject victoryScreen;

    [Header("Slash Effect")]
    [SerializeField] private Animator slashAnimator;
    [SerializeField] private Transform slashTransform;
    [SerializeField] private float slashDuration = 0.3f;

    [Header("Damage Indicator")]
    [SerializeField] private Canvas combatCanvas; // assign your combat UI canvas here
    [SerializeField] private GameObject damageIndicatorPrefab; // assign your prefab

    // ADDED: Item drop system fields
    [Header("Item Drop System")]
    [SerializeField] private ItemDefinition[] commonDrops;
    [SerializeField] private ItemDefinition[] rareDrops;
    [SerializeField] private ItemDefinition[] legendaryDrops;
    [SerializeField] [Range(0f, 1f)] private float dropChance = 0.7f; // 70% chance to drop something
    [SerializeField] [Range(0f, 1f)] private float rareDropChance = 0.15f; // 15% chance for rare
    [SerializeField] [Range(0f, 1f)] private float legendaryDropChance = 0.05f; // 5% chance for legendary

    private Moncarg player;
    private Moncarg enemy;
    private Moncarg currentTurn;
    private Moncarg other;
    private MoncargDatabase moncargDatabase;
    private GameObject enemyObj;
    private bool waitingForPlayerToEquip = false;

    private void Awake()
    {
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

        if (selectionUI != null)
        {
            selectionUI.OnMoncargSelected += OnMoncargSelected;
            selectionUI.OnSelectionCancelled += OnSelectionCancelled;
        }
    }
    
    //START EVENT DRIVEN BEGIN ENCOUNTER
    public void BeginEncounter(int roomID)
    {
        //disable move buttons
        GameManager.Instance.moveUI.DisableAllButtons();

        //Create enemy Moncarg instance for battle
        moncargDatabase = GameManager.Instance.moncargDatabase;
        int databaseLen = moncargDatabase.availableEnemyMoncargs.Count;
        //boss and miniboss rooms
        if (roomID < 0)
        {
            switch (roomID)
            {
                case -1:
                    //mini boss 1 (grass)
                    enemyObj = Instantiate(moncargDatabase.availableEnemyMoncargs[databaseLen - 4]);
                    break;
                case -2:
                    //mini boss 2 (water)
                    enemyObj = Instantiate(moncargDatabase.availableEnemyMoncargs[databaseLen - 3]);
                    break;
                case -3:
                    //mini boss 3 (fire)
                    enemyObj = Instantiate(moncargDatabase.availableEnemyMoncargs[databaseLen - 2]);
                    break;
                case -99:
                    //final boss
                    enemyObj = Instantiate(moncargDatabase.availableEnemyMoncargs[databaseLen - 1]);
                    break;
                default:
                    Debug.LogError("Invalid miniboss/boss room ID: " + roomID);
                    int randIndex = Random.Range(0, databaseLen - 4);
                    enemyObj = Instantiate(moncargDatabase.availableEnemyMoncargs[randIndex]);
                    break;
            }
        }
        else
        {
            int randIndex = Random.Range(0, databaseLen - 4);
            enemyObj = Instantiate(moncargDatabase.availableEnemyMoncargs[randIndex]);
        }

       enemyObj.transform.localScale = new Vector3(15f, 15f, 0f);
       enemyObj.transform.position = new Vector3(15f, 0f, 0f);
       enemy = enemyObj.GetComponent<Moncarg>();   
       enemy.InitStats();
       //hide until combat starts
       enemyObj.SetActive(false);

        //auto equip moncargs if none are equipped
        //AutoEquipMoncargs();

        //start moncarg selection
        StartCoroutine(StartMoncargSelection());
    }

    public void BeginBattle()
    {
        //hide the room grid
        FindFirstObjectByType<BoardManager>().disableRoom();

        //hide player sprite
        GameObject playerObj = GameObject.FindWithTag("Player");
        SpriteRenderer spriteRenderer = playerObj.GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false;

        //show enemy
        enemyObj.SetActive(true);

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
        Debug.Log($"[NextTurn] Called - Current turn: {currentTurn.moncargName}");
        
        if (!player.active)
        {
            Debug.Log("You need to switch Moncargs!");
            // ADDED: Check if all player Moncargs are dead
            if (!HasAnyAliveMoncargs())
            {
                OnAllMoncargsDefeated();
                return;
            }
            Debug.Log("[NextTurn] Player has other alive Moncargs, need to switch");
            return;
        }
        if (!enemy.active)
        {
            Debug.Log("[NextTurn] Enemy defeated! You won the battle!");
            OnEnemyDefeated();
            return;
        }

        combatUI.UpdateMoncargStats(player, enemy);

        if (currentTurn == player)
        {
            Debug.Log($"[NextTurn] Player's turn - Mana: {player.mana}/{player.maxMana}");
            
            //automatic resting
            if (player.mana <=0 )
            {
                Debug.Log($"[NextTurn] {player.moncargName} ran out of mana! Automatic resting...");
                Rest(player);
            }
            else
            {
                Debug.Log("[NextTurn] Your turn! Choose an action.");
                // UI buttons are active, waiting for player click
            }
        }
        else
        {
            Debug.Log($"[NextTurn] Enemy's turn - {enemy.moncargName} (Mana: {enemy.mana}/{enemy.maxMana})");
            EnemyTurn();
        }
    }

    private void OnAttackClicked(int attackOption)
    {
        if (currentTurn != player)
        {
            Debug.LogWarning($"[OnAttackClicked] Not player's turn! Current turn: {currentTurn.moncargName}");
            return;
        }

        Debug.Log($"[OnAttackClicked] Player chose attack option {attackOption}");

        SkillDefinition attackChoice = player.skillset[attackOption - 1];
        Debug.Log($"[OnAttackClicked] Selected skill: {attackChoice.name} (Mana cost: {attackChoice.manaCost})");

        if (attackChoice.name == "Rest")
        {
            Debug.Log("[OnAttackClicked] Rest selected, calling Rest()");
            Rest(player);
            return;
        }
        else
        {
            Debug.Log($"[OnAttackClicked] Starting attack: {attackChoice.name}");
            StartCoroutine(ExecuteAttackWithDelay(player, enemy, attackChoice, true));
        }
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
        Debug.Log($"[EnemyTurn] Starting enemy turn for {enemy.moncargName}");
        
        // List of moves with weights
        List<(SkillDefinition skill, float weight)> movePool = new List<(SkillDefinition, float)>();

        // Add Ultimate if available
        if (enemy.mana >= enemy.skillset[3].manaCost)
        {
            movePool.Add((enemy.skillset[3], 0.2f)); // 20%
            Debug.Log($"[EnemyTurn] Ultimate available: {enemy.skillset[3].name}");
        }

        // Add Elemental if available
        if (enemy.mana >= enemy.skillset[2].manaCost)
        {
            movePool.Add((enemy.skillset[2], 0.6f)); // 60%
            Debug.Log($"[EnemyTurn] Elemental available: {enemy.skillset[2].name}");
        }

        // Add Basic (always available, fallback)
        movePool.Add((enemy.skillset[1], 0.2f)); // 20%
        Debug.Log($"[EnemyTurn] Basic attack available: {enemy.skillset[1].name}");

        // If no mana at all for any move except basic chance to rest
        if (enemy.mana < enemy.skillset[2].manaCost && enemy.mana < enemy.skillset[3].manaCost)
        {
            float restRoll = Random.value;
            Debug.Log($"[EnemyTurn] Low mana! Rest roll: {restRoll} (need < 0.3)");
            
            if (restRoll < 0.3f) // 30% chance to rest early
            {
                Debug.Log($"[EnemyTurn] {enemy.moncargName} is low on mana and decides to Rest...");
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
                Debug.Log($"[EnemyTurn] Selected: {chosenSkill.name}");
                break;
            }
        }

        // Execute chosen move
        Debug.Log($"[EnemyTurn] {enemy.moncargName} chose {chosenSkill.name}");
        StartCoroutine(ExecuteAttackWithDelay(enemy, player, chosenSkill, false));
    }

    private void EndTurn()
    {
        Debug.Log($"[EndTurn] Current turn was: {currentTurn.moncargName}");
        
        // Swap turn
        Moncarg temp = currentTurn;
        currentTurn = other;
        other = temp;

        Debug.Log($"[EndTurn] Next turn is: {currentTurn.moncargName}");
        
        NextTurn();
    }

    //END EVENT DRIVEN BEGIN ENCOUNTER

    #region Attack Execution
    // ADDED: Coroutine to handle attack with 1-second delay
 private IEnumerator ExecuteAttackWithDelay(Moncarg attacker, Moncarg defender, SkillDefinition attackChoice, bool isPlayerAttacking)
{
    // Wait for 1 second before executing attack
    yield return new WaitForSeconds(1f);
    
    // Check if objects still exist before executing attack
    if (attacker == null || defender == null || attacker.gameObject == null || defender.gameObject == null)
    {
        Debug.LogWarning("[ExecuteAttackWithDelay] Attacker or Defender was destroyed before attack could execute");
        yield break;
    }
    
    // Execute the attack
    ExecuteAttack(attacker, defender, attackChoice, isPlayerAttacking);
    
    // End turn after attack completes
    EndTurn();
}

    public void ExecuteAttack(Moncarg attacker, Moncarg defender, SkillDefinition attackChoice, bool isPlayerAttacking)
    {
        Debug.Log($"[ExecuteAttack] {attacker.moncargName} attacking {defender.moncargName} with {attackChoice.name}");
        
        bool isDodged = TryDodge(defender);
        
        if (isDodged)
        {
            Debug.Log($"[ExecuteAttack] {defender.moncargName} dodged the attack!");
            return;
        }

        // Play slash animation only if not Rest and not dodged
        if (attackChoice.name != "Rest")
        {
            PlaySlashAnimation(isPlayerAttacking, attacker.transform, defender.transform);
        }

        // Deduct mana cost
        attacker.mana -= attackChoice.manaCost;
        Debug.Log($"[ExecuteAttack] {attacker.moncargName} used {attackChoice.manaCost} mana. Remaining: {attacker.mana}/{attacker.maxMana}");

        // Calculate base damage
        float damage = attackChoice.damage + attacker.attack - defender.defense;
        Debug.Log($"[ExecuteAttack] Base damage calculation: {attackChoice.damage} + {attacker.attack} - {defender.defense} = {damage}");

        damage = checkElemental(attacker, defender, attackChoice, damage);
        Debug.Log($"[ExecuteAttack] After elemental modifier: {damage}");

        // Ensure damage is not negative
        if (damage < 0)
        {
            damage = 0;
            Debug.Log($"[ExecuteAttack] Damage clamped to 0");
        }

        // Apply damage to defender
        float oldHealth = defender.health;
        defender.health -= damage;
        Debug.Log($"[ExecuteAttack] {defender.moncargName} health: {oldHealth} -> {defender.health}");

        // ADDED: Flash red and show damage indicator
        StartCoroutine(FlashRed(defender));
        ShowDamageIndicator(defender, damage);

        Debug.Log($"[ExecuteAttack] {attacker.moncargName} used {attackChoice.name} on {defender.moncargName} for {damage} damage!");

        // Check if defender is defeated
        if (defender.health <= 0)
        {
            defender.health = 0;
            defender.active = false;
            Debug.Log($"[ExecuteAttack] {defender.moncargName} has been defeated!");
        }

        combatUI.UpdateMoncargStats(player, enemy);
    }

    // ==========================================
    // DAMAGE INDICATOR + SPRITE FLASH SECTION
    // ==========================================
private void ShowDamageIndicator(Moncarg moncarg, float damage)
{
    if (moncarg == null || damageIndicatorPrefab == null || combatCanvas == null)
    {
        Debug.LogWarning("[CombatHandler] Missing references for damage indicator!");
        return;
    }
    
    // Convert world position to screen position
    Vector3 screenPos = Camera.main.WorldToScreenPoint(moncarg.transform.position + Vector3.up * 1f);
    
    // Instantiate as child of combat canvas
    GameObject indicator = Instantiate(damageIndicatorPrefab, combatCanvas.transform);
    indicator.transform.position = screenPos;
    indicator.transform.localScale = new Vector3(5f, 5f, 1f);
    
    FloatingDamageText dmgText = indicator.GetComponent<FloatingDamageText>();
    if (dmgText != null)
    {
        dmgText.Initialize(damage);
    }
    else
    {
        Debug.LogError("[CombatHandler] FloatingDamageText component not found on prefab!");
    }
}
    
    private IEnumerator FlashRed(Moncarg moncarg)
    {
        if (moncarg == null)
        {
            Debug.LogWarning("[CombatHandler] FlashRed: Moncarg is null");
            yield break;
        }
        
        SpriteRenderer sr = moncarg.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogWarning($"[CombatHandler] No SpriteRenderer found on {moncarg.moncargName}");
            yield break;
        }
        
        // Store the ORIGINAL color (should be white)
        Color originalColor = Color.white;
        
        // Force set to red
        sr.color = Color.red;
        
        // Wait for flash duration
        yield return new WaitForSeconds(0.15f);
        
        // Restore to white (default sprite color)
        if (sr != null)
        {
            sr.color = originalColor;
        }
    }

    // Slash animation method
private void PlaySlashAnimation(bool isPlayerAttacking, Transform attackerTransform, Transform defenderTransform)
{
    if (slashAnimator != null && slashTransform != null && attackerTransform != null && defenderTransform != null)
    {
        Vector3 startPos = attackerTransform.position;
        Vector3 endPos = defenderTransform.position;

        slashTransform.position = startPos;

        Vector3 scale = new Vector3(10f, 10f, 1f);
        scale.x = isPlayerAttacking ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        slashTransform.localScale = scale;

        float angle = isPlayerAttacking ? -45f : 45f;
        slashTransform.rotation = Quaternion.Euler(0, 0, angle);

        slashTransform.gameObject.SetActive(true);
        StartCoroutine(PlaySlashMoveCoroutine(startPos, endPos));
    }
}

    private IEnumerator PlaySlashMoveCoroutine(Vector3 start, Vector3 end)
    {
        int timesToPlay = 2;
        for (int i = 0; i < timesToPlay; i++)
        {
            slashAnimator.SetBool("isSlashing", true);

            float timer = 0f;
            while (timer < slashDuration)
            {
                slashTransform.position = Vector3.Lerp(start, end, timer / slashDuration);
                timer += Time.deltaTime;
                yield return null;
            }

            slashTransform.position = end;
            slashAnimator.SetBool("isSlashing", false);
            yield return new WaitForSeconds(0.05f);
        }

        slashTransform.position = new Vector3(1000f, 1000f, 0f);
        slashTransform.gameObject.SetActive(false);
        slashTransform.rotation = Quaternion.identity;
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

    // ADDED: Game Over System Methods
    #region Game Over System
    private bool HasAnyAliveMoncargs()
    {
        // Check if any equipped Moncargs are still alive (health > 0)
        var equippedMoncargs = PlayerInventory.Instance.StoredMoncargs
            .Where(m => m.IsEquipped && m?.Details != null)
            .ToList();

        foreach (var storedMoncarg in equippedMoncargs)
        {
            if (storedMoncarg.Details.moncargData.health > 0)
            {
                return true; // Found at least one alive Moncarg
            }
        }

        Debug.Log("All equipped Moncargs are defeated!");
        return false; // All Moncargs are dead
    }

    private void OnAllMoncargsDefeated()
    {
        Debug.Log("OnAllMoncargsDefeated() called!");
        
        // Hide combat UI
        combatUI.ShowCombatUI(false);
        
        // Trigger game over through GameManager
        if (GameManager.Instance != null)
        {
            Debug.Log("Calling GameManager.TriggerGameOver()");
            GameManager.Instance.TriggerGameOver();
        }
        else
        {
            Debug.LogError("GameManager.Instance is null!");
        }
        
        // Clean up combat objects
        Cleanup();
    }

    private void CleanupForGameOver()
    {
        // Destroy game objects but don't re-enable move buttons
        // since we're going to game over screen
        if (player != null && player.gameObject != null)
        {
            GameObject.Destroy(player.gameObject);
        }
        if (enemy != null && enemy.gameObject != null)
        {
            GameObject.Destroy(enemy.gameObject);
        }
    }
    #endregion

    private void Rest(Moncarg moncarg)
    {
        Debug.Log($"[Rest] {moncarg.moncargName} is resting...");
        
        int manaRecovered = moncarg.maxMana / 4; // Recover 25% of max mana
        moncarg.mana += manaRecovered;
        
        //in case of overheal
        if (moncarg.mana > moncarg.maxMana)
        {
            moncarg.mana = moncarg.maxMana;
        }

        combatUI.UpdateMoncargStats(player, enemy);

        Debug.Log($"[Rest] {moncarg.moncargName} recovered {manaRecovered} mana. Current: {moncarg.mana}/{moncarg.maxMana}");
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
        selectionUI.Show(equippedMoncargs);

        GameObject.Destroy(player.gameObject);
    }

    private void OnEnemyDefeated()
    {
        // ADDED: Generate item drops when enemy is defeated
        GenerateItemDrops();

        if (!IsBossOrMiniboss(enemy))
        {
            combatUI.ShowCatchPanel();
        }

        if (enemy.data.isBoss)
        {
            victoryScreen.SetActive(true);
        }
        Cleanup();
    }

    // ADDED: Item drop system methods
    #region Item Drop System
    private void GenerateItemDrops()
    {
        // ADDED: Check if enemy is a boss or miniboss only
        if (!IsBossOrMiniboss(enemy))
        {
            Debug.Log($"{enemy.moncargName} is not a boss/miniboss - no item drops");
            return;
        }

        // Check if anything drops at all
        float dropRoll = Random.value;
        if (dropRoll > dropChance)
        {
            Debug.Log($"No items dropped from {enemy.moncargName}");
            return;
        }

        // Determine rarity
        ItemDefinition droppedItem = null;
        float rarityRoll = Random.value;

        if (rarityRoll <= legendaryDropChance && legendaryDrops != null && legendaryDrops.Length > 0)
        {
            // Legendary drop
            droppedItem = legendaryDrops[Random.Range(0, legendaryDrops.Length)];
            Debug.Log($"LEGENDARY DROP! {enemy.moncargName} dropped {droppedItem.FriendlyName}!");
        }
        else if (rarityRoll <= legendaryDropChance + rareDropChance && rareDrops != null && rareDrops.Length > 0)
        {
            // Rare drop
            droppedItem = rareDrops[Random.Range(0, rareDrops.Length)];
            Debug.Log($"Rare drop! {enemy.moncargName} dropped {droppedItem.FriendlyName}!");
        }
        else if (commonDrops != null && commonDrops.Length > 0)
        {
            // Common drop
            droppedItem = commonDrops[Random.Range(0, commonDrops.Length)];
            Debug.Log($"Boss {enemy.moncargName} dropped {droppedItem.FriendlyName}");
        }

        // Add item to inventory if we got something
        if (droppedItem != null && PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.AddItemToInventory(droppedItem);
        }
    }

    // ADDED: Method to check if enemy is a boss or miniboss
    private bool IsBossOrMiniboss(Moncarg enemy)
    {
        if (enemy.data.isBoss || enemy.data.isMiniBoss)
        {
            return true;
        }

        return false;
    }

    // Optional: Method to set custom drop tables for specific enemies
    public void SetCustomDropTable(ItemDefinition[] commons, ItemDefinition[] rares, ItemDefinition[] legendaries)
    {
        commonDrops = commons;
        rareDrops = rares;
        legendaryDrops = legendaries;
    }

    // Optional: Method to modify drop chances dynamically
    public void SetDropChances(float baseChance, float rareChance, float legendaryChance)
    {
        dropChance = Mathf.Clamp01(baseChance);
        rareDropChance = Mathf.Clamp01(rareChance);
        legendaryDropChance = Mathf.Clamp01(legendaryChance);
    }
    #endregion

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
        
        // ADDED: Check if Moncarg inventory is full before adding
        if (PlayerInventory.Instance.IsMoncargInventoryFull())
        {
            Debug.Log($"Cannot store {enemy.moncargName} - Moncarg inventory is full! Consider releasing some Moncargs first.");
            // You could show a UI message here instead of just logging
            Cleanup();
            return;
        }
        
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

        //reset enemy stats
        resetEnemy();

        //Destroy moncarg game objects to prevent duplicates
        GameObject.Destroy(player.gameObject);
        GameObject.Destroy(enemy.gameObject);

        //reshow the room grid
        FindFirstObjectByType<BoardManager>().enableRoom();

        //reshow player sprite
        GameObject playerObj = GameObject.FindWithTag("Player");
        SpriteRenderer spriteRenderer = playerObj.GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = true;

        //reenable move buttons
        GameManager.Instance.moveUI.EnableAllButtons();
    }

    private void resetEnemy()
    {
        //Retrieve enemy moncarg game object and StoredMoncarg component
        GameObject enemyGO = enemy.gameObject;
        StoredMoncarg enemyStoredMoncarg = enemyGO.GetComponent<StoredMoncarg>();
        enemyStoredMoncarg.Details.moncargData.reset(); //reset health, mana and status
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

    private System.Collections.IEnumerator StartMoncargSelection()
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
            selectionUI.Show(equippedMoncargs);
        }
    }

    private void ForcePlayerToEquipMoncarg()
    {
        waitingForPlayerToEquip = true;

        forceEquipUI.ShowPrompt("Please equip at least one Moncarg from your inventory to continue!");

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

    private System.Collections.IEnumerator WaitForPlayerToEquip()
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
                selectionUI.Show(equippedMoncargs);
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

        if (forceEquipUI != null)
        {
            forceEquipUI.HidePrompt();
        }

        // Spawn the selected moncarg for battle
        GameObject playerMoncargObj = selectedMoncarg.CreateMoncargGameObject();
        playerMoncargObj.transform.localScale = new Vector3(15f, 15f, 0f);
        playerMoncargObj.transform.position = new Vector3(-15f, 0f, 0f);
        player = playerMoncargObj.GetComponent<Moncarg>();
        player.InitStats();

        // Begin battle
        BeginBattle();
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
        if (selectionUI != null)
        {
            selectionUI.OnMoncargSelected -= OnMoncargSelected;
            selectionUI.OnSelectionCancelled -= OnSelectionCancelled;
        }
    }

    // Public method to cancel the equip waiting (if player closes inventory without equipping)
    public void CancelEquipWaiting()
    {
        waitingForPlayerToEquip = false;
    }
    #endregion
}