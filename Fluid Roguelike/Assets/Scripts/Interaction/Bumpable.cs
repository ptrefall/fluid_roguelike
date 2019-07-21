using Fluid.Roguelike.Actions;
using UnityEngine;

namespace Fluid.Roguelike.Interaction
{
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

        public bool TryApply(string key, string value)
        {
            return false;
        }
    }
}