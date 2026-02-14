namespace MeEncoderOnBoard
{
    public interface IMeEncoderOnBoard
    {
        float GetCurrentSpeed();
        void SetCurrentSpeed(float speed);
        float GetPosition();
        void SetPosition(float position);
        void StopMotor();
    }
}