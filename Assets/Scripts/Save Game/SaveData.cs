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

    // Map State - FIXED: Use MapManager.SerializableMapNode
    public List<MapManager.SerializableMapNode> mapNodes = new List<MapManager.SerializableMapNode>();
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

// REMOVED the duplicate SerializableMapNode class from here
// We're using the one from MapManager.cs instead