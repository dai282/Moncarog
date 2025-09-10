using UnityEngine;
using Elementals;
using UnityEngine.UIElements;

public class Moncarg : MonoBehaviour
{

    //moncarg stats are now stored in MoncargData ScriptableObject
    public MoncargData data;

    public string moncargName { get => data.moncargName; set => data.moncargName = value; }
    public float maxHealth { get => data.maxHealth; set => data.maxHealth = value; }
    public float health { get => data.health; set => data.health = value; }
    public float attack { get => data.attack; set => data.attack = value; }
    public float defense { get => data.defense; set => data.defense = value; }
    public int speed { get => data.speed; set => data.speed = value; }
    public int exp { get => data.exp; set => data.exp = value; }
    public int level { get => data.level; set => data.level = value; }
    public int mana { get => data.mana; set => data.mana = value; }
    public int maxMana { get => data.maxMana; set => data.maxMana = value; }
    public float catchChance { get => data.catchChance; set => data.catchChance = value; }
    public float dodgeChance { get => data.dodgeChance; set => data.dodgeChance = value; }
    public bool active { get => data.active; set => data.active = value; }
    public ElementalType type { get => data.type; set => data.type = value; }
    public SkillDefinition[] skillset { get => data.skillset; set => data.skillset = value; }

    /*
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
    public ElementalType type;*/

    //public Skill[] skillset;
    //private SkillList skillList;

    public enum moncargRole
    {
        PlayerOwned,
        Wild
    }

    public moncargRole role;

    
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

        Debug.Log("Moncarg Initialized");

    }
    // New method to get data for inventory
    public MoncargData GetMoncargData()
    {
        return data;
    }

    // New method to load data from inventory
    public void LoadMoncargData(MoncargData newData)
    {
        data = newData;
        //InitStats(); // Reinitialize with loaded data
    }


}