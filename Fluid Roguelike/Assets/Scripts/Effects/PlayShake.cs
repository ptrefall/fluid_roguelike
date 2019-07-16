using System.Collections;
using System.Collections.Generic;
using Fluid.Roguelike.UI;
using UnityEngine;

namespace Fluid.Roguelike.Effects
{
    public class PlayShake : MonoBehaviour, IUIStatusUpdater
    {
        [SerializeField] private float _shakeDuration = 0.3f;
        private CameraShake _shaker;

        private void Start()
        {
            _shaker = CameraShake.ShakeStatic(_shakeDuration);
        }

        public void Setup(int turns)
        {

        }

        public void Tick()
        {
            if (_shaker == null)
                Start();
            else
                _shaker.Shake(_shakeDuration);
        }
    }
}