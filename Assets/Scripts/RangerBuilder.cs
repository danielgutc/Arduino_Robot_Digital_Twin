using TFminiS;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RangerBuilder
{
    private GameObject ranger;

    public RangerBuilder NewRanger(string rangerPrefabPath)
    {
        GameObject rangerPrefab = Resources.Load<GameObject>(rangerPrefabPath);
        ranger = GameObject.Instantiate(rangerPrefab);
        ranger.name = "Ranger";

        return this;
    }

    public RangerBuilder SetLidar(string lidarPrefabPath)
    {
        Transform servo = ranger.transform.Find("ServoMotor");
        GameObject lidarPrefab = Resources.Load<GameObject>(lidarPrefabPath);
        GameObject lidar = GameObject.Instantiate(lidarPrefab, servo);
        lidar.name = "Lidar";

        ArduinoController controller = ranger.GetComponent<ArduinoController>();
        controller.lidarSensor = lidar.GetComponent<ITFminiS>();

        return this;
    }

    public RangerBuilder SetDebugDisplay(DebugDisplay debugDisplay)
    {
        ArduinoController controller = ranger.GetComponent<ArduinoController>();
        controller.debugDisplay = debugDisplay;
        
        return this;
    }

    public GameObject Build()
    {
        SceneManager.MoveGameObjectToScene(ranger, SceneManager.GetActiveScene());
        return ranger;
    }
}
