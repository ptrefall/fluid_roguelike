
using System.Collections.Generic;
using Fluid.Roguelike.Actions;
using Fluid.Roguelike.Character.Sensory;
using Fluid.Roguelike.Character.State;
using Fluid.Roguelike.Database;
using Fluid.Roguelike.Dungeon;
using Fluid.Roguelike.Item;
using FluidHTN;
using Unity.Mathematics;
using UnityEngine;

namespace Fluid.Roguelike.Character
{
    public partial class Character
    {
        private List<Item.Item> _inventory = new List<Item.Item>();
        public Item.Item PrimaryWeapon { get; private set; }

        public delegate void InventoryEvent(Item.Item item, Item.Item oldItem);

        public InventoryEvent OnPrimaryWeaponChanged { get; set; }

        public bool GiveItem(string itemName)
        {
            ItemDbEntry meta;
            if (this.Context.Dungeon.ItemDb.Find(itemName, out meta))
            {
                var item = new Item.Item();
                item.Setup(Context.Dungeon, meta, spawnInWorld:false);
                _inventory.Add(item);
                Debug.Log($"{Context.Self.name} was given a {itemName}");

                if (PrimaryWeapon == null && item.Meta.Type == ItemType.Weapon)
                {
                    PrimaryWeapon = item;
                    OnPrimaryWeaponChanged?.Invoke(item, null);
                }
                return true;
            }

            return false;
        }

        public List<Item.Item> SpawnLoot()
        {
            List<Item.Item> loot = new List<Item.Item>();
            foreach (var itemName in Meta.LootItems)
            {
                ItemDbEntry meta;
                if (this.Context.Dungeon.ItemDb.Find(itemName, out meta))
                {
                    var item = new Item.Item();
                    item.Setup(Context.Dungeon, meta, spawnInWorld: false);
                    loot.Add(item);
                    Debug.Log($"{Context.Self.name} drops a {itemName}");
                }
            }

            return loot;
        }
    }
}