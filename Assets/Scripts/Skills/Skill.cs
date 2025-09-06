using UnityEngine;
using Elementals;

public class Skill
{

    public string name;
    public ElementalType type;
    public float damage;
    public int manaCost;

    //Constructor
    public Skill(string name, ElementalType type, float damage, int manaCost)
    {
        this.name = name;
        this.type = type;
        this.damage = damage;
        this.manaCost = manaCost;
    }
}


