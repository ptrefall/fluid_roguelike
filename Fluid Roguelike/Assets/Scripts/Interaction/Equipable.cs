using UnityEngine;

namespace Fluid.Roguelike.Interaction
{
    [CreateAssetMenu(fileName = "Equipable", menuName = "Content/Interactions/Equipable")]
    public class EquipableMeta : ScriptableObject, IInteractibleMeta
    {
        public IInteractible Create(Item.Item item)
        {
            var value = new Equipable();
            value.Setup(item, this);
            return value;
        }
    }

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
    }
}