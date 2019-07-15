using System.Collections;
using System.Collections.Generic;
using Fluid.Roguelike.UI;
using UnityEngine;

namespace Fluid.Roguelike.Effects
{
    public class PlayShake : MonoBehaviour, IUIStatusUpdater
    {
        private CameraShake _shaker;
        private void Start()
        {
            var camera = Camera.main;
            if (camera != null)
            {
                _shaker = camera.GetComponent<CameraShake>();
                if (_shaker != null)
                {
                    _shaker.Shake();
                }
            }
        }

        public void Setup(int turns)
        {

        }

        public void Tick()
        {
            if (_shaker == null)
                Start();
            else
                _shaker.Shake();
        }
    }
}