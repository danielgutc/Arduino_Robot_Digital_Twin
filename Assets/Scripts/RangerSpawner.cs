using System;
using System.Linq;
using UnityEngine;

public class RangerSpawner : MonoBehaviour
{
    private string prefabsPath = "prefabs/";
    public enum RangerType
    {
        RangerModularPrefab,
        RangerPrefab
    }
    public enum LidarType
    {
        LidarPhysicalPrefab,
        LidarSimulatedPrefab
    }
    public RangerType rangerType= RangerType.RangerModularPrefab;
    public LidarType lidarType = LidarType.LidarSimulatedPrefab;
    public Vector3 rangerPosition = new(-2176, 242, 13329);
    public TerminalDisplay terminal;

    void Start()
    {
        RangerBuilder rangerBuilder = new();
        GameObject ranger = rangerBuilder
            .NewRanger(prefabsPath + rangerType.ToString())
            .SetLidar(prefabsPath + lidarType.ToString())
            .SetTerminal(terminal)
            .Build();

        ranger.transform.position = rangerPosition;
    }
}
