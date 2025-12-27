using System;
using UnityEngine;
using TFminiS;
using MeEncoderOnBoard;
using MeUltrasonicSensor;

public class AgenticController : ArduinoController
{
    private float leftMotorSpeed;
    private float rightMotorSpeed;  

    public int DistanceLidar { get => distanceLidar; set => distanceLidar = value; }
    public int DistanceUltrasonic { get => distanceUltrasonic; set => distanceUltrasonic = value; }
    public int Angle { get => angle; set => angle = value; }
    public float LeftMotorSpeed { get => leftMotorSpeed; set => leftMotorSpeed = value; }
    public float RightMotorSpeed { get => rightMotorSpeed; set => rightMotorSpeed = value; }
    

    void Update()
    {
        UpdateDistanceLidar();
        Move(LeftMotorSpeed, RightMotorSpeed);

        terminalDisplay.UpdateDisplay(
            $"Lidar: {DistanceLidar} \n" +
            $"Ultrasonic: {DistanceUltrasonic} \n" +
            $"Angle: {Angle} \n" +
            $"LeftMotorSpeed: {LeftMotorSpeed} \n" +
            $"RightMotorSpeed: {RightMotorSpeed}"
            );
    }
}