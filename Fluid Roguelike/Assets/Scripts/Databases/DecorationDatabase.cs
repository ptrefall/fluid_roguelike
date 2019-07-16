using System;
using System.Collections.Generic;
using Fluid.Roguelike.Dungeon;
using UnityEngine;

namespace Fluid.Roguelike.Database
{
    [CreateAssetMenu(fileName = "Decoration Database", menuName = "Content/Decoration Database")]
    public class DecorationDatabase : ScriptableObject
    {
        [SerializeField] private List<DecorationEntry> _db = new List<DecorationEntry>();

        public DecorationInfo Find(DecorationType type)
        {
            foreach (var entry in _db)
            {
                if (entry.Type == type)
                {
                    var ground = GetRandom(ref entry.GroundAssets);
                    if (ground == null) ground = entry.DefaultGround;

                    var items = GetRandom(ref entry.ItemAssets);
                    if (items == null) items = entry.DefaultItem;

                    var characters = GetRandom(ref entry.CharacterAssets);
                    if (characters == null) characters = entry.DefaultCharacter;

                    return new DecorationInfo
                    {
                        Ground = ground,
                        Items = items,
                        Characters = characters,
                    };
                }
            }

            return null;
        }

        private TextAsset GetRandom(ref WeightedTextAsset[] assets)
        {
            var totalWeight = 0f;
            foreach (var option in assets)
            {
                totalWeight += option.Weight;
            }

            var targetWeight = UnityEngine.Random.Range(0, totalWeight);
            foreach (var option in assets)
            {
                totalWeight -= option.Weight;

                if (targetWeight >= totalWeight)
                    return option.Asset;
            }

            return null;
        }
    }

    public class DecorationInfo
    {
        public TextAsset Ground { get; set; }
        public TextAsset Items { get; set; }
        public TextAsset Characters { get; set; }
    }

    [Serializable]
    public class DecorationEntry
    {
        public DecorationType Type;
        public WeightedTextAsset[] GroundAssets;
        public WeightedTextAsset[] ItemAssets;
        public WeightedTextAsset[] CharacterAssets;

        public TextAsset DefaultGround => GroundAssets != null && GroundAssets.Length > 0 ? GroundAssets[0].Asset : null;
        public TextAsset DefaultItem => ItemAssets != null && ItemAssets.Length > 0 ? ItemAssets[0].Asset : null;
        public TextAsset DefaultCharacter => CharacterAssets != null && CharacterAssets.Length > 0 ? CharacterAssets[0].Asset : null;
    }

    [Serializable]
    public class WeightedTextAsset
    {
        public TextAsset Asset;
        public float Weight;
    }

    [CreateAssetMenu(fileName = "Decoration Database Manager", menuName = "Content/Decoration Database Manager")]
    public class DecorationDatabaseManager : ScriptableObject
    {
        [SerializeField] private List<DecorationDatabaseEntry> _dbs = new List<DecorationDatabaseEntry>();

        public DecorationInfo Find(DungeonTheme theme, DecorationType decorationType)
        {
            foreach (var entry in _dbs)
            {
                if (entry.Theme == theme)
                {
                    return entry.Db.Find(decorationType);
                }
            }

            return null;
        }
    }

    [Serializable]
    public class DecorationDatabaseEntry
    {
        public DungeonTheme Theme;
        public DecorationDatabase Db;
    }

    public enum DecorationType
    {
        Altar,
        River,
        Patch,
    }
}