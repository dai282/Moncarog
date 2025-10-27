using UnityEngine;
using Elementals; //for elemental types
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class CombatHandler : MonoBehaviour
{
    #region Variables
    [SerializeField] private CombatHandlerUI combatUI;
    [SerializeField] private MoncargSelectionUI selectionUI;
    [SerializeField] private ForceEquipPromptUI forceEquipUI;
    [SerializeField] private CameraManager cam;

    [Header("Combat Positions")] // Add this header for organization
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private Transform enemySpawnPoint;

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

    // Making these constants, and also static if they need to be accessed globally
    public const float DROP_CHANCE = 0.7f;            // 70% chance to drop something
    public const float RARE_DROP_CHANCE = 0.15f;      // 15% chance for rare
    public const float LEGENDARY_DROP_CHANCE = 0.05f; // 5% chance for legendary

    [Header("Sound Effects")]
    [SerializeField] private AudioClip normalAttackSoundFX;
    [SerializeField] private AudioClip fireAttackSoundFX;
    [SerializeField] private AudioClip waterAttackSoundFX;
    [SerializeField] private AudioClip plantAttackSoundFX;
    [SerializeField] private AudioClip levelUpSoundFX;

    private Moncarg player;
    private Moncarg enemy;
    private Moncarg currentTurn;
    private Moncarg other;
    private MoncargDatabase moncargDatabase;
    private GameObject enemyObj;
    private GameObject playerObj;
    private bool waitingForPlayerToEquip = false;
    public bool encounterStarted = false;

    #endregion

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

        InstantiateMoncargBasedOnRoomID(roomID);

        //enemyObj.transform.localScale = new Vector3(15f, 15f, 0f);
        //enemyObj.transform.position = new Vector3(15f, 0f, 0f);

        enemyObj.transform.position = enemySpawnPoint.position;
        enemyObj.transform.rotation = enemySpawnPoint.rotation; // Good practice to also set rotation

        enemy = enemyObj.GetComponent<Moncarg>();
        if (enemy.isBoss || enemy.isMiniBoss)
        {
            combatUI.DisableFlee();
        }
        enemy.InitStats();
        //hide until combat starts
        enemyObj.SetActive(false);

        //auto equip moncargs if none are equipped
        AutoEquipMoncargs();

        StartCoroutine(StartMoncargSelection());

        //play combat music
        MusicManager.Instance.SwapTrack();
    }

    private void InstantiateMoncargBasedOnRoomID(int roomID)
    {
        //Create enemy Moncarg instance for battle
        moncargDatabase = GameManager.Instance.moncargDatabase;
        int databaseLen = moncargDatabase.availableEnemyMoncargs.Count;
        int databaseF = moncargDatabase.fireMoncargs.Count;
        int databaseP = moncargDatabase.plantMoncargs.Count;
        int databaseW = moncargDatabase.waterMoncargs.Count;
        int databaseN = moncargDatabase.normalMoncargs.Count;
        int numberOfBossesAndMinibosses = moncargDatabase.GetNumberOfBossAndMiniboss();
        // Instantiate moncarg and check if they're boss or miniboss rooms
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
                case -10:
                    break;
                case -11:
                    break;
                default:
                    Debug.LogError("Invalid miniboss/boss room ID: " + roomID);
                    int randIndex = Random.Range(0, databaseLen - numberOfBossesAndMinibosses);
                    enemyObj = Instantiate(moncargDatabase.availableEnemyMoncargs[randIndex]);
                    break;
            }
        }
        //otherwise just spawn a normal moncarg
        else
        {
            if (1 < roomID && roomID < 6)
            {
                int randIndex = Random.Range(0, databaseN);
                enemyObj = Instantiate(moncargDatabase.normalMoncargs[randIndex]);
                //Debug.Log($"Normal");
            }
            if (5 < roomID && roomID < 11)
            {
                int randIndex = Random.Range(0, databaseP);
                enemyObj = Instantiate(moncargDatabase.plantMoncargs[randIndex]);
                //Debug.Log($"Grass");
            }
            if (10 < roomID && roomID < 16)
            {
                Debug.Log("Number of water moncargs available: " + databaseW);
                int randIndex = Random.Range(0, databaseW);
                enemyObj = Instantiate(moncargDatabase.waterMoncargs[randIndex]);
                //Debug.Log($"Water");
            }
            if (15 < roomID && roomID < 21)
            {
                int randIndex = Random.Range(0, databaseF);
                enemyObj = Instantiate(moncargDatabase.fireMoncargs[randIndex]);
                //Debug.Log($"Fire");
            }
        }
    }

    public void BeginBattle()
    {
        //Move the Camera
        cam.LockToPoint();

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

            //remove defeated moncarg from inventory
            PlayerInventory.Instance.RemoveMoncargFromInventory(player.GetComponent<StoredMoncarg>().Details);

            //disable skill buttons
            combatUI.DisableSkillButtons(false);

            combatUI.UpdateCombatTerminal($"{player.moncargName} was defeated!");
            combatUI.UpdateCombatTerminal("You need to switch Moncargs!");

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
            Debug.Log("You won the battle!");
            combatUI.UpdateCombatTerminal("You won the battle!");

            Debug.Log("[NextTurn] Enemy defeated! You won the battle!");
            OnEnemyDefeated();
            return;
        }

        combatUI.UpdateMoncargStats(player, enemy);

        if (currentTurn == player)
        {
            Debug.Log($"[NextTurn] Player's turn - Mana: {player.mana}/{player.maxMana}");

            //automatic resting
            if (player.mana <= 0)
            {
                Debug.Log(player.moncargName + " ran out of mana! Automatic resting...");
                combatUI.UpdateCombatTerminal("Ran out of mana! Automatic resting...");

                Debug.Log($"[NextTurn] {player.moncargName} ran out of mana! Automatic resting...");
                Rest(player);
            }
            else
            {
                Debug.Log("Your turn! Choose an action.");
                combatUI.UpdateCombatTerminal("Your turn! Choose an action.");

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
                Debug.Log(enemy.moncargName + " is low on mana and decides to Rest...");
                combatUI.UpdateCombatTerminal(enemy.moncargName + "is low on mana and decides to Rest...");

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
        PlayAttackSoundFX(attackChoice);

        Debug.Log($"[ExecuteAttack] {attacker.moncargName} attacking {defender.moncargName} with {attackChoice.name}");

        bool isDodged = TryDodge(defender);

        if (isDodged)
        {
            Debug.Log($"[ExecuteAttack] {defender.moncargName} dodged the attack!");
            combatUI.UpdateCombatTerminal($"{defender.moncargName} dodged the attack!");
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

        if (attacker == player)
        {
            StatsCollector.Instance?.RecordManaChange(attackChoice.manaCost, false); // Record mana spent
            StatsCollector.Instance?.RecordAbilityUsed(); // Record that an ability was used
                                                          // Calculate base damage
        }
        // Use the testable calculation method
        float damage = CalculateDamage(attacker, defender, attackChoice);

        // Apply damage to defender
        float oldHealth = defender.health;
        defender.health -= damage;
        Debug.Log($"[ExecuteAttack] {defender.moncargName} health: {oldHealth} -> {defender.health}");

        if (attacker == player && damage > 0)
        {
            StatsCollector.Instance?.RecordDamageDealt(damage); // Record damage dealt BY player
        }
        else if (defender == player && damage > 0)
        {
            StatsCollector.Instance?.RecordHPChange(damage, false); // Record HP lost BY player
        }

        Debug.Log(attacker.moncargName + " used " + attackChoice.name + " on " + defender.moncargName + " for " + damage + " damage!");
        combatUI?.UpdateCombatTerminal(attacker.moncargName + " used " + attackChoice.name + " on " + defender.moncargName + " for " + damage + " damage!");
        // ADDED: Flash red and show damage indicator
        StartCoroutine(FlashRed(defender));
        ShowDamageIndicator(defender, damage);

        Debug.Log($"[ExecuteAttack] {attacker.moncargName} used {attackChoice.name} on {defender.moncargName} for {damage} damage!");

        // Check if defender is defeated
        if (defender.health <= 0)
        {
            defender.health = 0;
            defender.active = false;
            Debug.Log(defender.moncargName + " has been defeated!");
            combatUI.UpdateCombatTerminal(defender.moncargName + " has been defeated!");

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

        //// Convert world position to screen position
        //Vector3 screenPos = Camera.main.WorldToScreenPoint(moncarg.transform.position + Vector3.up * 1f);

        //// Instantiate as child of combat canvas
        //GameObject indicator = Instantiate(damageIndicatorPrefab, combatCanvas.transform);
        //indicator.transform.position = screenPos;

        // --- NEW AND CORRECTED LOGIC ---

        // 1. Get the RectTransform of the canvas.
        RectTransform canvasRect = combatCanvas.GetComponent<RectTransform>();

        // 2. Convert the Moncarg's world position to a screen pixel position.
        // (Your original line for this was correct)
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(moncarg.transform.position + Vector3.up * 1.5f); // Increased the offset a bit to appear above the head

        // 3. Convert the screen pixel position to a local position within the canvas.
        // This is the key step.
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, combatCanvas.worldCamera, out localPoint);

        // 4. Instantiate the indicator as a child of the canvas.
        GameObject indicator = Instantiate(damageIndicatorPrefab, combatCanvas.transform);
        indicator.SetActive(true);

        // 5. Set the indicator's anchoredPosition to the calculated local point.
        indicator.GetComponent<RectTransform>().anchoredPosition = localPoint;

        // --- End of new logic ---

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

            Vector3 scale = new Vector3(5f, 5f, 1f);
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

    public float CalculateDamage(Moncarg attacker, Moncarg defender, SkillDefinition attackChoice)
    {
        // Pure damage calculation logic - easily testable!
        float damage = attackChoice.damage + attacker.attack - defender.defense;
        damage = checkElemental(attacker, defender, attackChoice, damage);

        if (damage < 0)
        {
            damage = 0;
        }

        return damage;
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

        return Mathf.RoundToInt(damage);
    }

    private void PlayAttackSoundFX(SkillDefinition attackChoice)
    {
        if (attackChoice.type == ElementalType.Fire)
        {
            SoundFxManager.Instance.PlaySoundFXClip(fireAttackSoundFX, transform, 1f);
        }
        else if (attackChoice.type == ElementalType.Water)
        {
            SoundFxManager.Instance.PlaySoundFXClip(waterAttackSoundFX, transform, 1f);
        }
        else if (attackChoice.type == ElementalType.Plant)
        {
            SoundFxManager.Instance.PlaySoundFXClip(plantAttackSoundFX, transform, 1f);
        }
        else
        {
            SoundFxManager.Instance.PlaySoundFXClip(normalAttackSoundFX, transform, 1f);
        }
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

        if (moncarg == player)
        {
            StatsCollector.Instance?.RecordManaChange(manaRecovered, true); // Record mana recovered
        }

        combatUI.UpdateMoncargStats(player, enemy);

        Debug.Log(moncarg.moncargName + " rested and recovered " + manaRecovered + " mana.");
        combatUI.UpdateCombatTerminal(moncarg.moncargName + " rested and recovered " + manaRecovered + " mana.");

        EndTurn();
    }

    private void OnInventoryClicked()
    {
        PlayerInventory.Instance.ShowInventory();
    }

    private void OnSwitchClicked()
    {
        SoundFxManager.Instance.PlayButtonFXClip();

        var equippedMoncargs = PlayerInventory.Instance.StoredMoncargs
            .Where(m => m.IsEquipped)
            .Select(m => m.Details)
            .ToList();

        // Show selection UI for multiple equipped moncargs
        selectionUI.Show(equippedMoncargs);

        combatUI.UpdateCombatTerminal("Switching out " + player.moncargName);

        GameObject.Destroy(player.gameObject);
    }

    private void OnEnemyDefeated()
    {
        StatsCollector.Instance?.RecordMoncarogDefeated();

        //Distribute Experience
        DistributeExpToPlayerMoncargs(enemy.level);

        // ADDED: Generate item drops when enemy is defeated
        GenerateItemDrops();

        if (!IsBossOrMiniboss(enemy))
        {
            combatUI.ShowCatchPanel();
        }
        else if (enemy.data.isBoss)
        {
            GameManager.Instance.TriggerVictory();
            Cleanup();
        }
        else
        {
            Cleanup();
        }


    }

    #region Distribute Experience
    private void DistributeExpToPlayerMoncargs(int enemyLevel)
    {
        if (PlayerInventory.Instance == null) return;

        foreach (var storedMoncarg in PlayerInventory.Instance.StoredMoncargs)
        {
            if (storedMoncarg?.Details?.moncargData != null)
            {
                MoncargData data = storedMoncarg.Details.moncargData;
                int expGained = data.GetExpForDefeating(enemyLevel);
                bool leveledUp = data.AddExp(expGained);

                Debug.Log($"{data.moncargName} gained {expGained} EXP (Total: {data.exp}/{data.expToNextLevel})");

                if (leveledUp)
                {
                    // Show level up UI or effects
                    ShowLevelUpEffect(data.moncargName, data.level);
                }
            }
        }
    }

    private void ShowLevelUpEffect(string moncargName, int newLevel)
    {
        // You can implement UI popup or visual effects here
        Debug.Log($"{moncargName} reached level {newLevel}!");

        // Example: Show alert message
        if (AlertManager.Instance != null)
        {
            AlertManager.Instance.ShowAlert($"{moncargName} reached level {newLevel}!", 3f);
        }

        SoundFxManager.Instance.PlaySoundFXClip(levelUpSoundFX, transform, 1f);
    }
    #endregion

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
        if (dropRoll > DROP_CHANCE)
        {
            Debug.Log($"No items dropped from {enemy.moncargName}");
            return;
        }

        // Determine rarity
        ItemDefinition droppedItem = null;
        float rarityRoll = Random.value;

        if (rarityRoll <= LEGENDARY_DROP_CHANCE && legendaryDrops != null && legendaryDrops.Length > 0)
        {
            // Legendary drop
            droppedItem = legendaryDrops[Random.Range(0, legendaryDrops.Length)];
            Debug.Log($"LEGENDARY DROP! {enemy.moncargName} dropped {droppedItem.FriendlyName}!");
        }
        else if (rarityRoll <= LEGENDARY_DROP_CHANCE + RARE_DROP_CHANCE && rareDrops != null && rareDrops.Length > 0)
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
            AlertManager.Instance.ShowAlert("Failed to catch " + enemy.moncargName + "!");
            Cleanup();
        }
    }

    private void OnCatchSussess()
    {


        // ADDED: Check if Moncarg inventory is full before adding
        if (PlayerInventory.Instance.IsMoncargInventoryFull())
        {
            // You could show a UI message here instead of just logging
            AlertManager.Instance.ShowAlert("Moncarg inventory is full! Cannot catch " + enemy.moncargName + "!");
            Cleanup();
            return;
        }

        StatsCollector.Instance?.RecordMoncarogCollected();


        AlertManager.Instance.ShowNotification("Successfully caught " + enemy.moncargName + "!");
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
        encounterStarted = false;

        if (player.gameObject.GetComponent<StoredMoncarg>().Details.moncargData.LeveledUp())
        {
            AlertManager.Instance.ShowNotification($"{player.moncargName} leveled up to level {player.level}!", 4.0f);
        }

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

        //Return camera to player
        cam.ResumeFollowing();

        //Play background music
        MusicManager.Instance.SwapTrack();
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
            selectionUI.Show(equippedMoncargs, true);
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

        //playerMoncargObj.transform.localScale = new Vector3(15f, 15f, 0f);
        //playerMoncargObj.transform.position = new Vector3(-15f, 0f, 0f);

        // --- NEW CODE ---
        playerMoncargObj.transform.position = playerSpawnPoint.position;
        playerMoncargObj.transform.rotation = playerSpawnPoint.rotation; // Good practice

        playerObj = playerMoncargObj;
        player = playerMoncargObj.GetComponent<Moncarg>();
        player.InitStats();

        // Begin battle
        BeginBattle();
    }

    private void OnSelectionCancelled()
    {
        //Auto selection and force equip only applies at the beginning of encounter
        //this prevents counting a turn when player switches Moncargs mid 
        if (selectionUI.IsEncounterStart())
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
        else
        {
            // Mid-combat cancellation - just close the UI, don't auto-select or count as a turn
            Debug.Log("Mid-combat switch cancelled. Keeping current Moncarg.");
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