using UnityEngine;

public class MeUltrasonicSensor : MonoBehaviour
{
    public Transform sensorTransform;
    public float detectionRange = 100f;

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
