using UnityEngine;

public class Moncarg : MonoBehaviour
{
    public string moncargName;
    public float maxHealth;
    public float health;
    public float attack;
    public float defense;
    public int speed;
    public int exp;
    public int level;
    public int mana;
    public float catchChance;
    public float dodgeChance;
    public bool active;

    public Skill basicAttack;
    public Skill skill;
    public Skill ultimate;
    public Skill[] skillset;
    private SkillList skillList;

    public enum elementalType
    {
        Fire,
        Water,
        Plant,
        Normal
    }

    public elementalType type;

    public enum moncargRole
    {
        PlayerOwned,
        Wild
    }

    public moncargRole role;

    //DELETE LATER, ONLY FOR TESTING COMBAT
    public CombatHandler combatHandler;
    public GameObject mockEnemyMoncargPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitStats();
    }

    public void InitStats()
    {
        if (maxHealth <= 0)
        {
            maxHealth = health;
        }
        active = true;
        health = maxHealth;

        basicAttack = new Skill("Tackle", "Basic", 30.0f, 20);
        skill = new Skill("Ignite", "Fire", 20.0f, 20);
        ultimate = new Skill("Incinerate", "Fire", 80.0f, 70);

        skillset = new Skill[3];
        skillset[0] = basicAttack;
        skillset[1] = skill;
        skillset[2] = ultimate;

        //mock enemy for testing combat system
        combatHandler = new CombatHandler();
        TestMockEncounter();

    }

    // TESTING ONLY - remove once encounter system is ready
    public void TestMockEncounter()
    {
        // Spawn a separate Moncarg object in memory (not the same as 'this')
        GameObject enemyObj = Instantiate(mockEnemyMoncargPrefab);
        Moncarg enemy = enemyObj.GetComponent<Moncarg>();
        
        enemy.moncargName = "Wild Moncarg";
        enemy.health = 50;
        enemy.maxHealth = 50;
        enemy.attack = 15;
        enemy.defense = 5;
        enemy.speed = 10;
        enemy.exp = 20;
        enemy.level = 1;
        enemy.mana = 30;
        enemy.catchChance = 0.2f;
        enemy.dodgeChance = 0.1f;
        enemy.active = true;
        enemy.skillset = new Skill[1];
        enemy.skillset[0] = new Skill("Scratch", "Normal", 10.0f, 5);
        enemy.mockEnemyMoncargPrefab = null; // Prevent infinite spawning

        // Run combat handler
        combatHandler.BeginEncounter(this, enemy);
    }

    //does not need Update() since combathandler will handle actions

}