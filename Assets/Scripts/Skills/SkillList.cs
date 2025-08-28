using UnityEngine;

public class SkillList
{
    public Skill[] skills = new Skill[50];

    public SkillList()
    { 
        //basic moves
        skills[0] = new Skill("Rest", "Basic", 0.0f, -50);
        skills[1] = new Skill("Bite", "Basic", 10.0f, 10);
        skills[2] = new Skill("Tackle", "Basic", 30.0f, 20);

        //elemental moves
        skills[3] = new Skill("Ignite", "Fire", 20.0f, 20);
        skills[4] = new Skill("Douse", "Water", 20.0f, 20);
        skills[5] = new Skill("Throw seeds", "Plant", 20.0f, 20);

        //ultimate moves
        skills[6] = new Skill("Drown", "Water", 80.0f, 70);
        skills[7] = new Skill("Incinerate", "Fire", 80.0f, 70);
        skills[8] = new Skill("Overgrow", "Water", 80.0f, 70);

        skills[9] = new Skill("Beatdown", "Basic", 90.0f, 70);



    }

}
