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

        public TextAsset Find(DecorationType type)
        {
            foreach (var entry in _db)
            {
                if (entry.Type == type)
                {
                    var totalWeight = 0f;
                    foreach (var option in entry.Assets)
                    {
                        totalWeight += option.Weight;
                    }

                    var targetWeight = UnityEngine.Random.Range(0, totalWeight);
                    foreach (var option in entry.Assets)
                    {
                        totalWeight -= option.Weight;

                        if (targetWeight >= totalWeight)
                            return option.Asset;
                    }

                    return entry.Default;
                }
            }

            return null;
        }
    }

    [Serializable]
    public class DecorationEntry
    {
        public DecorationType Type;
        public WeightedTextAsset[] Assets;

        public TextAsset Default => Assets != null && Assets.Length > 0 ? Assets[0].Asset : null;
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

        public TextAsset Find(DungeonTheme theme, DecorationType decorationType)
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