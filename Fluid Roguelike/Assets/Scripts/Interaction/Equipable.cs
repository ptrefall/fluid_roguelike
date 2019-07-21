using UnityEngine;

namespace Fluid.Roguelike.Interaction
{
    public class Equipable : IInteractible
    {
        private Item.Item _item;
        private EquipableMeta _meta;

        public void Setup(Item.Item item, IInteractibleMeta meta)
        {
            _item = item;
            _meta = (EquipableMeta) meta;
        }

        public bool TryInteract(Character.Character character)
        {
            return character.PickupItem(_item);
        }

        public bool TryApply(string key, string value)
        {
            return false;
        }
    }
}