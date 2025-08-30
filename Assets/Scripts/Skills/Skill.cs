using UnityEngine;

public class Skill
{
    public enum elementalType
    {
        Fire,
        Water,
        Plant,
        Normal
    }

    public string name;
    public elementalType type;
    public float damage;
    public int manaCost;

    //Constructor
    public Skill(string name, elementalType type, float damage, int manaCost)
    {
        this.name = name;
        this.type = type;
        this.damage = damage;
        this.manaCost = manaCost;
    }
}


