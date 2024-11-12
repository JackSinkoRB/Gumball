using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gumball
{
    public static class SensorManager
    {

        private static LinearAccelerationSensor linearAccelerationSensorCached;
        private static Accelerometer accelerometerCached;

        private static readonly HashSet<object> listeners = new();
        
        public static LinearAccelerationSensor LinearAccelerationSensor => linearAccelerationSensorCached ??= LinearAccelerationSensor.current;
        public static Accelerometer Accelerometer => accelerometerCached ??= Accelerometer.current;

        public static Vector3 LinearAcceleration => LinearAccelerationSensor.acceleration.ReadValue();
        public static Vector3 Acceleration => Accelerometer.acceleration.value;
        public static Vector3 Gravity => Acceleration - LinearAcceleration;

        public static void AddListener(object context)
        {
            if (listeners.Contains(context))
            {
                Debug.LogWarning($"Could not add listener to SensorManager as it is already added.");
                return;
            }

            if (listeners.Count == 0)
                EnableSensors();

            listeners.Add(context);
        }

        public static void RemoveListener(object context)
        {
            if (!listeners.Contains(context))
            {
                Debug.LogWarning($"Could not remove listener to SensorManager as it is not added.");
                return;
            }

            listeners.Remove(context);
        
            if (listeners.Count == 0)
                DisableSensors();
        }
        
        private static void EnableSensors()
        {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        InputSystem.EnableDevice(LinearAccelerationSensor);
        InputSystem.EnableDevice(Accelerometer);
#endif
        }

        private static void DisableSensors()
        {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        InputSystem.DisableDevice(LinearAccelerationSensor);
        InputSystem.DisableDevice(Accelerometer);
#endif
        }

    }
}
