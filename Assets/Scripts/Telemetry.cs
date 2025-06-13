using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[System.Serializable]
public class Telemetry
{
    public string state;
    public string lidar;
    public string ultrasonic;
    public string angle;
    public string obstacleDetected;
    public string currentScanMaxDistance;
    public string currentScanMaxDistanceAngle;
    public string maxDistanceAngle;
    public string waitNextScan;
    public string leftMotorSpeed;
    public string rightMotorSpeed;
}

