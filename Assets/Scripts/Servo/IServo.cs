namespace Servo
{
    public interface IServo
    {
        bool Attached();
        void Detach();
        int Read();
        int ReadMicroseconds();
        void Write(int value);
        void WriteMicroseconds(int value);
    }
}