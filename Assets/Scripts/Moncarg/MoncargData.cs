using System;
using Elementals;
using UnityEngine;

[System.Serializable]
public class MoncargData
{
    public string moncargName;

    //Base Stats
    public float baseMaxHealth;
    public float baseAttack;
    public float baseDefense;
    public int baseSpeed;
    public int baseMaxMana;
    

    //Current Stats
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
    public ElementalType type;
    public bool active;
    public SkillDefinition[] skillset = new SkillDefinition[4];
    public bool isBoss;
    public bool isMiniBoss;

    //Level Progression
    public int expToNextLevel;
    public const int BASE_EXP_REQUIRED = 100;
    public const float EXP_GROWTH_RATE = 1.2f;

    public void InitializeBaseStats()
    {
        // Set base stats (these should be set in the inspector for each Moncarg)
        baseMaxHealth = maxHealth;
        baseAttack = attack;
        baseDefense = defense;
        baseSpeed = speed;
        baseMaxMana = maxMana;

        level = 1;
        exp = 0;
        CalculateExpToNextLevel();
        ScaleStatsToLevel();
        reset();
    }

    public void reset()
    {
        health = maxHealth;
        mana = maxMana;
        active = true;
    }


    public void ScaleStatsToLevel()
    {
        // Stat scaling formula: base * (1 + (level - 1) * scalingFactor)
        float healthScaling = 0.15f;    // 15% increase per level
        float attackScaling = 0.12f;    // 12% increase per level  
        float defenseScaling = 0.10f;   // 10% increase per level
        float speedScaling = 0.08f;     // 8% increase per level
        float manaScaling = 0.10f;      // 10% increase per level

        maxHealth = baseMaxHealth * (1 + (level - 1) * healthScaling);
        attack = baseAttack * (1 + (level - 1) * attackScaling);
        defense = baseDefense * (1 + (level - 1) * defenseScaling);
        speed = Mathf.RoundToInt(baseSpeed * (1 + (level - 1) * speedScaling));
        maxMana = Mathf.RoundToInt(baseMaxMana * (1 + (level - 1) * manaScaling));

        // Ensure minimum values
        maxHealth = Mathf.Max(maxHealth, baseMaxHealth);
        attack = Mathf.Max(attack, baseAttack);
        defense = Mathf.Max(defense, baseDefense);
        speed = Mathf.Max(speed, baseSpeed);
        maxMana = Mathf.Max(maxMana, baseMaxMana);
    }

    public void CalculateExpToNextLevel()
    {
        expToNextLevel = Mathf.RoundToInt(BASE_EXP_REQUIRED * Mathf.Pow(EXP_GROWTH_RATE, level - 1));
    }

    public bool AddExp(int expGained)
    {
        exp += expGained;
        bool leveledUp = false;

        //when exp gained exceeds next level threshold, level up
        while (exp >= expToNextLevel && level < 12) // Cap at level 12
        {
            exp -= expToNextLevel;
            level++;
            leveledUp = true;

            ScaleStatsToLevel();
            CalculateExpToNextLevel();

            Debug.Log($"{moncargName} leveled up to level {level}!");
        }

        return leveledUp;
    }

    //call this when defeating an enemy
    public int GetExpForDefeating(int enemyLevel)
    {
        // Base EXP = 50, scaled by level difference
        int baseExp = 50;
        float levelMultiplier = 1.0f + (enemyLevel - level) * 0.1f;
        return Mathf.RoundToInt(baseExp * levelMultiplier);
    }
}