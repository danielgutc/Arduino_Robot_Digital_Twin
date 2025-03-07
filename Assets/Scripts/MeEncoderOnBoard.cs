using UnityEngine;

public class MeEncoderOnBoard : MonoBehaviour
{
    public Transform motorWheel;
    public float speedMultiplier = 0.1f;
    private float currentSpeed = 0;
    private float targetPosition = 0;

    public void SetMotorSpeed(int speed)
    {
        currentSpeed = speed * speedMultiplier;
        motorWheel.Rotate(Vector3.left * currentSpeed);
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
        return motorWheel.localEulerAngles.z;
    }
}
