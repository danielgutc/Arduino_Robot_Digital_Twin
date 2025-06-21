using Servo;
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
        Transform servo = ranger.transform.Find("Servo");
        GameObject lidarPrefab = Resources.Load<GameObject>(lidarPrefabPath);
        GameObject lidar = GameObject.Instantiate(lidarPrefab, servo);
        lidar.name = "Lidar";

        ArduinoController controller = ranger.GetComponent<ArduinoController>();
        controller.lidarSensor = lidar.GetComponent<ITFminiS>();

        return this;
    }

    public RangerBuilder SetTerminal(TerminalDisplay terminal)
    {
        ArduinoController controller = ranger.GetComponent<ArduinoController>();
        controller.terminalDisplay = terminal;
        
        return this;
    }

    public RangerBuilder SetServo(string servoPrefabPath)
    {
        GameObject servoPrefab = Resources.Load<GameObject>(servoPrefabPath);
        GameObject servo = GameObject.Instantiate(servoPrefab, ranger.transform);
        servo.name = "Servo";

        ArduinoControllerExtension controllerExtension = ranger.GetComponent<ArduinoControllerExtension>();
        controllerExtension.servo = servo.GetComponent<IServo>();

        return this;
    }

    public GameObject Build()
    {
        SceneManager.MoveGameObjectToScene(ranger, SceneManager.GetActiveScene());
        return ranger;
    }
}
