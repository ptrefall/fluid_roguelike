
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

        public List<Item.Item> Inventory => _inventory;

        public int Scraps { get; set; }

        public void SetPrimaryWeapon(Item.Item item)
        {
            var oldItem = PrimaryWeapon;
            PrimaryWeapon = item;

            if (item == null)
            {
                Debug.Log($"{Context.Self.name} stops wielding a {oldItem.Meta.Name}");
            }
            else
            {
                Debug.Log($"{Context.Self.name} wields a {item.Meta.Name}");
            }

            OnPrimaryWeaponChanged?.Invoke(item, oldItem);
        }

        public bool GiveItem(string itemName)
        {
            if (_inventory.Count >= 10)
            {
                Debug.Log($"{Context.Self.name} failed to receive a {itemName}. Inventory is full.");
                return false;
            }

            ItemDbEntry meta;
            if (this.Context.Dungeon.ItemDb.Find(itemName, out meta))
            {
                var item = new Item.Item();
                item.Setup(Context.Dungeon, meta, spawnInWorld:false);
                _inventory.Add(item);
                Debug.Log($"{Context.Self.name} was given a {itemName}");

                if (PrimaryWeapon == null && item.Meta.Type == ItemType.Weapon)
                {
                    SetPrimaryWeapon(item);
                }
                return true;
            }

            return false;
        }

        public bool PickupItem(Item.Item item)
        {
            if (item.Meta.Type == ItemType.Scraps)
            {
                var value = Context.Dungeon.Destroy(item);
                Debug.Log($"{Context.Self.name} picked up {value} scraps.");
                Scraps += value;
                return true;
            }

            foreach (var i in _inventory)
            {
                if (i.Meta == item.Meta)
                {
                    var value = Context.Dungeon.Destroy(item);
                    Debug.Log($"{Context.Self.name} already has a {item.Meta.Name}. Turn it into {value} scraps!");
                    Scraps += value;
                    return true;
                }
            }

            if (_inventory.Count >= 10)
            {
                Debug.Log($"{Context.Self.name} failed to pick up a {item.Meta.Name}. Inventory is full.");
                return false;
            }

            Context.Dungeon.PickupItemFromWorld(item);
            _inventory.Add(item);
            Debug.Log($"{Context.Self.name} picked up a {item.Meta.Name}");

            if (PrimaryWeapon == null && item.Meta.Type == ItemType.Weapon)
            {
                SetPrimaryWeapon(item);
            }
            return true;
        }

        public void DropItem(Item.Item item)
        {
            if (PrimaryWeapon == item)
            {
                SetPrimaryWeapon(null);
            }

            _inventory.Remove(item);

            Context.Dungeon.DropItemIntoWorld(item, Position);
            Debug.Log($"{Context.Self.name} drops a {item.Meta.Name}");
        }

        public List<Item.Item> SpawnLoot()
        {
            List<Item.Item> loot = new List<Item.Item>();
            var numToSpawn = UnityEngine.Random.Range(Meta.MinLootDrops, Meta.MaxLootDrops + 1);

            if (Meta.AlwaysDropLootItems.Count > 0)
            {
                numToSpawn -= Meta.AlwaysDropLootItems.Count;
                if (Meta.AlwaysDropLootItems.Count >= Meta.MaxLootDrops && Meta.LootItems.Count > 0)
                {
                    Debug.LogError("Random loot will never drop. Always drop loot takes up all the slots");
                }

                foreach (var entry in Meta.AlwaysDropLootItems)
                {
                    SpawnLootEntry(entry, loot);
                }
            }

            if (numToSpawn <= 0)
                return loot;

            var lootPool = new List<LootDbEntry>(Meta.LootItems);
            for (var i = 0; i < numToSpawn; i++)
            {
                var totalWeight = 0f;
                foreach (var entry in lootPool)
                {
                    totalWeight += entry.DropChance;
                }

                var targetWeight = UnityEngine.Random.value * totalWeight;
                foreach (var entry in lootPool)
                {
                    targetWeight -= entry.DropChance;
                    if (targetWeight <= 0)
                    {
                        if (!entry.CanDropMultipleTimes)
                        {
                            lootPool.Remove(entry);
                        }

                        SpawnLootEntry(entry, loot);
                        break;
                    }
                }
            }

            return loot;
        }

        private void SpawnLootEntry(LootDbEntry entry, List<Item.Item> loot)
        {
            ItemDbEntry meta;
            if (this.Context.Dungeon.ItemDb.Find(entry.Item, out meta))
            {
                var item = new Item.Item();
                item.Setup(Context.Dungeon, meta, spawnInWorld: true);
                loot.Add(item);
                DropItem(item);
            }
        }
    }
}