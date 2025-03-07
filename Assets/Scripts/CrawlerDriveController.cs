using UnityEngine;

/*
 * This class controls the movement of the crawler robot using two motors. 
 * It simulates the movement of a tracked vehicle.
 * */
public class CrawlerDriveController : MonoBehaviour
{
    public Rigidbody crawlerBody; // Assign the rigidbody of the chassis
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
        crawlerBody.MovePosition(crawlerBody.position + movement);
        crawlerBody.MoveRotation(crawlerBody.rotation * Quaternion.Euler(0, rotation * Time.fixedDeltaTime, 0));
    }
}