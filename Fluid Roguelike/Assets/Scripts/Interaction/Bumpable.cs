using Fluid.Roguelike.Actions;
using UnityEngine;

namespace Fluid.Roguelike.Interaction
{
    [CreateAssetMenu(fileName = "Bumpable", menuName = "Content/Interactions/Bumpable")]
    public class BumpableMeta : ScriptableObject, IInteractibleMeta
    {
        public IInteractible Create(Item.Item item)
        {
            var value = new Bumpable();
            value.Setup(item, this);
            return value;
        }
    }

    public class Bumpable : IInteractible, IBumpTarget
    {
        private Item.Item _item;
        private BumpableMeta _meta;

        public void Setup(Item.Item item, IInteractibleMeta meta)
        {
            _item = item;
            _meta = (BumpableMeta) meta;
        }

        public bool TryInteract(Character.Character character)
        {
            return false;
        }
    }
}