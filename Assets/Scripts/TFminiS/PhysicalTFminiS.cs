using System;
using UnityEngine;

namespace TFminiS
{
    public class PhysicalTFminiS : MonoBehaviour, ITFminiS
    {
        public RangerBle rangerBle;
        private int distance;
        private int strength;
        private int temperature;

        void Start()
        {
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
            distance = Convert.ToInt32(rangerBle.Telemetry.lidar);
        }


    }
}