using UnityEngine;

using System.Collections.Generic;
using System;

public class I2CBus : MonoBehaviour
{
    private Dictionary<int, Action<int>> deviceRegistry = new Dictionary<int, Action<int>>();

    // Register a device with a unique ID and its event callback
    public void RegisterDevice(int deviceId, Action<int> callback)
    {
        if (!deviceRegistry.ContainsKey(deviceId))
        {
            deviceRegistry.Add(deviceId, callback);
            Debug.Log($"I2C Device {deviceId} registered.");
        }
    }

    // Unregister a device (useful for cleanup)
    public void UnregisterDevice(int deviceId)
    {
        if (deviceRegistry.ContainsKey(deviceId))
        {
            deviceRegistry.Remove(deviceId);
            Debug.Log($"I2C Device {deviceId} unregistered.");
        }
    }

    // Send data to a registered device
    public void TransmitData(int targetDeviceId, int data)
    {
        if (deviceRegistry.TryGetValue(targetDeviceId, out Action<int> callback))
        {
            callback.Invoke(data);
            Debug.Log($"I2C Transmission -> Device: {targetDeviceId}, Data: {data}");
        }
        else
        {
            Debug.LogWarning($"I2C Transmission failed: Device {targetDeviceId} not found.");
        }
    }
}