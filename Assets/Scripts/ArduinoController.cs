using System;
using TFminiS;
using MeEncoderOnBoard;
using UnityEngine;

public class ArduinoController : MonoBehaviour
{
    private I2CBus i2c;
    public SimulatedMeEncoderOnBoard leftMotor;
    public SimulatedMeEncoderOnBoard rightMotor;
    public MeUltrasonicSensor ultrasonicSensor;
    public ITFminiS lidarSensor;
    public TerminalDisplay terminalDisplay;

    private DriveController rangerDriveController;

    public int FORWARD_SCAN_ANGLE = 45;
    public int MIN_DISTANCE = 1000;
    public float MIN_DISTANCE_MULT = 1.5f;
    public int MAX_SPEED = 100; 
    public int WIDE_SCAN_ANGLE = 180;
    public int WAIT_SERVO_POSITION = 5;
    public float TURN_SPEED_MULT = 0.5f;

    private int distanceLidar;
    private int distanceUltrasonic;
    private int state = 0; // 0 - stopped; 1 - forward; 2 - backguard; 3 - rotating left; 4 - rotating right
    private int angle;
    private int currentScanDirection = 1; // Left = -1, Right = 1

    //private bool currentScanObstacleDetected = false;
    private bool obstacleDetected = true;

    private int currentScanMaxDistance = -1;
    private int currentScanMaxDistanceAngle = -1;
    private int currentScanMinDistance = int.MaxValue;
    private float minDistance = -1;
    private int maxDistanceAngle = -1;
    private bool waitNextScan = false;
    private float waitEndTime = -1;

    void Start()
    {
        rangerDriveController = FindFirstObjectByType<DriveController>();
        rangerDriveController.SetMotors(leftMotor, rightMotor);
        i2c = FindFirstObjectByType<I2CBus>();
        i2c.RegisterDevice(1, null, null);
        SendScanMaxAngle(FORWARD_SCAN_ANGLE);
    }

    void Update()
    {
        // Simulate Wait time in Arduino
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

        RequestServoAngle();
        UpdateDistanceUltrasonic();
        UpdateDistanceLidar();
        ObstacleDetection();
        Move();

        terminalDisplay.UpdateDisplay(
            $"State: {state} \n" +
            $"Lidar: {distanceLidar} \n" +
            $"Ultrasonic: {distanceUltrasonic} \n" +
            $"Angle: {angle} \n" +
            $"ObstacleDetected: {obstacleDetected} \n" +
            $"CurrentScanMaxDistance: {currentScanMaxDistance} \n" +
            $"CurrentScanMaxDistanceAngle: {currentScanMaxDistanceAngle} \n" +
            $"MaxDistanceAngle: {maxDistanceAngle} \n" +
            $"WaitNextScan: {waitNextScan} \n" +
            $"LeftMotorSpeed: {leftMotor.GetCurrentSpeed()} \n" +
            $"RightMotorSpeed: {rightMotor.GetCurrentSpeed()}"
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

    private void RequestServoAngle()
    {
        this.angle = i2c.RequestData(2);
        Debug.Log($"Arduino received servo angle: {angle}");
    }
    private void SendScanMaxAngle(int angle)
    {
        i2c.TransmitData(2, angle);
    }

    private void ObstacleDetection()
    {
        if (angle < 0 && currentScanDirection == 1)
        {
            if (!waitNextScan)
            {
                maxDistanceAngle = currentScanMaxDistanceAngle;
                obstacleDetected = currentScanMinDistance < minDistance ? true : false;
            }
            
            currentScanDirection = -1;
            currentScanMaxDistanceAngle = -1;
            currentScanMaxDistance = -1;
            currentScanMinDistance = int.MaxValue;
            waitNextScan = false;

        }
        if (angle > 0 && currentScanDirection == -1)
        {
            if (!waitNextScan)
            {
                maxDistanceAngle = currentScanMaxDistanceAngle;
                obstacleDetected = currentScanMinDistance < minDistance ? true : false;
            }

            currentScanDirection = 1;
            currentScanMaxDistanceAngle = -1;
            currentScanMaxDistance = -1;
            currentScanMinDistance = int.MaxValue;
            waitNextScan = false;
        }

        currentScanMaxDistanceAngle = distanceLidar > currentScanMaxDistance ? Math.Abs(angle) : currentScanMaxDistanceAngle;
        currentScanMinDistance = Math.Min(currentScanMinDistance, distanceLidar);
        currentScanMaxDistance = Math.Max(currentScanMaxDistance, distanceLidar);
    }

    private void Move()
    {
        if (state == 0) // Stopped
        {
            if (!obstacleDetected)
            {
                state = 1;
                minDistance = MIN_DISTANCE;
            }
            else
            {
                minDistance = MIN_DISTANCE * MIN_DISTANCE_MULT;
                SendScanMaxAngle(WIDE_SCAN_ANGLE);
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
                SendScanMaxAngle(FORWARD_SCAN_ANGLE);
                waitNextScan = true;
                obstacleDetected = true;
                Wait(WAIT_SERVO_POSITION);
            }
        }
    }

    private void Wait(int v)
    {
        // calculate the next time
        waitEndTime = Time.time + v;
    }
}