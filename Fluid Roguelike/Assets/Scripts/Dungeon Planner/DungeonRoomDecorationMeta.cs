using System;
using System.Collections.Generic;
using System.IO;
using Fluid.Roguelike.Database;
using Unity.Mathematics;
using UnityEngine;

namespace Fluid.Roguelike.Dungeon
{
    public class DungeonRoomDecorationMeta
    {
        public Dictionary<int2, Dungeon.MapValue> ValueMap { get; } = new Dictionary<int2, Dungeon.MapValue>();
        public Dictionary<int2, string> ItemMap { get; } = new Dictionary<int2, string>();
        public Dictionary<int2, Tuple<string, string>> CharacterMap { get; } = new Dictionary<int2, Tuple<string, string>>();

        /// <summary>
        /// Generate a value map from a double array.
        /// Dimension 0 is read as Column
        /// Dimension 1 is read as Row
        /// </summary>
        /// <param name="valueMap"></param>
        public void GenerateGround(DungeonTheme theme, int[,] valueMap)
        {
            for (var x = 0; x < valueMap.GetLength(0); x++)
            {
                for (var y = 0; y < valueMap.GetLength(1); y++)
                {
                    var key = new int2(x, y);
                    ValueMap.Add(key, new Dungeon.MapValue
                    {
                        Theme = theme,
                        Index = (DungeonTile.Index)valueMap[y, x],
                        IsSpecial = true,
                    });
                }
            }
        }

        public void GenerateGround(DungeonTheme theme, string textFilePath)
        {
            var textAsset = Resources.Load<TextAsset>(textFilePath);
            if (textAsset == null)
                return;

            GenerateGround(theme, textAsset);
        }

        public void Generate(DungeonTheme theme, DecorationInfo info)
        {
            if (info == null)
                return;

            if (info.Ground != null)
            {
                GenerateGround(theme, info.Ground);
            }

            if (info.Items != null)
            {
                WriteItems(info.Items);
            }

            if (info.Characters != null)
            {
                WriteCharacters(info.Characters);
            }
        }

        public void GenerateGround(DungeonTheme theme, TextAsset textAsset)
        {
            if (textAsset == null)
                return;

            using (var reader = new StringReader(textAsset.text))
            {
                int y = 0;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var x = 0;
                    var split = line.Split(',');
                    foreach (var tile in split)
                    {
                        var key = new int2(x, y);
                        WriteToGround(key, theme, tile);
                        x++;
                    }

                    y++;
                }
            }
        }

        private void WriteToGround(int2 key, DungeonTheme theme, string tile)
        {
            ValueMap.Add(key, new Dungeon.MapValue
            {
                Theme = theme,
                Index = (DungeonTile.Index)Convert.ToInt32(tile),
                IsSpecial = true,
            });
        }

        public void WriteItems(TextAsset textAsset)
        {
            if (textAsset == null)
                return;

            using (var reader = new StringReader(textAsset.text))
            {
                int lineNum = 0;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var split = line.Split(',');
                    if (split.Length < 3 || split.Length > 3)
                    {
                        Debug.LogError($"Error in syntax at line {lineNum} reading item from asset {textAsset.name}!");
                        continue;
                    }

                    var x = Convert.ToInt32(split[0]);
                    var y = Convert.ToInt32(split[1]);
                    var name = split[2];

                    var pos = new int2(x, y);

                    if (ItemMap.ContainsKey(pos))
                    {
                        Debug.LogError($"Error reading position at line {lineNum} reading item from asset {textAsset.name}. It already exist!");
                        continue;
                    }
                    ItemMap.Add(pos, name);

                    lineNum++;
                }
            }
        }

        public void WriteCharacters(TextAsset textAsset)
        {
            if (textAsset == null)
                return;

            using (var reader = new StringReader(textAsset.text))
            {
                int lineNum = 0;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var split = line.Split(',');
                    if (split.Length < 4 || split.Length > 4)
                    {
                        Debug.LogError($"Error in syntax at line {lineNum} reading character from asset {textAsset.name}!");
                        continue;
                    }

                    var x = Convert.ToInt32(split[0]);
                    var y = Convert.ToInt32(split[1]);
                    var race = split[2];
                    var name = split[2];

                    var pos = new int2(x, y);

                    if (CharacterMap.ContainsKey(pos))
                    {
                        Debug.LogError($"Error reading position at line {lineNum} reading character from asset {textAsset.name}. It already exist!");
                        continue;
                    }
                    CharacterMap.Add(pos, new Tuple<string, string>(race, name));

                    lineNum++;
                }
            }
        }
    }
}