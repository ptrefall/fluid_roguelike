using System;
using System.Collections.Generic;
using Fluid.Roguelike.Ability;
using Fluid.Roguelike.AI;
using Fluid.Roguelike.Character.Sensory;
using Fluid.Roguelike.Character.Stats;
using UnityEngine;

namespace Fluid.Roguelike.Database
{
    [CreateAssetMenu(fileName = "Item Database", menuName = "Content/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        [SerializeField] private List<ItemDbEntry> _db = new List<ItemDbEntry>();

        public bool Find(string name, out ItemDbEntry item)
        {
            foreach (var entry in _db)
            {
                if (entry.Name == name)
                {
                    item = entry;
                    return true;
                }
            }

            item = null;
            return false;
        }
    }

    [Serializable]
    public class ItemDbEntry
    {
        public string Name;
        public Item.ItemType Type;
        public Item.ItemRarity Rarity;
        public List<ScriptableObject> Abilities;
        public int DefaultAbilityIndex = 0;

        public Sprite Sprite;
        public Color Color = UnityEngine.Color.white;

        //TODO: Extend with more data later
    }

    [CreateAssetMenu(fileName = "Item Database Manager", menuName = "Content/Item Database Manager")]
    public class ItemDatabaseManager : ScriptableObject
    {
        [SerializeField] private List<ItemDatabase> _dbs = new List<ItemDatabase>();

        public bool Find(string name, out ItemDbEntry output)
        {
            foreach (var db in _dbs)
            {
                if (db.Find(name, out output))
                    return true;
            }

            output = null;
            return false;
        }
    }
}