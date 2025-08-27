using UnityEngine;

public class SkillList
{
    public Skill[] skills = new Skill[50];

    public SkillList()
    {
        skills[0] = new Skill("Bite", "Basic", 10.0f, 10);
        skills[1] = new Skill("Tackle", "Basic", 30.0f, 20);
        skills[2] = new Skill("Ignite", "Fire", 20.0f, 20);
        skills[3] = new Skill("Douse", "Water", 20.0f, 20);
        skills[4] = new Skill("Throw seeds", "Plant", 20.0f, 20);
    }

}
