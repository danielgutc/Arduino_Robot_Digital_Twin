using UnityEngine;

public class WheelDrive : MonoBehaviour
{
    [System.Serializable]
    public class AxleInfo
    {
        public WheelCollider leftWheel;
        public WheelCollider rightWheel;
        public Transform leftVisual;
        public Transform rightVisual;
        public bool motor; // if this axle is motorized
        public bool steering; // if this axle can steer
    }

    public float maxMotorTorque = 1500f; // Max torque per motor wheel
    public float maxSteeringAngle = 30f; // Max steering angle

    public AxleInfo[] axles;

    private void FixedUpdate()
    {
        float motor = maxMotorTorque ;
        float steering = maxSteeringAngle;

        foreach (AxleInfo axle in axles)
        {
            if (axle.steering)
            {
                axle.leftWheel.steerAngle = steering;
                axle.rightWheel.steerAngle = steering;
            }

            if (axle.motor)
            {
                axle.leftWheel.motorTorque = motor;
                axle.rightWheel.motorTorque = motor;
            }

            UpdateWheelVisual(axle.leftWheel, axle.leftVisual);
            UpdateWheelVisual(axle.rightWheel, axle.rightVisual);
        }
    }

    private void UpdateWheelVisual(WheelCollider collider, Transform visual)
    {
        if (visual == null)
            return;

        Vector3 pos;
        Quaternion rot;
        collider.GetWorldPose(out pos, out rot);
        visual.position = pos;
        //visual.rotation = rot;
    }
}
