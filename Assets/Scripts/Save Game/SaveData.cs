// SaveData.cs

using System;
using System.Collections.Generic;
using UnityEngine;

// Main container for the current run save file
[Serializable]
public class RunData
{
    // Player State
    public Vector2 playerPosition;
    public int currentRoomId;

    // Inventory State
    public List<SavedStoredItem> items = new List<SavedStoredItem>();
    public List<SavedStoredMoncarg> moncargs = new List<SavedStoredMoncarg>();

    // Map State
    public List<SerializableMapNode> mapNodes = new List<SerializableMapNode>();
    public List<int> traversalPath = new List<int>(); // The sequence of exit choices (0 or 1)
}

// Serializable version of StoredItem
[Serializable]
public class SavedStoredItem
{
    public string itemDefinitionId; // e.g., the name of the ScriptableObject
    // Add quantity, durability, etc. here if needed
}

// Serializable version of StoredMoncargData
[Serializable]
public class SavedStoredMoncarg
{
    public string moncargAdapterId; // e.g., the name of the ScriptableObject
    public float currentHealth;
    public float currentMana;
    public bool isEquipped;
}

// Serializable version of MapGenerator.MapNode
[Serializable]
public class SerializableMapNode
{
    public int roomId;
    public int roomType; // Storing the enum as an int
    public Vector2 position;
    public List<int> exitRoomIds = new List<int>();
}

// The LifetimeStats file is just the GameStats class, which is already serializable.
// We don't need a new class for it.