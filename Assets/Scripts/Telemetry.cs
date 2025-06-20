using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[System.Serializable]
public class Telemetry
{
    public int State { get; set; } 
    public int Lidar { get; set; }
    public int Ultrasonic { get; set; }
    public int Angle { get; set; }
    public bool ObstacleDetected { get; set; }
    public int CurrentScanMaxDistance { get; set; }
    public int CurrentScanMaxDistanceAngle { get; set; }
    public int MaxDistanceAngle { get; set; }
    public int WaitNextScan { get; set; }
    public int LeftMotorSpeed { get; set; }
    public int RightMotorSpeed { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"State: {State}");
        sb.AppendLine($"Lidar: {Lidar}");
        sb.AppendLine($"Ultrasonic: {Ultrasonic}");
        sb.AppendLine($"Angle: {Angle}");
        sb.AppendLine($"ObstacleDetected: {ObstacleDetected}");
        sb.AppendLine($"CurrentScanMaxDistance: {CurrentScanMaxDistance}");
        sb.AppendLine($"CurrentScanMaxDistanceAngle: {CurrentScanMaxDistanceAngle}");
        sb.AppendLine($"MaxDistanceAngle: {MaxDistanceAngle}");
        sb.AppendLine($"WaitNextScan: {WaitNextScan}");
        sb.AppendLine($"LeftMotorSpeed: {LeftMotorSpeed}");
        sb.AppendLine($"RightMotorSpeed: {RightMotorSpeed}");
        return sb.ToString();
    }
}

