using System;
using UnityEngine;

[CreateAssetMenu(fileName ="New Item", menuName ="Data/Item")]
public class ItemDefinition : ScriptableObject
{
    public string ID = Guid.NewGuid().ToString();
    public string FriendlyName;
    public string Description;
    public string affectedStat;
    public int effectValue;
    public Sprite Icon;
    public Dimensions SlotDimension;

    [Header("Powerup Properties")]
    public bool isPowerup = false;
    [Header("Stat Multipliers (1.0 = no change, 1.5 = +50%)")]
    public float healthMultiplier = 1.0f;
    public float attackMultiplier = 1.0f;
    public float defenseMultiplier = 1.0f;
    public float speedMultiplier = 1.0f;
    public float manaMultiplier = 1.0f;
}

[Serializable]
public struct Dimensions
{
    public int Height;
    public int Width;
}