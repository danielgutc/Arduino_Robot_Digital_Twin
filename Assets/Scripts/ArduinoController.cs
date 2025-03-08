using System;
using Unity.VisualScripting;
using UnityEngine;

public class ArduinoController : MonoBehaviour
{
    private I2CBus i2c;
    public MeEncoderOnBoard leftMotor;
    public MeEncoderOnBoard rightMotor;
    public MeUltrasonicSensor ultrasonicSensor;
    public TFminiS lidarSensor;
    public DebugDisplay debugDisplay;

    private CrawlerDriveController crawlerDriveController;

    public int maxScanAngle = 45;
    public int MIN_DISTANCE = 1000;
    public int MAX_SPEED = 100; 
    
    private int distanceLidar;
    private int distanceUltrasonic;
    private int state = 0; // 0 - stopped; 1 - forward; 2 - backguard; 3 - rotating left; 4 - rotating right
    private int angle;
    private int currentScanDirection = 1; // Left = -1, Right = 1
    private int currentScanAngle;
    private bool currentScanObstacleDetected = false;
    private bool obstacleDetected = false;

    void Start()
    {
        crawlerDriveController = FindFirstObjectByType<CrawlerDriveController>();
        crawlerDriveController.SetMotors(leftMotor, rightMotor);
        debugDisplay = FindFirstObjectByType<DebugDisplay>();
        i2c = FindFirstObjectByType<I2CBus>();
        i2c.RegisterDevice(1, ReceiveServoAngle);

        currentScanAngle = maxScanAngle;
        SendServoAngle(currentScanAngle);
    }

    void Update()
    {
        UpdateDistanceUltrasonic();
        UpdateDistanceLidar();
        ObstacleDetection();
        Move();

        debugDisplay.UpdateDisplay(
            $"Speed: {MAX_SPEED} \n" +
            $"Lidar: {distanceLidar} \n" +
            $"Ultrasonic: {distanceUltrasonic} \n" +
            $"Angle: {angle} \n" +
            $"ScanObstacleDetected: {currentScanObstacleDetected} \n" +
            $"ObstacleDetected: {obstacleDetected} \n");
    }

    private void ObstacleDetection()
    {
        if (angle < 0 && currentScanDirection == 1)
        {
            obstacleDetected = currentScanObstacleDetected;

            currentScanObstacleDetected = false;
            currentScanDirection = -1;
        }
        if (angle > maxScanAngle && currentScanDirection == -1)
        {
            obstacleDetected = currentScanObstacleDetected;

            currentScanObstacleDetected = false;
            currentScanDirection = 1;
        }   

        if (distanceLidar < MIN_DISTANCE)
        {
            if (!currentScanObstacleDetected)
            {
                currentScanObstacleDetected = true;
            }
        }
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

    private void SendServoAngle(int angle)
    {
        i2c.TransmitData(2, angle);
    }

    private void ReceiveServoAngle(int angle)
    {
        this.angle = angle;
        Debug.Log($"Arduino received servo angle: {angle}");
    }

    private void Move()
    {
        if (state == 0) // Stopped
        {
            if (!obstacleDetected)
            {
                state = 1;
            }
            else
            {
                state = 3;
            }
        }
        else if (state == 1) // Forward
        {
            if (obstacleDetected)
            {
                Move(0, 0);
                state = 0;
            }
            else
            {
                Move(MAX_SPEED, MAX_SPEED);
            }
            
        }
        else if (state == 2) // Backward
        {
            if (!obstacleDetected)
            {
                state = 4; // Turn right after backing
            }
            else
            {
                Move(-MAX_SPEED, -MAX_SPEED);
            }
        }
        else if (state == 3) // Turn left
        {
            if (!obstacleDetected)
            {
                state = 0;
            }
            else
            {
                Move(MAX_SPEED, -MAX_SPEED);
            }
            
        }
        else if (state == 4) // Turn right
        {
            if (!obstacleDetected)
            {
                state = 0;
            }
            else
            {
                Move(-MAX_SPEED, MAX_SPEED);
            }
        }
    }
}