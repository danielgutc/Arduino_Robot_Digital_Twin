using UnityEngine;

namespace TFminiS
{
    public class SimulatedTFminiS : MonoBehaviour, ITFminiS
    {
        public Transform sensorTransform;
        public int detectionRange = 40;
        private int distance;
        private int strength;
        private int temperature;

        void Update()
        {
            ReadSensor();
        }

        public void ReadSensor()
        {
            RaycastHit hit;
            if (Physics.Raycast(sensorTransform.position, sensorTransform.forward, out hit, detectionRange))
            {
                distance = (int)hit.distance;
                strength = Mathf.Clamp(1000 - (int)(hit.distance * 50), 0, 1000); // Simulating signal strength
            }
            else
            {
                distance = detectionRange;
                strength = 0;
            }
            temperature = 25 + Random.Range(-2, 2); // Simulating sensor temperature variation
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

        public static string GetErrorString(int errorCode)
        {
            switch (errorCode)
            {
                case 1: return "Signal too weak";
                case 2: return "Out of range";
                case 3: return "Hardware failure";
                default: return "Unknown error";
            }
        }
    }
}