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
}