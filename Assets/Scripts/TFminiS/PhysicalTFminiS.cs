using System;
using UnityEngine;

namespace TFminiS
{
    public class PhysicalTFminiS : MonoBehaviour, ITFminiS
    {
        public RangerBle rangerBle;
        public float DIST_MULT = 0.1f; //Twin scale 10:1, so 1 meter in Unity is 10 centimeters in real world
        private int distance;
        private int strength;
        private int temperature;

        void Start()
        {
            if (rangerBle == null)
            {
                rangerBle = FindFirstObjectByType<RangerBle>();
            }
            
            strength = 0; // Not possible to read strength from the physical sensor
            temperature = 0; // Not possible to read temperature from the physical sensor
        }

        void Update()
        {
            ReadSensor();
        }

        public int GetDistance()
        {
            return distance;
        }

        public int GetStrength()
        {
            return strength;
        }

        public int GetTemperature()
        {
            return temperature; 
        }

        public void ReadSensor()
        {
            distance = (int)((float)rangerBle.Telemetry.Lidar * DIST_MULT);
        }


    }
}