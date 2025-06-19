using System.Linq;
using UnityEngine;

public class RangerSpawner : MonoBehaviour
{
    public string rangerPrefabPath = "Prefabs/RangerModulablePrefab"; // Path to the Ranger prefab
    public string lidarPrefabPath = "Prefabs/LidarSimulatedPrefab"; // Path to the Lidar prefab
    public Vector3 rangerPosition = new(-2176, 242, 13329); // Initial position of the Ranger

    void Start()
    {
        DebugDisplay terminal = FindObjectsByType<DebugDisplay>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(x => x.name == "Terminal");
        RangerBuilder rangerBuilder = new();
        GameObject ranger = rangerBuilder
            .NewRanger(rangerPrefabPath)
            .SetLidar(lidarPrefabPath)
            .SetDebugDisplay(terminal)
            .Build();

        ranger.transform.position = rangerPosition;
    }
}
