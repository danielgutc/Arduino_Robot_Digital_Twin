namespace TFminiS
{
    public interface ITFminiS
    {
        int GetDistance();
        int GetStrength();
        int GetTemperature();
        void ReadSensor();
    }
}