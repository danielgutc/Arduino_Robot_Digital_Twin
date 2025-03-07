using UnityEngine;
using System.Collections;
using System;

public class ServoMotor : MonoBehaviour
{
    public Transform servoArm; // Assign the servo arm GameObject in Unity
    public float speed = 2f; // Degrees per frame
    private float angle = 90; // Start at middle position
    private int direction = 1;
    private int minAngle = 0;
    private int maxAngle = 180;
    private I2CCommunication i2c;

    void Start()
    {
        i2c = FindFirstObjectByType<I2CCommunication>();
        i2c.RegisterDevice(2, ReceiveServoAngle);
    }

    private void ReceiveServoAngle(int openAngle)
    {
        minAngle = 90 - openAngle / 2;
        maxAngle = 90 + openAngle / 2;
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
        servoArm.localRotation = Quaternion.Euler(0, angle, 0);

        // Simulate sending the angle via I2C
        i2c.TransmitData(1, (int)angle);
    }
}
