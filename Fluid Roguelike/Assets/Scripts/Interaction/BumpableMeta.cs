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
}