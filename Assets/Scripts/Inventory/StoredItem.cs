using System;
using UnityEngine;
using Assets.Scripts.Inventory; // <-- Add this at the top

[Serializable]
public class StoredItem
{
    // Reference to the data for this item (name, description, icon, etc.)
    public ItemDefinition Details;

    // Reference to the UI element that displays this item in the inventory
    public ItemVisual RootVisual;
}
