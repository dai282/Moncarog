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

        Debug.Log($"InitStats for {moncargName}, skillList.skills length = {skillList.skills?.Length}");

        skillset = new Skill[4];
        skillset[0] = skillList.skills[0];
        skillset[1] = skillList.skills[1];
        skillset[2] = skillList.skills[4];
        skillset[3] = skillList.skills[7];

        //mock enemy for testing combat system
        //TestMockEncounter();

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

    //does not need Update() since combathandler will handle actions

}