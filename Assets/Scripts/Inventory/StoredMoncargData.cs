using UnityEngine;
using System;
using UnityEngine.UIElements;

[System.Serializable]
public class StoredMoncargData //MoncargInventoryItem
{
    public MoncargInventoryAdapter Details;
    public VisualElement RootVisual;

    // You can add any additional inventory-specific data here
    public bool IsEquipped = true;
}
