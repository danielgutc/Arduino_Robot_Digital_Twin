using UnityEngine;

namespace Servo
{
    /*
     * PhysicalServo connects the physical servo motor but still represents the rotation of the servo in Unity.
     */
    public class PhysicalServo : MonoBehaviour, IServo
    {
        public RangerBle rangerBle;
        
        private readonly float minAngle = 0f;
        private readonly float maxAngle = 180f;
        private bool isAttached = false;

        private void Start()
        {
            if (rangerBle == null)
            {
                rangerBle = FindFirstObjectByType<RangerBle>();
            }

            if (this.transform != null)
            {
                isAttached = true;
            }
        }

        private void Update()
        {
            if (isAttached)
            {
                this.transform.localRotation = Quaternion.Euler(0, rangerBle.Telemetry.Angle, 0); // Rotates around the Y-axis
            }
        }

        public void Detach()
        {
            isAttached = false;
        }

        public void Write(int value)
        {
            // Not implemeted yet. Target is to send the angle to the physical servo via Bluetooth.
        }

        public void WriteMicroseconds(int value)
        {
            Write(value);
        }

        public int Read()
        {
            return rangerBle.Telemetry.Angle; // Read the angle from the RangerBle telemetry
        }

        public int ReadMicroseconds()
        {
            return Mathf.RoundToInt(Mathf.Lerp(544, 2400, (rangerBle.Telemetry.Angle - minAngle) / (maxAngle - minAngle)));
        }

        public bool Attached()
        {
            return isAttached;
        }
    }
}