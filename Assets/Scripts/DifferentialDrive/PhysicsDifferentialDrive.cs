using DifferentialDrive;
using MeEncoderOnBoard;
using System.Linq;
using UnityEngine;

namespace DiferentialDrive
{
    public class PhysicsDifferentialDrive: MonoBehaviour, IDifferentialDrive
    {
        [Header("Motor Unit Properties")]
        public float centreOfGravityOffset = -1f;
        public float maxRpm = 60f;             // absolute cap for requested RPM
        public float kp = 0.8f;                 // proportional gain: torque per rpm error (tune!)
        public float maxTorque = 500f;          // clamp for motorTorque
        public float maxBrake = 800f;           // clamp for brakeTorque
        public float rpmOvershootBand = 20f;    // how far above target before we start braking
        public float idleRpmBand = 5f;          // treat near-zero target as stop/hold
        public float freeSpinTorque = 5f;       // tiny torque when airborne to keep visuals sane
        public Wheel[] wheels;

        private Rigidbody rigidBody;
        private IMeEncoderOnBoard leftMotor;
        private IMeEncoderOnBoard rightMotor;

        public void SetElements(Rigidbody rangerBody, IMeEncoderOnBoard leftMotor, IMeEncoderOnBoard rightMotor)
        {
            this.rigidBody = rangerBody;
            this.leftMotor = leftMotor;
            this.rightMotor = rightMotor;

            // Adjust center of mass to improve stability and prevent rolling
            Vector3 centerOfMass = rigidBody.centerOfMass;
            centerOfMass.y += centreOfGravityOffset;
            rigidBody.centerOfMass = centerOfMass;

        }

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // FixedUpdate is called at a fixed time interval
        void FixedUpdate()
        {
            if (rigidBody == null || leftMotor == null || rightMotor == null)
            {
                return;
            }

            // 1) Compute per-side target RPMs (assuming your motors return desired RPM, can be +/-)
            float rightTarget = Mathf.Clamp(rightMotor.GetCurrentSpeed(), -maxRpm, maxRpm);
            float leftTarget = Mathf.Clamp(leftMotor.GetCurrentSpeed(), -maxRpm, maxRpm);

            // 2) Loop wheels
            foreach (var wheel in wheels)
            {
                var col = wheel.WheelCollider;
                if (!col)
                {
                    Debug.LogWarning($"WheelCollider not found for wheel: {wheel.gameObject.name}");
                    continue;
                }

                // Decide which side’s target this wheel should use.
                bool isRight = wheel.gameObject.name.IndexOf("Right", System.StringComparison.OrdinalIgnoreCase) >= 0;

                float targetRpm = isRight ? rightTarget : leftTarget;
                float currentRpm = col.rpm;

                // 3) If not motorized, just clear motor/brake and continue (you could still brake to hold)
                if (!wheel.motorized)
                {
                    col.motorTorque = 0f;
                    // Optional: hold brake when target is near zero
                    col.brakeTorque = (Mathf.Abs(targetRpm) <= idleRpmBand) ? maxBrake : 0f;
                    continue;
                }

                // 4) If wheel is airborne, don't fight the air: tiny torque, no brakes.
                if (!col.isGrounded)
                {
                    col.motorTorque = Mathf.Sign(targetRpm) * freeSpinTorque;
                    col.brakeTorque = 0f;
                    continue;
                }

                // 5) If target is near zero, hold still with brake; no motor torque.
                if (Mathf.Abs(targetRpm) <= idleRpmBand)
                {
                    col.motorTorque = 0f;
                    col.brakeTorque = maxBrake;
                    continue;
                }

                // 6) Compute error and proportional torque command
                float errorRpm = targetRpm - currentRpm;
                float torqueCmd = Mathf.Clamp(kp * errorRpm, -maxTorque, maxTorque);

                // 7) If we're overspeeding in the same direction as target, use brake instead of “negative torque”
                bool sameDir = Mathf.Sign(currentRpm) == Mathf.Sign(targetRpm);
                bool overspeed = sameDir && (Mathf.Abs(currentRpm) > Mathf.Abs(targetRpm) + rpmOvershootBand);

                if (overspeed)
                {
                    col.motorTorque = 0f;

                    // Scale brake with overshoot amount (gentle -> firm)
                    float excess = Mathf.Abs(currentRpm) - Mathf.Abs(targetRpm);
                    float brake = Mathf.Clamp01(excess / Mathf.Max(rpmOvershootBand, 1f)) * maxBrake;
                    col.brakeTorque = brake;
                }
                else
                {
                    // Drive toward target; release brakes
                    col.brakeTorque = 0f;
                    col.motorTorque = torqueCmd;
                }
            }
        }        
    }
}