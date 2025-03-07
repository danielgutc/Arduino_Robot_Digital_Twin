using UnityEngine;
using System.Collections;

public class ServoMotor : MonoBehaviour
{
    public Transform servoArm; // Assign the servo arm GameObject in Unity
    public float speed = 2f; // Degrees per frame
    private float angle = 90; // Start at middle position
    private int direction = 1;
    private I2CCommunication i2c;

    void Start()
    {
        i2c = FindFirstObjectByType<I2CCommunication>();
    }

    void Update()
    {
        // Simulate the servo oscillating between 0 and 180 degrees
        angle += speed * direction;
        if (angle >= 180)
        {
            angle = 180;
            direction = -1;
        }
        else if (angle <= 0)
        {
            angle = 0;
            direction = 1;
        }

        // Apply rotation to the servo arm (converting to a realistic servo rotation range)
        servoArm.localRotation = Quaternion.Euler(0, angle, 0);

        // Simulate sending the angle via I2C
        i2c.TransmitData(1, (int)angle);
    }
}
