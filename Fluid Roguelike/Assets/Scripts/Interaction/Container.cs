using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using Fluid.Roguelike.Database;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Fluid.Roguelike.Interaction
{
    public class Container : IInteractible
    {
        private Item.Item _item;
        private ContainerMeta _meta;
        private string _keyName;

        public void Setup(Item.Item item, IInteractibleMeta meta)
        {
            _item = item;
            _meta = (ContainerMeta) meta;
        }

        public bool TryInteract(Character.Character character)
        {
            if (string.IsNullOrEmpty(_keyName) == false)
            {
                var key = character.FindItem(_keyName);
                if (key == null)
                {
                    Debug.Log($"{character.name} does not have a matching key to open {_item.Meta.DisplayName}!");
                    return false;
                }

                character.Context.Dungeon.Destroy(key);
            }

            var loot = SpawnLoot(character);
            character.Context.Dungeon.Destroy(_item);
            return true;
        }

        public bool TryApply(string key, string value)
        {
            if (key == "require")
            {
                _keyName = value;
                return true;
            }

            return false;
        }

        private List<Item.Item> SpawnLoot(Character.Character character)
        {
            List<Item.Item> loot = new List<Item.Item>();
            var numToSpawn = Random.Range(_meta.MinLootDrops, _meta.MaxLootDrops + 1);
            if (_meta.AlwaysDropLootItems.Count > 0)
            {
                numToSpawn -= _meta.AlwaysDropLootItems.Count;
                if (_meta.AlwaysDropLootItems.Count >= _meta.MaxLootDrops && _meta.LootItems.Count > 0)
                {
                    Debug.LogError("Random loot will never drop. Always drop loot takes up all the slots");
                }

                foreach (var entry in _meta.AlwaysDropLootItems)
                {
                    SpawnLootEntry(character, entry, loot);
                }
            }

            if (numToSpawn <= 0)
                return loot;

            var lootPool = new List<LootDbEntry>(_meta.LootItems);
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

                        SpawnLootEntry(character, entry, loot);
                        break;
                    }
                }
            }

            return loot;
        }

        private void SpawnLootEntry(Character.Character character, LootDbEntry entry, List<Item.Item> loot)
        {
            ItemDbEntry meta;
            if (_meta.ItemDb.Find(entry.Item, out meta))
            {
                var item = new Item.Item();
                item.Setup(character.Context.Dungeon, meta, spawnInWorld: true);
                loot.Add(item);

                character.Context.Dungeon.DropItemIntoWorld(item, _item.WorldPosition);
                Debug.Log($"{_item.Meta.Name} drops a {item.Meta.Name}");
            }
        }
    }
}