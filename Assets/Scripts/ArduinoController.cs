using UnityEngine;

public class ArduinoController : MonoBehaviour
{
    private I2CCommunication i2c;
    public MeEncoderOnBoard leftMotor;
    public MeEncoderOnBoard rightMotor;
    public MeUltrasonicSensor ultrasonicSensor;
    public TFminiS lidarSensor;
    public DebugDisplay debugDisplay;

    private CrawlerDriveController crawlerDriveController;

    private int speed;
    private int MIN_DISTANCE = 500;

    void Start()
    {
        
        //leftMotor = FindFirstObjectByType<MeEncoderOnBoard>();
        //rightMotor = FindFirstObjectByType<MeEncoderOnBoard>();
        //ultrasonicSensor = FindFirstObjectByType<MeUltrasonicSensor>();
        //lidarSensor = FindFirstObjectByType<TFminiS>();

        crawlerDriveController = FindFirstObjectByType<CrawlerDriveController>();
        crawlerDriveController.SetMotors(leftMotor, rightMotor);
        debugDisplay = FindFirstObjectByType<DebugDisplay>();

        i2c = FindFirstObjectByType<I2CCommunication>();
        i2c.RegisterDevice(1, ReceiveServoAngle);
    }

    void Update()
    {
        // Read sensor data
        int distanceUltrasonic = (int)ultrasonicSensor.GetDistanceCm();
        int distanceLidar = lidarSensor.GetDistance();

        // Control logic based on sensor inputs
        if (distanceUltrasonic < MIN_DISTANCE)
        {
            speed = 0;
            leftMotor.SetMotorSpeed(speed);
            rightMotor.SetMotorSpeed(speed);
            Debug.Log("Obstacle detected! Stopping motors.");
        }
        else if (distanceLidar < MIN_DISTANCE * 10)
        {
            speed = 50;
            leftMotor.SetMotorSpeed(-speed);
            rightMotor.SetMotorSpeed(speed);
            Debug.Log("Object close, moving slowly.");
        }
        else
        {
            speed = 100;
            leftMotor.SetMotorSpeed(-speed);
            rightMotor.SetMotorSpeed(speed);
            Debug.Log("Path clear, moving normally.");
        }

        debugDisplay.UpdateDisplay($"Speed: {speed}, Lidar: {distanceLidar}, Ultrasonic: {distanceUltrasonic}");
    }

    private void ReceiveServoAngle(int angle)
    {
        Debug.Log($"Arduino received servo angle: {angle}");
    }
}