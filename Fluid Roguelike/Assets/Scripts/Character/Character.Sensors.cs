
using System.Collections.Generic;
using Fluid.Roguelike.Actions;
using Fluid.Roguelike.Character.Sensory;
using Fluid.Roguelike.Character.State;
using FluidHTN;
using UnityEngine;

namespace Fluid.Roguelike.Character
{
    public partial class Character
    {
        private readonly List<ICharacterSensor> _sensors = new List<ICharacterSensor>();

        private ICharacterSensor Create(SensorTypes sensorType)
        {
            switch (sensorType)
            {
                case SensorTypes.Sight:
                    return new SightSensor();
                case SensorTypes.MeleeRange:
                    return new MeleeRangeSensor();
            }

            return null;
        }
        
        public void AddSensor(SensorTypes sensorType)
        {
            foreach (var s in _sensors)
            {
                if (s.Type == sensorType)
                    return;
            }

            var sensor = Create(sensorType);
            _sensors.Add(sensor);
        }

        public void RemoveSensor(SensorTypes sensorType)
        {
            foreach (var s in _sensors)
            {
                if (s.Type == sensorType)
                {
                    _sensors.Remove(s);
                    return;
                }
            }
        }

        public void TickTurn_Sensors()
        {
            foreach (var sensor in _sensors)
            {
                sensor.Tick(Context);
            }
        }
    }
}