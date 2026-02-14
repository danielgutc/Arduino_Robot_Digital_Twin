using System;
using TFminiS;
using MeEncoderOnBoard;
using UnityEngine;
using DiferentialDrive;
using MeUltrasonicSensor;

public class ArduinoController : MonoBehaviour
{
    protected I2CBus i2c;
    public IMeEncoderOnBoard leftMotor;
    public IMeEncoderOnBoard rightMotor;
    public IMeUltrasonicSensor ultrasonicSensor;
    public ITFminiS lidarSensor;
    public TerminalDisplay terminalDisplay;

    public bool agentControlled = false;

    public int FORWARD_SCAN_ANGLE = 45;
    public int MIN_DISTANCE = 5;
    public float MIN_DISTANCE_MULT = 1.5f;
    public int MAX_SPEED = 100; 
    public int WIDE_SCAN_ANGLE = 180;
    public float WAIT_SERVO_POSITION = 1.5f;
    public float TURN_SPEED_MULT = 0.5f;

    public float Angle { get => angle; }
    public float LeftMotorSpeed { get => leftMotor.GetCurrentSpeed(); }
    public float RightMotorSpeed { get => rightMotor.GetCurrentSpeed(); }
    public int DistanceLidar { get => distanceLidar; }
    protected int distanceLidar;
    protected int distanceUltrasonic;
    private int state = 0; // 0 - stopped; 1 - forward; 2 - backguard; 3 - rotating left; 4 - rotating right
    protected int angle;
    private int currentScanDirection = 1; // Left = -1, Right = 1

    private bool obstacleDetected = true;
    private int currentScanMaxDistance = -1;
    private int currentScanMaxDistanceAngle = -1;
    private int currentScanMinDistance = int.MaxValue;
    private float minDistance = -1;
    private int maxDistanceAngle = -1;
    private bool waitNextScan = false;
    private float waitEndTime = -1;
    private bool firstCyle = true;

    void Start()
    {
        i2c = FindFirstObjectByType<I2CBus>();
        i2c.RegisterDevice(1, null, null);
    }

    void Update()
    {
        // TODO: remove this ugly solution for the first cycle where the servo angle is not received yet. Is there any AfterStar() in Unity?
        if (firstCyle)
        {
            SendScanMaxAngle(FORWARD_SCAN_ANGLE);
            firstCyle = false;
        }

        RequestServoAngle();
        UpdateDistanceUltrasonic();
        UpdateDistanceLidar();
        ObstacleDetection();

        if (!agentControlled)
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
            Move();
        }
        
        DisplayTelemetry();
    }

    protected void DisplayTelemetry()
    {
        terminalDisplay.UpdateDisplay(
            $"State: {state} \n" +
            $"Lidar: {DistanceLidar} \n" +
            $"Ultrasonic: {distanceUltrasonic} \n" +
            $"Angle: {Angle} \n" +
            $"ObstacleDetected: {obstacleDetected} \n" +
            $"CurrentScanMaxDistance: {currentScanMaxDistance} \n" +
            $"CurrentScanMaxDistanceAngle: {currentScanMaxDistanceAngle} \n" +
            $"MaxDistanceAngle: {maxDistanceAngle} \n" +
            $"WaitNextScan: {waitNextScan} \n" +
            $"LeftMotorSpeed: {leftMotor.GetCurrentSpeed()} \n" +
            $"RightMotorSpeed: {rightMotor.GetCurrentSpeed()}"
            );
    }

    protected void UpdateDistanceLidar()
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

    protected void UpdateDistanceUltrasonic()
    {
        distanceUltrasonic = (int)ultrasonicSensor.GetDistanceCm();
    }

    public void Move(float leftSpeed, float rightSpeed)
    {
        leftMotor.SetCurrentSpeed(-leftSpeed);
        rightMotor.SetCurrentSpeed(rightSpeed);
    }

    protected void RequestServoAngle()
    {
        this.angle = i2c.RequestData(2);
        Debug.Log($"Arduino received servo angle: {angle}");
    }
    protected void SendScanMaxAngle(int angle)
    {
        if (i2c != null)
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

    private void Wait(float t)
    {
        // calculate the next time
        waitEndTime = Time.time + t;
    }
}