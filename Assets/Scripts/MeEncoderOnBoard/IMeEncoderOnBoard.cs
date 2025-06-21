namespace MeEncoderOnBoard
{
    public interface IMeEncoderOnBoard
    {
        float GetCurrentSpeed();
        float GetPosition();
        void SetMotorSpeed(int speed);
        void SetPosition(float position);
        void StopMotor();
    }
}