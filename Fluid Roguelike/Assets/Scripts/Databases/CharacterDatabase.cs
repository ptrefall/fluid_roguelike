using System;
using System.Collections.Generic;
using Fluid.Roguelike.AI;
using UnityEngine;

namespace Fluid.Roguelike.Database
{
    [CreateAssetMenu(fileName = "Character Database", menuName = "Content/Character Database")]
    public class CharacterDatabase : ScriptableObject
    {
        [SerializeField] private string _race;
        [SerializeField] private List<CharacterDbEntry> _db = new List<CharacterDbEntry>();

        public string Race => _race;

        public Sprite Find(string name, out Color color, out CharacterDomainDefinition brain)
        {
            foreach (var entry in _db)
            {
                if (entry.Name == name)
                {
                    color = entry.Color;
                    brain = entry.Brain;
                    return entry.Sprite;
                }
            }

            color = Color.white;
            brain = null;
            return default(Sprite);
        }
    }

    [Serializable]
    public class CharacterDbEntry
    {
        public string Name;
        public Sprite Sprite;
        public Color Color = UnityEngine.Color.white;
        public CharacterDomainDefinition Brain;
        //TODO: Extend with more data later
    }

    [CreateAssetMenu(fileName = "Character Database Manager", menuName = "Content/Character Database Manager")]
    public class CharacterDatabaseManager : ScriptableObject
    {
        [SerializeField] private List<CharacterDatabase> _dbs = new List<CharacterDatabase>();

        public Sprite Find(string race, string name, out Color color, out CharacterDomainDefinition brain)
        {
            foreach (var db in _dbs)
            {
                if (db.Race == race)
                {
                    return db.Find(name, out color, out brain);
                }
            }

            color = Color.white;
            brain = null;
            return default(Sprite);
        }
    }
}