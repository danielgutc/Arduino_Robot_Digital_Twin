using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RangerSpawner : MonoBehaviour
{
    private string prefabsPath = "prefabs/";
    
    public enum RangerType
    {
        RangerModularPrefab,
        RangerModularAgenticPrefab
    }
    
    public enum LidarType
    {
        LidarPhysicalPrefab,
        LidarSimulatedPrefab
    }

    public enum ServoType
    {
        ServoPhysicalPrefab,
        ServoSimulatedPrefab
    }

    public enum DifferentialDriveType
    {
        DifferentialDrivePhysicsPrefab,
        DifferentialDriveTransformPrefab
    }

    public enum MeEncoderOnBoardType
    {
        MeEncoderOnBoardPhysicalPrefab,
        MeEncoderOnBoardSimulatedPrefab
    }

    public enum MeUltrasonicSensorType
    {
        MeUltrasonicSensorPhysicalPrefab,
        MeUltrasonicSensorSimulatedPrefab
    }

    public RangerType rangerType= RangerType.RangerModularPrefab;
    public DifferentialDriveType differentialDriveType = DifferentialDriveType.DifferentialDriveTransformPrefab;
    public MeEncoderOnBoardType meEncodersOnBoardType = MeEncoderOnBoardType.MeEncoderOnBoardSimulatedPrefab;
    public ServoType servoType = ServoType.ServoSimulatedPrefab;
    public LidarType lidarType = LidarType.LidarSimulatedPrefab;
    public MeUltrasonicSensorType meUltrasonicSensorType = MeUltrasonicSensorType.MeUltrasonicSensorSimulatedPrefab;
    public List<Vector3> rangerPosition = new();
    public TerminalDisplay terminal;
    public TerminalDisplay bleTerminal;

    void Start()
    {
        for (int i = 0; i < rangerPosition.Count; i++)
        {
            RangerBuilder rangerBuilder = new();
            GameObject ranger = rangerBuilder
                .NewRanger(prefabsPath + rangerType.ToString(), $"Ranger_{i}")
                .SetDiffentialDrive(prefabsPath + differentialDriveType.ToString())
                .SetMeEncodersOnBoard(prefabsPath + meEncodersOnBoardType.ToString())
                .SetServo(prefabsPath + servoType.ToString())
                .SetLidar(prefabsPath + lidarType.ToString())
                .SetUltrasonicSensor(prefabsPath + meUltrasonicSensorType.ToString())
                .SetTerminal(terminal)
                .SetBleTerminal(bleTerminal)
                .Build();
;
            ranger.transform.position = rangerPosition[i];
        }
        
    }
}
