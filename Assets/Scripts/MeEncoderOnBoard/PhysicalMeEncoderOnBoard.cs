using Ble;
using UnityEngine;

namespace MeEncoderOnBoard
{
    public class PhysicalMeEncoderOnBoard : MonoBehaviour, IMeEncoderOnBoard
    {
        public RangerBle rangerBle;
        public float speedMultiplier = 0.1f;
        private float currentSpeed = 0;
        private float targetPosition = 0;

        private void Start()
        {
            if (rangerBle == null)
            {
                rangerBle = FindFirstObjectByType<RangerBle>();
            }
        }

        private void Update()
        {
            this.transform.Rotate(Vector3.left * currentSpeed);
        }

        public void SetCurrentSpeed(float speed)
        {
            // Not implemented yet. Target is to send the speed to the physical motor via Bluetooth.
        }

        public void StopMotor()
        {
            // Not implemented yet. Target is to send the speed to the physical motor via Bluetooth.
        }

        public float GetCurrentSpeed()
        {
            if (this.transform.gameObject.name.ToLower().Contains("left"))
            {
                return rangerBle.Telemetry.LeftMotorSpeed;
            }
            else
            {
                return rangerBle.Telemetry.RightMotorSpeed;
            }
        }

        public void SetPosition(float position)
        {
            targetPosition = position;
        }

        public float GetPosition()
        {
            return this.transform.localEulerAngles.z;
        }
    }
}