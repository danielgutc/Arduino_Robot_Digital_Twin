using Unity.VisualScripting;
using UnityEngine;

namespace TFminiS
{
    public class ProxyTFminiS : MonoBehaviour, ITFminiS
    {
        public string type = "simulated";

        private ITFminiS tFminiS;
    
    public int GetDistance()
        {
            throw new System.NotImplementedException();
        }

        public int GetStrength()
        {
            throw new System.NotImplementedException();
        }

        public int GetTemperature()
        {
            throw new System.NotImplementedException();
        }

        public void ReadSensor()
        {
            throw new System.NotImplementedException();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (type == "simulated")
            {
                tFminiS = this.AddComponent<SimulatedTFminiS>();
            }
            else if (type == "physical")
            {
                tFminiS = this.AddComponent<PhysicalTFminiS>();
            }
            else
            {
                Debug.LogError("Unknown TFminiS type: " + type);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}