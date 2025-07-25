using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AutoSuspensionTuner : MonoBehaviour
{
    public WheelCollider[] wheelColliders;
    public float springCompressionFactor = 1.5f;  // 1.5 to 2.0 is typical
    public float damperMultiplier = 0.25f;        // damper = spring * multiplier
    public float suspensionDistance = 0.2f;       // standard suspension travel

    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (wheelColliders.Length == 0)
        {
            Debug.LogWarning("No wheel colliders assigned!");
            return;
        }

        int wheelCount = wheelColliders.Length;
        float totalWeight = rb.mass * Physics.gravity.magnitude; // N (newtons)
        float springForcePerWheel = (totalWeight / wheelCount) * springCompressionFactor;
        float damper = springForcePerWheel * damperMultiplier;

        JointSpring spring = new JointSpring
        {
            spring = springForcePerWheel,
            damper = damper,
            targetPosition = 0.5f
        };

        foreach (WheelCollider wc in wheelColliders)
        {
            wc.suspensionDistance = suspensionDistance;
            wc.suspensionSpring = spring;
        }

        Debug.Log($"[Suspension Tuner] Spring: {springForcePerWheel:F1}, Damper: {damper:F1}, Wheels: {wheelCount}, Mass: {rb.mass}");
    }
}
