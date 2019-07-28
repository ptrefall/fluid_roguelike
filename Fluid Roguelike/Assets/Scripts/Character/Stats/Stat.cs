using System;
using UnityEngine;

namespace Fluid.Roguelike.Character.Stats
{
    [Serializable]
    public class Stat
    {
        private int _value;
        private int _maxValue;
        private int _regenRate;

        public StatType Type { get; private set; }

        public delegate void StatEvent(Stat stat, int oldValue);

        public StatEvent OnValueChanged { get; set; }
        public StatEvent OnMaxValueChanged { get; set; }

        public int Value
        {
            get => _value;
            set
            {
                var newValue = Mathf.Clamp(value, 0, _maxValue);
                if (_value != newValue)
                {
                    var oldValue = _value;
                    _value = newValue;
                    OnValueChanged?.Invoke(this, oldValue);
                }
            }
        }

        public int MaxValue
        {
            get => _maxValue;
            set
            {
                if (_maxValue != value)
                {
                    var oldValue = _maxValue;
                    _maxValue = value;
                    OnValueChanged?.Invoke(this, oldValue);
                }
            }
            
        }

        public int RegenRate => _regenRate;
        public int NextRegenCountdown { get; set; }

        public float Fraction => Value / (float) MaxValue;

        public Stat(StatType type, int value, int maxValue, int regenRate)
        {
            Type = type;
            _value = value;
            _maxValue = maxValue;
            _regenRate = regenRate;
            NextRegenCountdown = _regenRate;
        }
    }
}