using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using Fluid.Roguelike.Database;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Fluid.Roguelike.Interaction
{
    [CreateAssetMenu(fileName = "Container", menuName = "Content/Interactions/Container")]
    public class ContainerMeta : ScriptableObject, IInteractibleMeta
    {
        public ItemDatabaseManager ItemDb;
        public List<LootDbEntry> LootItems;
        public List<LootDbEntry> AlwaysDropLootItems;
        public int MinLootDrops = 0;
        public int MaxLootDrops = 1;

        public IInteractible Create(Item.Item item)
        {
            var value = new Container();
            value.Setup(item, this);
            return value;
        }
    }
}