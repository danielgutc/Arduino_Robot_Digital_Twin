using NUnit.Framework;
using System;
using System.Linq;
using UnityEngine;

namespace Transmission
{
    public class TransmissionControl : MonoBehaviour
    {
        [Header("Car Properties")]
        public float motorTorque = 2000f;
        public float brakeTorque = 2000f;
        public float maxSpeed = 200f;
        public float steeringRange = 30f;
        public float steeringRangeAtMaxSpeed = 10f;
        public float centreOfGravityOffset = -1f;

        private WheelControl[] wheels;
        private Rigidbody rigidBody;

        private InputSystemActions carControls; // Reference to the new input system

        void Awake()
        {
            carControls = new InputSystemActions(); // Initialize Input Actions
        }
        void OnEnable()
        {
            carControls.Enable();
        }

        void OnDisable()
        {
            carControls.Disable();
        }

        // Start is called before the first frame update
        void Start()
        {
            rigidBody = GetComponent<Rigidbody>();

            // Adjust center of mass to improve stability and prevent rolling
            Vector3 centerOfMass = rigidBody.centerOfMass;
            centerOfMass.y += centreOfGravityOffset;
            rigidBody.centerOfMass = centerOfMass;

            // Get all wheel components attached to the car
            wheels = GetComponentsInChildren<WheelControl>();
        }

        // FixedUpdate is called at a fixed time interval
        void FixedUpdate()
        {
            // Read the Vector2 input from the new Input System
            Vector2 inputVector = carControls.Player.Move.ReadValue<Vector2>();

            // Get player input for acceleration and steering
            float vInput = inputVector.y; // Forward/backward input
            //float hInput = inputVector.x; // Steering input
            float minWheelRpms = wheels.Min(w => w.WheelCollider.rpm);

            foreach (var wheel in wheels)
            {
                if (motorTorque > 0)
                {
                    if (wheel.motorized)
                    {
                        float speedFactor = Mathf.InverseLerp(0, Math.Min(minWheelRpms, maxSpeed), Mathf.Abs(wheel.WheelCollider.rpm)); // Normalized speed factor
                        float currentMotorTorque = Mathf.Lerp(motorTorque, 0, speedFactor);
                        float currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, speedFactor);
                        if (currentMotorTorque < 1f)
                        {
                            Debug.Log("Low torque");
                        }

                        int direction = wheel.gameObject.name.Contains("Right")? 1 : -1;
                        wheel.WheelCollider.motorTorque = currentMotorTorque * direction;
                    }
                    wheel.WheelCollider.brakeTorque = 0f;
                }
                else
                {
                    // Apply brakes when reversing direction
                    wheel.WheelCollider.motorTorque = 0f;
                    wheel.WheelCollider.brakeTorque = Mathf.Abs(vInput) * brakeTorque;
                }
            }
        }
    }
}