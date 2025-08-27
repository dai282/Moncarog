using UnityEngine;

public class Skill
{
    public string name;
    public string elementalType;
    public float damage;
    public int manaCost;

    //Constructor
    public Skill(string name, string elementalType, float damage, int manaCost)
    {
        this.name = name;
        this.elementalType = elementalType;
        this.damage = damage;
        this.manaCost = manaCost;

    }
}
