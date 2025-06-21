using Servo;
using UnityEngine;

public class ArduinoControllerExtension : MonoBehaviour
{
    public IServo servo;
    public float SERVO_SPEED = 2f; // Degrees per frame
    public float SERVO_SPEED_MULT = 0.2f;
    
    private float speed;
    private float angle = 90; // Start at middle position
    private int direction = 1;
    private int minAngle = 0;
    private int maxAngle = 180;
    private I2CBus i2c;

    void Start()
    {
        i2c = FindFirstObjectByType<I2CBus>();
        i2c.RegisterDevice(2, ReceiveServoAngle, SendCurrentAngle);

        speed = SERVO_SPEED;
    }

    private void ReceiveServoAngle(int openAngle)
    {
        if (openAngle == 0)
        {
            speed = 0;
            minAngle = maxAngle = 90;
        }
        else
        {
            minAngle = 90 - openAngle / 2;
            maxAngle = 90 + openAngle / 2;

            if (openAngle > 65)
            {
                speed = SERVO_SPEED * SERVO_SPEED_MULT;
            }
            else
            {
                speed = SERVO_SPEED;
            }
        }
    }

    private int SendCurrentAngle()
    {
        return servo.Read();
    }

    void Update()
    {
        angle += speed * direction;
        if (angle >= maxAngle)
        {
            angle = maxAngle;
            direction = -1;
        }
        else if (angle <= minAngle)
        {
            angle = minAngle;
            direction = 1;
        }

        servo.Write((int)angle);
    }
}
