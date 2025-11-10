using UnityEngine;

namespace MeUltrasonicSensor
{
    public class SimulatedMeUltrasonicSensor : MonoBehaviour, IMeUltrasonicSensor
    {
        public Transform sensorTransform;
        public float detectionRange = 30f;

        // TODO Return the minimum distance detected by the sensor in a cone in front of it
        public float GetDistanceCm()
        {
            RaycastHit hit;
            if (Physics.Raycast(sensorTransform.position, sensorTransform.forward, out hit, detectionRange))
            {
                return hit.distance;
            }
            return detectionRange;
        }
    }
}