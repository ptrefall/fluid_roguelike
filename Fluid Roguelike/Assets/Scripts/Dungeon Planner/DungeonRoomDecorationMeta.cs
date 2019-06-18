using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Fluid.Roguelike.Dungeon
{
    public class DungeonRoomDecorationMeta
    {
        public Dictionary<Tuple<int, int>, int> ValueMap { get; } = new Dictionary<Tuple<int, int>, int>();

        /// <summary>
        /// Generate a value map from a double array.
        /// Dimension 0 is read as Column
        /// Dimension 1 is read as Row
        /// </summary>
        /// <param name="valueMap"></param>
        public void Generate(int[,] valueMap)
        {
            for (var x = 0; x < valueMap.GetLength(0); x++)
            {
                for (var y = 0; y < valueMap.GetLength(1); y++)
                {
                    var key = new Tuple<int, int>(x, y);
                    ValueMap.Add(key, valueMap[y,x]);
                }
            }
        }

        public void Generate(string textFilePath)
        {
            var textAsset = Resources.Load<TextAsset>(textFilePath);
            if (textAsset == null)
                return;

            Generate(textAsset);
        }

        public void Generate(TextAsset textAsset)
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
                        var key = new Tuple<int, int>(x, y);
                        ValueMap.Add(key, Convert.ToInt32(tile));
                        x++;
                    }

                    y++;
                }
            }
        }
    }
}