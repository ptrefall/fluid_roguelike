﻿using System;
using System.Collections.Generic;
using Fluid.Roguelike.AI;
using Fluid.Roguelike.Character.Sensory;
using Fluid.Roguelike.Character.Stats;
using UnityEngine;

namespace Fluid.Roguelike.Database
{
    [CreateAssetMenu(fileName = "Character Database", menuName = "Content/Character Database")]
    public class CharacterDatabase : ScriptableObject
    {
        [SerializeField] private string _race;
        [SerializeField] private List<CharacterDbEntry> _db = new List<CharacterDbEntry>();

        public string Race => _race;

        public bool Find(string name, out CharacterDbEntry character)
        {
            foreach (var entry in _db)
            {
                if (entry.Name == name)
                {
                    character = entry;
                    return true;
                }
            }

            character = null;
            return false;
        }
    }

    [Serializable]
    public class CharacterDbEntry
    {
        public string Name;
        public string DisplayName;
        public Sprite Sprite;
        public Sprite DeathSprite;
        public Color Color = UnityEngine.Color.white;
        public Color DeathColor = UnityEngine.Color.white;
        public CharacterDomainDefinition Brain;

        public List<SensorTypes> Sensors;
        public List<StatDbEntry> Stats = new List<StatDbEntry>
        {
            new StatDbEntry
            {
                Type = StatType.Health,
                Value = 1,
            },
            new StatDbEntry
            {
                Type = StatType.Sight,
                Value = 6,
            },
        };

        public List<ScriptableObject> Abilities;
        public List<string> Items;
        public List<LootDbEntry> LootItems;
        public List<LootDbEntry> AlwaysDropLootItems;
        public int MinLootDrops = 0;
        public int MaxLootDrops = 1;

        //TODO: Extend with more data later
    }

    [Serializable]
    public class StatDbEntry
    {
        public StatType Type;
        public int Value;
        public int StartValue = -1;
        public int RegenRate;
    }

    [Serializable]
    public class LootDbEntry
    {
        public string Item;
        public float DropChance = 1f;
        public bool CanDropMultipleTimes = false;
    }

    [CreateAssetMenu(fileName = "Character Database Manager", menuName = "Content/Character Database Manager")]
    public class CharacterDatabaseManager : ScriptableObject
    {
        [SerializeField] private List<CharacterDatabase> _dbs = new List<CharacterDatabase>();

        public bool Find(string race, string name, out CharacterDbEntry output)
        {
            foreach (var db in _dbs)
            {
                if (db.Race == race)
                {
                    return db.Find(name, out output);
                }
            }

            output = null;
            return false;
        }
    }
}