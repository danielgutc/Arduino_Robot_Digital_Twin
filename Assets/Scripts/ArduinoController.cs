using System;
using System.Collections;
using System.Threading;
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

    public int FORWARD_SCAN_ANGLE = 45;
    public int MIN_DISTANCE = 1000;
    public int MAX_SPEED = 100; 
    public int WIDE_SCAN_ANGLE = 180;
    public int WAIT_SERVO_POSITION = 5;
    public float TURN_SPEED_MULT = 0.5f;

    private int distanceLidar;
    private int distanceUltrasonic;
    private int state = 0; // 0 - stopped; 1 - forward; 2 - backguard; 3 - rotating left; 4 - rotating right
    private int angle;
    private int currentScanDirection = 1; // Left = -1, Right = 1

    private bool currentScanObstacleDetected = false;
    private bool obstacleDetected = false;

    private int currentScanMaxDistance = -1;
    private int currentScanMaxDistanceAngle = -1;
    private int maxDistanceAngle = -1;
    private bool waitNextScan = false;

    private float waitEndTime = -1;


    void Start()
    {
        crawlerDriveController = FindFirstObjectByType<CrawlerDriveController>();
        crawlerDriveController.SetMotors(leftMotor, rightMotor);
        debugDisplay = FindFirstObjectByType<DebugDisplay>();
        i2c = FindFirstObjectByType<I2CBus>();
        i2c.RegisterDevice(1, ReceiveServoAngle);

        SendScanAngle(FORWARD_SCAN_ANGLE);
    }

    void Update()
    {
        // Simulate Wait time in Arduino. This is not required to be ported to Arduino
        if (waitEndTime != -1)
        {
            if (Time.time < waitEndTime)
            {
                return;
            }
            else
            {
                waitEndTime = -1;
            }    
        }

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
            $"ObstacleDetected: {obstacleDetected} \n" +
            $"CurrentScanMaxDistance: {currentScanMaxDistance} \n" +
            $"CurrentScanMaxDistanceAngle: {currentScanMaxDistanceAngle} \n" +
            $"MaxDistanceAngle: {maxDistanceAngle} \n" +
            $"WaitNextScan: {waitNextScan} \n" +
            $"waitEndTime: {waitEndTime} \n"
            );
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

    private void ReceiveServoAngle(int angle)
    {
        this.angle = angle;
        Debug.Log($"Arduino received servo angle: {angle}");
    }
    private void SendScanAngle(int angle)
    {
        i2c.TransmitData(2, angle);
    }

    private void ObstacleDetection()
    {
        if (angle < 0 && currentScanDirection == 1)
        {
            if (!waitNextScan)
            {
                obstacleDetected = currentScanObstacleDetected;
                maxDistanceAngle = currentScanMaxDistanceAngle;
            }
            currentScanObstacleDetected = false;

            currentScanDirection = -1;
            currentScanMaxDistanceAngle = -1;
            currentScanMaxDistance = -1;
            waitNextScan = false;

        }
        if (angle > 0 && currentScanDirection == -1)
        {
            if (!waitNextScan)
            {
                maxDistanceAngle = currentScanMaxDistanceAngle;
                obstacleDetected = currentScanObstacleDetected;
            }

            currentScanObstacleDetected = false;
            currentScanDirection = 1;
            currentScanMaxDistanceAngle = -1;
            currentScanMaxDistance = -1;
            waitNextScan = false;
        }

        if (distanceLidar < MIN_DISTANCE)
        {
            if (!currentScanObstacleDetected)
            {
                currentScanObstacleDetected = true;
            }
        }

        currentScanMaxDistanceAngle = distanceLidar > currentScanMaxDistance ? Math.Abs(angle) : currentScanMaxDistanceAngle;
        currentScanMaxDistance = Math.Max(currentScanMaxDistance, distanceLidar);
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
                SendScanAngle(WIDE_SCAN_ANGLE);
                waitNextScan = true;
                maxDistanceAngle = -1;
                state = 5; // Wait for the farthest direction
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
                Move(MAX_SPEED * TURN_SPEED_MULT, -MAX_SPEED * TURN_SPEED_MULT);
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
                Move(-MAX_SPEED * TURN_SPEED_MULT, MAX_SPEED * TURN_SPEED_MULT);
            }
        }
        else if (state == 5) // Find farthest direction
        {
            if (maxDistanceAngle != -1)
            {
                state = (maxDistanceAngle < 90) ? 3 : 4;
                SendScanAngle(FORWARD_SCAN_ANGLE);
                waitNextScan = true;
                Wait(WAIT_SERVO_POSITION);
                //obstacleDetected = true;
            }
        }
    }

    private void Wait(int v)
    {
        // calculate the next time
        waitEndTime = Time.time + v;
    }
}