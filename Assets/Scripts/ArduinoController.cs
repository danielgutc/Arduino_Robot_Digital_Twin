using System;
using UnityEngine;

public class ArduinoController : MonoBehaviour
{
    private I2CCommunication i2c;
    public MeEncoderOnBoard leftMotor;
    public MeEncoderOnBoard rightMotor;
    public MeUltrasonicSensor ultrasonicSensor;
    public TFminiS lidarSensor;
    public DebugDisplay debugDisplay;

    private CrawlerDriveController crawlerDriveController;

    public int openAngle = 90;
    public int MIN_DISTANCE = 1000;
    public int WEIGHT_DISTANCE = 1;
    public int MAX_SPEED = 75;
    //private const float M_PI = 3.14159265358979323846f;

    private int speed;
    private int distanceLidar;
    private int distanceUltrasonic;
    private int state = 0; // 0 - stopped; 1 - forward; 2 - backguard; 3 - rotating right; 4 - rotating left
    private float angle;
    

    void Start()
    {
        crawlerDriveController = FindFirstObjectByType<CrawlerDriveController>();
        crawlerDriveController.SetMotors(leftMotor, rightMotor);
        debugDisplay = FindFirstObjectByType<DebugDisplay>();

        i2c = FindFirstObjectByType<I2CCommunication>();
        i2c.RegisterDevice(1, ReceiveServoAngle);
        i2c.TransmitData(2, (int)openAngle);
    }

    void Update()
    {
        // Read sensor data
        UpdateDistanceUltrasonic();
        UpdateDistanceLidar();

        /*// Control logic based on sensor inputs
        if (distanceUltrasonic < MIN_DISTANCE)
        {
            speed = 0;
            leftMotor.SetMotorSpeed(speed);
            rightMotor.SetMotorSpeed(speed);
            Debug.Log("Obstacle detected! Stopping motors.");
        }
        else if (distanceLidar < MIN_DISTANCE * 10)
        {
            speed = 50;
            leftMotor.SetMotorSpeed(-speed);
            rightMotor.SetMotorSpeed(speed);
            Debug.Log("Object close, moving slowly.");
        }
        else
        {
            speed = 100;
            leftMotor.SetMotorSpeed(-speed);
            rightMotor.SetMotorSpeed(speed);
            Debug.Log("Path clear, moving normally.");
        }*/

        Move();

        debugDisplay.UpdateDisplay($"Speed: {speed}, Lidar: {distanceLidar}, Ultrasonic: {distanceUltrasonic}, Angle: {angle}");
    }

    private void ReceiveServoAngle(int angle)
    {
        this.angle = angle;
        Debug.Log($"Arduino received servo angle: {angle}");
    }

    private void UpdateDistanceLidar()
    {
        lidarSensor.ReadSensor();

        int dist = lidarSensor.GetDistance();
        int strength = lidarSensor.GetStrength();
        int temperature = lidarSensor.GetTemperature();

        // Check for and handle any errors.
        if (dist >= 0)
        { 
            distanceLidar = dist;
        }
    }

    private void UpdateDistanceUltrasonic()
    {
        distanceUltrasonic = (int)ultrasonicSensor.GetDistanceCm();
    }

    private void Move(float leftSpeed, float rightSpeed)
    {
        leftMotor.SetMotorSpeed((int)-leftSpeed);
        rightMotor.SetMotorSpeed((int)rightSpeed);
    }

    void Move()
    {
        float leftSpeed = MAX_SPEED;
        float rightSpeed = MAX_SPEED;
        double speedModifier = 0;
        double angleRadians = 0;
        
        angleRadians = (Math.PI * (angle - 90)) / 180;
        speedModifier = 1 - (Math.Abs(Math.Sin(angleRadians)) * (1 - Math.Exp(-WEIGHT_DISTANCE * distanceLidar)));


        if (angle < 90 && distanceLidar < MIN_DISTANCE)
        {
            leftSpeed = MAX_SPEED * (float)speedModifier;
        }

        if (angle > 90 && distanceLidar < MIN_DISTANCE)
        {
            rightSpeed = MAX_SPEED * (float)speedModifier;
        }

        Debug.Log($" AngleRadians: {angleRadians}, SpeedModifier: {speedModifier}, LeftSpeed: {leftSpeed}, RightSpeed: {rightSpeed}");

        Move(leftSpeed, rightSpeed);
    }
}