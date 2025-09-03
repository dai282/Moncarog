using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.Inventory
{
    /// <summary>
    /// Visual representation of an item in the inventory grid.
    /// </summary>
    public class ItemVisual : VisualElement
    {
        private readonly ItemDefinition m_Item;

        public ItemVisual(ItemDefinition item)
        {
            m_Item = item;

            name = $"{m_Item.FriendlyName}";

            // Set width and height based on slot dimensions
            style.height = m_Item.SlotDimension.Height * PlayerInventory.SlotDimension.Height;
            style.width = m_Item.SlotDimension.Width * PlayerInventory.SlotDimension.Width;

            style.visibility = Visibility.Hidden;

            // Create the icon for the item
            VisualElement icon = new VisualElement
            {
                style =
                {
                    backgroundImage = m_Item.Icon.texture,
                    backgroundSize = new BackgroundSize(BackgroundSizeType.Cover),
                    flexGrow = 1
                }
            };
            Add(icon);

            icon.AddToClassList("visual-icon");
            AddToClassList("visual-icon-container");
        }

        /// <summary>
        /// Set the position of this item within the inventory grid
        /// </summary>
        public void SetPosition(Vector2 pos)
        {
            style.left = pos.x;
            style.top = pos.y;
        }
    }
}
