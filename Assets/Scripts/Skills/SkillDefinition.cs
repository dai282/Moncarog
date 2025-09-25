using UnityEngine;
using Elementals;

[CreateAssetMenu(fileName = "SkillDefinition", menuName = "Data/SkillDefinition")]
public class SkillDefinition : ScriptableObject
{
    public string skillName;
    public ElementalType type;
    public float damage;
    public int manaCost;
}
