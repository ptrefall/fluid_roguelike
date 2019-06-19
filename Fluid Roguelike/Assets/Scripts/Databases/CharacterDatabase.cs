using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using Fluid.Roguelike.Dungeon;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Fluid.Roguelike.Database
{
    [CreateAssetMenu(fileName = "Character Database", menuName = "Content/Character Database")]
    public class CharacterDatabase : ScriptableObject
    {
        [SerializeField] private string _race;
        [SerializeField] private List<CharacterDbEntry> _db = new List<CharacterDbEntry>();

        public string Race => _race;

        public Sprite Find(string name, out Color color)
        {
            foreach (var entry in _db)
            {
                if (entry.name == name)
                {
                    color = entry.Color;
                    return entry.Sprite;
                }
            }

            color = Color.white;
            return default(Sprite);
        }
    }

    [Serializable]
    public class CharacterDbEntry
    {
        public string name;
        public Sprite Sprite;
        public Color Color = UnityEngine.Color.white;
        //TODO: Extend with more data later
    }

    [CreateAssetMenu(fileName = "Character Database Manager", menuName = "Content/Character Database Manager")]
    public class CharacterDatabaseManager : ScriptableObject
    {
        [SerializeField] private List<CharacterDatabase> _dbs = new List<CharacterDatabase>();

        public Sprite Find(string race, string name, out Color color)
        {
            foreach (var db in _dbs)
            {
                if (db.Race == race)
                {
                    return db.Find(name, out color);
                }
            }

            color = Color.white;
            return default(Sprite);
        }
    }
}