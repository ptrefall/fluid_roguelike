using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using Fluid.Roguelike.Dungeon;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Fluid.Roguelike.Database
{
    [CreateAssetMenu(fileName = "Sprite Database", menuName = "Content/Sprite Database")]
    public class SpriteDatabase : ScriptableObject
    {
        [SerializeField] private List<SpriteDbEntry> _db = new List<SpriteDbEntry>();

        public Sprite Find(DungeonTile.Index index, bool isEdge, bool isBorder, out Color color)
        {
            foreach (var entry in _db)
            {
                if (entry.Index == index)
                {
                    color = entry.Color;

                    var options = (isEdge && entry.HasEdges) ? entry.EdgeSprites : ((isBorder && entry.HasBorders) ? entry.BorderSprites : entry.Sprites);
                    var totalWeight = 0f;
                    foreach (var option in options)
                    {
                        totalWeight += option.Weight;
                    }

                    var targetWeight = Random.Range(0, totalWeight);
                    foreach (var option in options)
                    {
                        totalWeight -= option.Weight;

                        if (targetWeight >= totalWeight)
                        {
                            return option.Sprite;
                        }
                    }

                    return entry.Default;
                }
            }

            color = Color.white;
            return default(Sprite);
        }
    }

    [Serializable]
    public class SpriteDbEntry
    {
        public DungeonTile.Index Index;
        public WeightedSprite[] Sprites;
        public WeightedSprite[] EdgeSprites;
        public WeightedSprite[] BorderSprites;
        public Color Color = UnityEngine.Color.white;
        public Sprite Default => Sprites != null && Sprites.Length > 0 ? Sprites[0].Sprite : default(Sprite);
        public bool HasEdges => EdgeSprites != null && EdgeSprites.Length > 0;
        public bool HasBorders => BorderSprites != null && BorderSprites.Length > 0;
    }

    [Serializable]
    public class WeightedSprite
    {
        public Sprite Sprite;
        public float Weight;
    }

    [CreateAssetMenu(fileName = "Sprite Database Manager", menuName = "Content/Sprite Database Manager")]
    public class SpriteDatabaseManager : ScriptableObject
    {
        [SerializeField] private List<SpriteDatabaseManagerEntry> _dbs = new List<SpriteDatabaseManagerEntry>();

        public Sprite Find(DungeonTheme theme, DungeonTile.Index index, bool isEdge, bool isBorder, out Color color)
        {
            foreach (var db in _dbs)
            {
                if (db.Theme == theme)
                {
                    return db.Database.Find(index, isEdge, isBorder, out color);
                }
            }

            color = Color.white;
            return default(Sprite);
        }
    }

    [Serializable]
    public class SpriteDatabaseManagerEntry
    {
        public DungeonTheme Theme;
        public SpriteDatabase Database;
    }
}