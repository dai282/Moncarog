using UnityEngine;
using Elementals;

[CreateAssetMenu(fileName = "New Moncarg Item", menuName = "Data/Moncarg Item")]
public class MoncargInventoryAdapter : ScriptableObject //MoncargItemDefinition
{
    [Header("Display Info")]
    public string FriendlyName;
    public Sprite Icon;

    //item definition holds data, which is also part of Moncarg component
    [Header("Moncarg Data")]
    public MoncargData moncargData;

    [Header("Inventory Settings")]
    public Dimensions SlotDimension = new Dimensions { Width = 1, Height = 1 };
    public bool IsEquipped = true;

    //prefab reference
    public GameObject linkedPrefab;

    // Convert to description string for inventory display
    public string Description
    {
        get
        {
            return $"Level {moncargData.level} {moncargData.type} type\n" +
                   $"Health: {moncargData.health}/{moncargData.maxHealth}\n" +
                   $"Mana: {moncargData.mana}/{moncargData.maxMana}\n" +
                   $"Attack: {moncargData.attack} | Defense: {moncargData.defense}\n" +
                   $"Speed: {moncargData.speed} | Experience: {moncargData.exp}";
        }
    }

    // Create a Moncarg GameObject from this data || THIS IS ALSO DONE IN StoredMoncarg Awake()
    public GameObject CreateMoncargGameObject()
    {
        // Instantiate the linked prefab
        GameObject moncargGO = Instantiate(linkedPrefab);
        Moncarg moncargComponent = moncargGO.GetComponent<Moncarg>();

        if (moncargComponent != null)
        {
            // Update the component with current data (in case stats changed)
            moncargComponent.moncargName = moncargData.moncargName;
            moncargComponent.maxHealth = moncargData.maxHealth;
            moncargComponent.health = moncargData.health;
            moncargComponent.attack = moncargData.attack;
            moncargComponent.defense = moncargData.defense;
            moncargComponent.speed = moncargData.speed;
            moncargComponent.exp = moncargData.exp;
            moncargComponent.level = moncargData.level;
            moncargComponent.mana = moncargData.mana;
            moncargComponent.maxMana = moncargData.maxMana;
            moncargComponent.catchChance = moncargData.catchChance;
            moncargComponent.dodgeChance = moncargData.dodgeChance;
            moncargComponent.type = moncargData.type;
            moncargComponent.active = moncargData.active;
            moncargComponent.role = Moncarg.moncargRole.PlayerOwned;
        }

        return moncargGO;
    }
}