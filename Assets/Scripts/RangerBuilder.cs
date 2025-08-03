using DiferentialDrive;
using MeEncoderOnBoard;
using MeUltrasonicSensor;
using Servo;
using System;
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
        Transform servo = ranger.transform.Find("Servo Base").Find("Servo");
        if (servo == null)
        {
            throw new Exception("Servo not found. Please build the servo by invoking SetServo() before building the lidar");
        }

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
        Transform servoBase = ranger.transform.Find("Servo Base");
        GameObject servoPrefab = Resources.Load<GameObject>(servoPrefabPath);
        GameObject servo = GameObject.Instantiate(servoPrefab, servoBase);
        servo.name = "Servo";

        ArduinoControllerExtension controllerExtension = ranger.GetComponent<ArduinoControllerExtension>();
        controllerExtension.servo = servo.GetComponent<IServo>();

        return this;
    }

    public RangerBuilder SetMeEncodersOnBoard(string meEncodersOnBoardPrefabPath)
    {
        GameObject meEncodersOnBoardPrefab = Resources.Load<GameObject>(meEncodersOnBoardPrefabPath);
        GameObject leftMotor = GameObject.Instantiate(meEncodersOnBoardPrefab, ranger.transform);
        leftMotor.name = "LeftMotor";
        GameObject rightMotor = GameObject.Instantiate(meEncodersOnBoardPrefab, ranger.transform);
        rightMotor.name = "RightMotor";

        ArduinoController controller = ranger.GetComponent<ArduinoController>();
        controller.leftMotor = leftMotor.GetComponent<IMeEncoderOnBoard>();
        controller.rightMotor = rightMotor.GetComponent<IMeEncoderOnBoard>();

        MotorUnit motorUnit = ranger.GetComponent<MotorUnit>();
        motorUnit.leftMotor = controller.leftMotor;
        motorUnit.rightMotor = controller.rightMotor;

        return this;
    }

    public RangerBuilder SetUltrasonicSensor(string ultrasonicSensorPrefabPath)
    {
        GameObject ultrasonicSensorPrefab = Resources.Load<GameObject>(ultrasonicSensorPrefabPath);
        GameObject ultrasonicSensor = GameObject.Instantiate(ultrasonicSensorPrefab, ranger.transform);
        ultrasonicSensor.name = "UltrasonicSensor";

        ArduinoController controller = ranger.GetComponent<ArduinoController>();
        controller.ultrasonicSensor = ultrasonicSensor.GetComponent<IMeUltrasonicSensor>();

        return this;
    }

    public GameObject Build()
    {
        SceneManager.MoveGameObjectToScene(ranger, SceneManager.GetActiveScene());
        return ranger;
    }
}
