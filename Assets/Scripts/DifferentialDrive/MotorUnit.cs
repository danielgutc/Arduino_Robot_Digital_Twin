using MeEncoderOnBoard;
using System.Linq;
using UnityEngine;

namespace DiferentialDrive
{
    public class MotorUnit: MonoBehaviour
    {
        [Header("Motor Unit Properties")]
        public float brakeTorque = 99999999f;
        public float torqueMultiplier = 10f;
        public float centreOfGravityOffset = -1f;

        public IMeEncoderOnBoard leftMotor;
        public IMeEncoderOnBoard rightMotor;

        public Wheel[] wheels;
        private Rigidbody rigidBody;

        // Start is called before the first frame update
        void Start()
        {
            rigidBody = GetComponent<Rigidbody>();

            // Adjust center of mass to improve stability and prevent rolling
            Vector3 centerOfMass = rigidBody.centerOfMass;
            centerOfMass.y += centreOfGravityOffset;
            rigidBody.centerOfMass = centerOfMass;
        }

        // FixedUpdate is called at a fixed time interval
        void FixedUpdate()
        {
            foreach (var wheel in wheels)
            {
                if (wheel.WheelCollider == null)
                {
                    Debug.LogWarning($"WheelCollider not found for wheel: {wheel.gameObject.name}");
                    continue;
                }

                if (leftMotor.GetCurrentSpeed() > 0  || rightMotor.GetCurrentSpeed() > 0)
                {
                    if (wheel.motorized)
                    {
                        float minWheelRpms = wheels.Where(w => w.gameObject.name.Contains("Right")).Min(w => Mathf.Abs(w.WheelCollider.rpm));

                        float maxSpeed = wheel.gameObject.name.Contains("Right") ? rightMotor.GetCurrentSpeed() : leftMotor.GetCurrentSpeed();
                        float speedFactor = Mathf.InverseLerp(0, Mathf.Min(Mathf.Abs(minWheelRpms), Mathf.Abs(maxSpeed)), Mathf.Abs(wheel.WheelCollider.rpm)); // Normalized speed factor
                        float currentMotorTorque = Mathf.Lerp(0 , Mathf.Abs(maxSpeed), speedFactor); 
                        currentMotorTorque = currentMotorTorque * (maxSpeed < 0 ? -1 : 1);

                        wheel.WheelCollider.motorTorque = currentMotorTorque * torqueMultiplier;
                    }
                    wheel.WheelCollider.brakeTorque = 0f;
                }
                else
                {
                    // Apply brakes when reversing direction
                    wheel.WheelCollider.motorTorque = 0f;
                    wheel.WheelCollider.brakeTorque = brakeTorque;
                }
            }
        }
    }
}