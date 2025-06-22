using UnityEngine;

namespace MeEncoderOnBoard
{
    public class SimulatedMeEncoderOnBoard : MonoBehaviour, IMeEncoderOnBoard
    {
        public float speedMultiplier = 0.1f;
        private float currentSpeed = 0;
        private float targetPosition = 0;

        private void Update()
        {
            this.transform.Rotate(Vector3.left * currentSpeed);
        }

        public void SetMotorSpeed(int speed)
        {
            currentSpeed = speed * speedMultiplier;
        }

        public void StopMotor()
        {
            currentSpeed = 0;
        }

        public float GetCurrentSpeed()
        {
            return currentSpeed;
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