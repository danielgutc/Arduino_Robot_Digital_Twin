using System;
using System.Text;
using UnityEngine;

public class RangerTelemetry : MonoBehaviour
{
    public string rangerNameKeyword = "Makeblock_LE703e97f555d4";
    public string telemetryServiceUuid = "00006287-3c17-d293-8e48-14fe2e4da212";
    public string telemetryCharUuid = "0000ffe2-0000-1000-8000-00805f9b34fb";
    public DebugDisplay debugDisplay;

    private string connectedDeviceId;
    private bool isScanningDevices = false;
    private bool isSubscribed = false;

    void Start()
    {
        isScanningDevices = true;
        isSubscribed = false;
    }

    void Update()
    {
        if (isScanningDevices)
        {
            BleApi.StartDeviceScan();

            BleApi.DeviceUpdate device = new();
            while (true)
            {
                var status = BleApi.PollDevice(ref device, false);
                if (status == BleApi.ScanStatus.AVAILABLE)
                {
                    if (!string.IsNullOrEmpty(device.name) &&
                        device.name.ToLower().Contains(rangerNameKeyword.ToLower()))
                    {
                        debugDisplay.UpdateDisplay($"Found Ranger device: {device.name} ({device.id})");
                        connectedDeviceId = device.id;
                        isScanningDevices = false;
                        Subscribe();
                        isSubscribed = true;

                        break;
                    }
                }
                else if (status == BleApi.ScanStatus.FINISHED)
                {
                    debugDisplay.UpdateDisplay("Scan finished with no match.");
                }
            }

            BleApi.StopDeviceScan();
        }

        if (isSubscribed)
        {
            BleApi.BLEData telemetry = new BleApi.BLEData();
            while (BleApi.PollData(out telemetry, false))
            {
                string message = Encoding.ASCII.GetString(telemetry.buf, 0, telemetry.size).TrimEnd('\0');
                message = message.Replace(", ", "\n");
                debugDisplay.UpdateDisplay(message);
            }
        }
    }

    public void Subscribe()
    {
        // no error code available in non-blocking mode
        BleApi.SubscribeCharacteristic(connectedDeviceId, telemetryServiceUuid, telemetryCharUuid, false);
        isSubscribed = true;
    }

    public void Write(String message)
    {
        byte[] payload = Encoding.ASCII.GetBytes(message);
        BleApi.BLEData data = new()
        {
            buf = new byte[512],
            size = (short)payload.Length,
            deviceId = connectedDeviceId,
            serviceUuid = telemetryServiceUuid,
            characteristicUuid = telemetryCharUuid
        };

        for (int i = 0; i < payload.Length; i++)
        {
            data.buf[i] = payload[i];
        }
        // no error code available in non-blocking mode
        BleApi.SendData(in data, false);
    }
}
