using System;
using System.Collections.Generic;
using Fluid.Roguelike.Actions;
using Fluid.Roguelike.AI;
using Fluid.Roguelike.Character.Sensory;
using Fluid.Roguelike.Character.Stats;
using Fluid.Roguelike.UI;
using UnityEngine;

namespace Fluid.Roguelike.Database
{
    [CreateAssetMenu(fileName = "UI Database", menuName = "Content/UI Database")]
    public class UiDatabase : ScriptableObject
    {
        [SerializeField] private List<UiDbEntry> _db = new List<UiDbEntry>();
        [SerializeField] private List<UiStatusDbEntry> _statusEffectDb = new List<UiStatusDbEntry>();
        [SerializeField] private List<UiLabelDbEntry> _labelDb = new List<UiLabelDbEntry>();
        public UnityEngine.UI.Image ItemUiPrefab;
        public UIKnownEnemyInfo KnownEnemyInfoPrefab;

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

        public bool Find(CharacterStatusType type, out UiStatusDbEntry effect)
        {
            foreach (var entry in _statusEffectDb)
            {
                if (entry.Type == type)
                {
                    effect = entry;
                    return true;
                }
            }

            effect = null;
            return false;
        }

        public bool Find(string character, out Sprite sprite)
        {
            foreach (var entry in _labelDb)
            {
                if (entry.Character == character)
                {
                    sprite = entry.Sprite;
                    return true;
                }
            }

            sprite = null;
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

    [Serializable]
    public class UiStatusDbEntry
    {
        public CharacterStatusType Type;
        public GameObject Effect;
        public bool NeedsRemoval;
    }

    [Serializable]
    public class UiLabelDbEntry
    {
        public string Character;
        public Sprite Sprite;
    }
}