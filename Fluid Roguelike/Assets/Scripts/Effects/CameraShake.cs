using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Events;

namespace Fluid.Roguelike.Effects
{
    public class CameraShake : MonoBehaviour
    {

        public float ShakeDuration = 0.3f; // Time the Camera Shake effect will last
        public float ShakeAmplitude = 1.2f; // Cinemachine Noise Profile Parameter
        public float ShakeFrequency = 2.0f; // Cinemachine Noise Profile Parameter

        private float ShakeElapsedTime = 0f;

        // Cinemachine Shake
        public CinemachineVirtualCamera VirtualCamera;
        private CinemachineBasicMultiChannelPerlin virtualCameraNoise;

        // Use this for initialization
        void Start()
        {
            // Get Virtual Camera Noise Profile
            if (VirtualCamera != null)
                virtualCameraNoise =
                    VirtualCamera.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>();
        }

        public void Shake()
        {
            ShakeElapsedTime = ShakeDuration;
        }

        public void Shake(float shakeDuration)
        {
            ShakeElapsedTime = shakeDuration;
        }

        public static CameraShake ShakeStatic()
        {
            var camera = Camera.main;
            if (camera != null)
            {
                var shaker = camera.GetComponent<CameraShake>();
                if (shaker != null)
                {
                    shaker.Shake();
                    return shaker;
                }
            }

            return null;
        }

        public static CameraShake ShakeStatic(float shakeDuration)
        {
            var camera = Camera.main;
            if (camera != null)
            {
                var shaker = camera.GetComponent<CameraShake>();
                if (shaker != null)
                {
                    shaker.Shake(shakeDuration);
                    return shaker;
                }
            }

            return null;
        }

        // Update is called once per frame
        void Update()
        {
            // If the Cinemachine componet is not set, avoid update
            if (VirtualCamera != null && virtualCameraNoise != null)
            {
                // If Camera Shake effect is still playing
                if (ShakeElapsedTime > 0)
                {
                    // Set Cinemachine Camera Noise parameters
                    virtualCameraNoise.m_AmplitudeGain = ShakeAmplitude;
                    virtualCameraNoise.m_FrequencyGain = ShakeFrequency;

                    // Update Shake Timer
                    ShakeElapsedTime -= Time.deltaTime;
                }
                else
                {
                    // If Camera Shake effect is over, reset variables
                    virtualCameraNoise.m_AmplitudeGain = 0f;
                    ShakeElapsedTime = 0f;
                }
            }
        }
    }
}