using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Item", menuName = "Item")]
public class ItemDefinition : ScriptableObject
{
    [Header("Basic Info")]
    public string FriendlyName;
    public string Description;
    public Sprite Icon;
    public Dimensions SlotDimension;
    
    [Header("Item Type")]
    public bool isPowerup = false;
    public bool isConsumable = false; // Use once then delete
    
    [Header("Equipment Status")]
    public bool isEquipped = false;
    
    [Header("Powerup Multipliers (only if isPowerup = true)")]
    public float attackMultiplier = 1.0f;
    public float defenseMultiplier = 1.0f;
    public float healthMultiplier = 1.0f;
    public float speedMultiplier = 1.0f;
    public float manaMultiplier = 1.0f;
}

[Serializable]
public struct Dimensions
{
    public int Height;
    public int Width;
}