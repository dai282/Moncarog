using System;
using Elementals;

[System.Serializable]
public class MoncargData
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
    public ElementalType type;
    public bool active;
    public SkillDefinition[] skillset = new SkillDefinition[4];

    public void reset()
    {
        health = maxHealth;
        mana = maxMana;
        active = true;
    }
}