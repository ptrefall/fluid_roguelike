
using System.Collections.Generic;
using Fluid.Roguelike.Actions;
using Fluid.Roguelike.Character.Sensory;
using Fluid.Roguelike.Character.State;
using Fluid.Roguelike.Character.Stats;
using FluidHTN;
using UnityEngine;

namespace Fluid.Roguelike.Character
{
    public partial class Character
    {
        private List<Stat> _stats = new List<Stat>();

        public int Health
        {
            get => GetStat(StatType.Health)?.Value ?? 0;
            set => SetValue(StatType.Health, value);
        }

        public int MaxHealth
        {
            get => GetStat(StatType.Health)?.MaxValue ?? 0;
            set => SetMaxValue(StatType.Health, value);
        }

        public int Sight
        {
            get => GetStat(StatType.Sight)?.Value ?? 0;
            set => SetValue(StatType.Sight, value);
        }

        public void AddStat(StatType type, int value)
        {
            foreach (var stat in _stats)
            {
                if (stat.Type == type)
                {
                    return;
                }
            }

            var s = new Stat(type, value);
            s.OnValueChanged += OnStatChanged;
            _stats.Add(s);
        }

        public bool SetValue(StatType type, int value)
        {
            foreach (var stat in _stats)
            {
                if (stat.Type == type)
                {
                    stat.Value = value;
                    return true;
                }
            }

            return false;
        }

        public bool SetMaxValue(StatType type, int maxValue)
        {
            foreach (var stat in _stats)
            {
                if (stat.Type == type)
                {
                    stat.MaxValue = maxValue;
                    return true;
                }
            }

            return false;
        }

        public Stat GetStat(StatType type)
        {
            foreach (var stat in _stats)
            {
                if (stat.Type == type)
                {
                    return stat;
                }
            }

            return null;
        }

        private void OnStatChanged(Stat stat, int oldValue)
        {
            switch (stat.Type)
            {
                case StatType.Health:
                    if (stat.Value <= 0)
                    {
                        Die();
                    }
                    break;
            }
        }
    }
}