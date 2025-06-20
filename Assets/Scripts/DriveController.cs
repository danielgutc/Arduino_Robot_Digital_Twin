using UnityEngine;

/*
 * This class controls the movement of the ranger robot using two motors. 
 * It simulates the movement of a tracked vehicle.
 * */
public class DriveController : MonoBehaviour
{
    public Rigidbody rangerBody; // Assign the rigidbody of the chassis
    public GameObject replacementPrefab;
    public float speedMultiplier = 0.1f;
    public float rotationMultiplier = 1f;

    private MeEncoderOnBoard leftMotor;
    private MeEncoderOnBoard rightMotor;

    public void SetMotors(MeEncoderOnBoard leftMotor, MeEncoderOnBoard rightMotor)
    {
        this.leftMotor = leftMotor;
        this.rightMotor = rightMotor;
    }

    void FixedUpdate()
    {
        var leftSpeed = -leftMotor.GetCurrentSpeed();
        var rightSpeed = rightMotor.GetCurrentSpeed();
        float forwardSpeed = (leftSpeed + rightSpeed) * speedMultiplier;
        float rotation = (rightSpeed - leftSpeed) * rotationMultiplier;

        Vector3 movement = transform.forward * forwardSpeed * Time.fixedDeltaTime;
        rangerBody.MovePosition(rangerBody.position + movement);
        rangerBody.MoveRotation(rangerBody.rotation * Quaternion.Euler(0, rotation * Time.fixedDeltaTime, 0));
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Collision detected with: {collision.gameObject.name}");

        if (collision.gameObject.CompareTag("Obstacle"))
        {
            if (replacementPrefab != null)
            {
                Vector3 initPosition = new Vector3(-2176, 146, 13329);
                GameObject newInstance = Instantiate(replacementPrefab, initPosition, transform.rotation);
                ArduinoController arduinoController = newInstance.GetComponent<ArduinoController>();
                if (arduinoController != null)
                {
                    arduinoController.terminalDisplay = FindFirstObjectByType<ArduinoController>().terminalDisplay;
                }

                DriveController rangerDriveController = newInstance.GetComponent<DriveController>();
                if (rangerDriveController != null)
                {
                    rangerDriveController.replacementPrefab = replacementPrefab;
                }
            }
            Destroy(gameObject);
        }
    }

}