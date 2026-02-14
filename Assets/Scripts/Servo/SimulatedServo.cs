using UnityEngine;

namespace Servo
{
    public class SimulatedServo : MonoBehaviour, IServo
    {
        public float minAngle = 0f;
        public float maxAngle = 180f;
        public float rotationSpeed = 0.1f; // Maximum degrees per second the servo can rotate

        private float currentAngle;
        private float targetAngle;
        private bool isAttached = false;

        private void Start()
        {
            if (this.transform != null)
            {
                isAttached = true;
            }
        }

        private void FixedUpdate()
        {
            if (isAttached)
            {
                // Rotate smoothly towards the target angle within the rotation speed limit
                float step = rotationSpeed;
                currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, step);
                this.transform.localRotation = Quaternion.Euler(0, currentAngle, 0); // Rotates around the Y-axis
            }
        }

        public void Detach()
        {
            isAttached = false;
        }

        public void Write(int value)
        {
            if (!isAttached) return;

            if (value < 200) // Angle mode
            {
                targetAngle = Mathf.Clamp(value, minAngle, maxAngle);
            }
            else // Microseconds mode (not directly applicable in Unity, map to degrees)
            {
                targetAngle = Mathf.Clamp(
                    Mathf.InverseLerp(544, 2400, value) * (maxAngle - minAngle) + minAngle,
                    minAngle, maxAngle);
            }
        }

        public void WriteMicroseconds(int value)
        {
            Write(value);
        }

        public int Read()
        {
            return Mathf.RoundToInt(targetAngle); // Returns the target angle, not the actual angle
        }

        public int ReadMicroseconds()
        {
            return Mathf.RoundToInt(Mathf.Lerp(544, 2400, (targetAngle - minAngle) / (maxAngle - minAngle)));
        }

        public bool Attached()
        {
            return isAttached;
        }
    }
}