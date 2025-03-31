using UnityEngine;

public class ArduinoControllerExtension : MonoBehaviour
{
    public ServoMotor servoMotor;
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
        i2c.RegisterDevice(2, ReceiveServoAngle);

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

    void Update()
    {
        // Simulate the servo oscillating between 0 and 180 degrees
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

        // Apply rotation to the servo arm (converting to a realistic servo rotation range)
        servoMotor.Write((int)angle);

        // Simulate sending the angle via I2C
        i2c.TransmitData(1, (int)angle * direction);
    }
}
