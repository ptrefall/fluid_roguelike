using System;
using System.Collections.Generic;
using Fluid.Roguelike.AI;
using Fluid.Roguelike.Character.Sensory;
using Fluid.Roguelike.Character.Stats;
using UnityEngine;

namespace Fluid.Roguelike.Database
{
    [CreateAssetMenu(fileName = "UI Database", menuName = "Content/UI Database")]
    public class UiDatabase : ScriptableObject
    {
        [SerializeField] private List<UiDbEntry> _db = new List<UiDbEntry>();

        public bool Find(StatType type, out UiDbEntry uiElement)
        {
            foreach (var entry in _db)
            {
                if (entry.Type == type)
                {
                    uiElement = entry;
                    return true;
                }
            }

            uiElement = null;
            return false;
        }
    }

    [Serializable]
    public class UiDbEntry
    {
        public StatType Type;
        public List<Sprite> Sprites;
        public UnityEngine.UI.Image Prefab;
    }
}