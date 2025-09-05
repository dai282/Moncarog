using UnityEngine;
using Elementals;
using UnityEngine.UIElements;

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
    public int maxMana;
    public float catchChance;
    public float dodgeChance;
    public bool active;

    
    public Skill[] skillset;
    private SkillList skillList;

    public ElementalType type;

    public enum moncargRole
    {
        PlayerOwned,
        Wild
    }

    private Label m_MoncargHealthLabel;
    private Label m_MoncargManaLabel;

    public moncargRole role;

    //DELETE LATER, ONLY FOR TESTING COMBAT
    public GameObject mockEnemyMoncargPrefab;
        // these 2 need to be moved to Game Manager, after we merge Moncarg with Main, now here for testing health display in combat
    [SerializeField] private UIDocument moncargHealthUI;
    private CombatHandler combatHandler = new CombatHandler();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //this goes to Start in GameManager after we merge Moncarg with Main
        combatHandler.SetUI(moncargHealthUI);


        InitStats();
    }

    public void InitStats()
    {
        if (maxHealth <= 0)
        {
            maxHealth = health;
        }
        if (maxMana <= 0)
        {
            maxMana = mana;
        }
        active = true;
        health = maxHealth;
        mana = maxMana;

        skillList = new SkillList();

      

        skillset = new Skill[4];
        skillset[0] = skillList.skills[0];
        skillset[1] = skillList.skills[1];
        skillset[2] = skillList.skills[4];
        skillset[3] = skillList.skills[7];

        //mock enemy for testing combat system
        TestMockEncounter();

    }

    public void SetHealthLabel(Label label)
    {
        m_MoncargHealthLabel = label;
        UpdateHealthLabel(); // Initialize
    }

    public void UpdateHealthLabel()
    {
        if (m_MoncargHealthLabel != null)
        {
            m_MoncargHealthLabel.text = $"{moncargName} HP: {health}";
        }
            
    }

    public void SetManaLabel(Label label)
    {
        m_MoncargManaLabel = label;
        UpdateManaLabel(); // Initialize
    }

    public void UpdateManaLabel()
    {
        if (m_MoncargManaLabel != null)
        {
            m_MoncargManaLabel.text = $"{moncargName} MP: {mana}";
        }
    }

    // TESTING ONLY - remove once encounter system is ready
    public void TestMockEncounter()
    {
        // Spawn a separate Moncarg object in memory (not the same as 'this')
        GameObject enemyObj = Instantiate(mockEnemyMoncargPrefab);
        Moncarg enemy = enemyObj.GetComponent<Moncarg>();
        
        enemy.moncargName = "Wild Moncarg";
        enemy.health = 500;
        enemy.maxHealth = 50;
        enemy.attack = 15;
        enemy.defense = 5;
        enemy.speed = 5;
        enemy.exp = 20;
        enemy.level = 1;
        enemy.maxMana = 30;
        enemy.mana = 30;
        enemy.type = ElementalType.Plant;
        enemy.catchChance = 0.2f;
        enemy.dodgeChance = 0.1f;
        enemy.active = true;
        enemy.skillset = new Skill[1];
        enemy.skillset[0] = new Skill("Scratch", ElementalType.Normal, 10.0f, 5);
        enemy.mockEnemyMoncargPrefab = null; // Prevent infinite spawning

        // Run combat handler
        combatHandler.BeginEncounter(this, enemy);
    }

    //does not need Update() since combathandler will handle actions

}