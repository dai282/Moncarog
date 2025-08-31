using UnityEngine;
using Elementals;

public class SkillList
{
    public Skill[] skills = new Skill[50];

    public SkillList()
    { 
        //basic moves
        skills[0] = new Skill("Rest", ElementalType.Normal, 0.0f, -50);
        skills[1] = new Skill("Bite", ElementalType.Normal, 10.0f, 10);
        skills[2] = new Skill("Tackle", ElementalType.Normal, 30.0f, 20);

        //elemental moves
        skills[3] = new Skill("Ignite", ElementalType.Fire, 20.0f, 20);
        skills[4] = new Skill("Douse", ElementalType.Water, 20.0f, 20);
        skills[5] = new Skill("Throw seeds", ElementalType.Plant, 20.0f, 20);

        //ultimate moves
        skills[6] = new Skill("Drown", ElementalType.Water, 80.0f, 70);
        skills[7] = new Skill("Incinerate", ElementalType.Fire, 80.0f, 70);
        skills[8] = new Skill("Overgrow", ElementalType.Water, 80.0f, 70);

        skills[9] = new Skill("Beatdown", ElementalType.Normal, 90.0f, 70);



    }

}
