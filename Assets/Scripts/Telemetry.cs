using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[System.Serializable]
public class Telemetry
{
    public string State { get; set; } 
    public string Lidar { get; set; }
    public string Ultrasonic { get; set; }
    public string Angle { get; set; }
    public string ObstacleDetected { get; set; }
    public string CurrentScanMaxDistance { get; set; }
    public string CurrentScanMaxDistanceAngle { get; set; }
    public string MaxDistanceAngle { get; set; }
    public string WaitNextScan { get; set; }
    public string LeftMotorSpeed { get; set; }
    public string RightMotorSpeed { get; set; }

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

