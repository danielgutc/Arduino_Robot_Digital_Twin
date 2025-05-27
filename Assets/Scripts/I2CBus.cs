using UnityEngine;

using System.Collections.Generic;
using System;

public class I2CBus : MonoBehaviour
{
    // Store Action<int> (onReceive) and Func<int> (onRequest)
    private Dictionary<int, Tuple<Action<int>, Func<int>>> deviceRegistry = new();

    // Register a device with a unique ID and its event callbacks
    public void RegisterDevice(int deviceId, Action<int> onReceive, Func<int> onRequest)
    {
        if (!deviceRegistry.ContainsKey(deviceId))
        {
            deviceRegistry.Add(deviceId, new Tuple<Action<int>, Func<int>>(onReceive, onRequest));
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
        if (deviceRegistry.TryGetValue(targetDeviceId, out Tuple<Action<int>, Func<int>> callbacks))
        {
            callbacks.Item1.Invoke(data);
            Debug.Log($"I2C Transmission -> Device: {targetDeviceId}, Data: {data}");
        }
        else
        {
            Debug.LogWarning($"I2C Transmission failed: Device {targetDeviceId} not found.");
        }
    }

    public int RequestData(int targetDeviceId)
    {
        if (deviceRegistry.TryGetValue(targetDeviceId, out Tuple<Action<int>, Func<int>> callbacks))
        {
            int data = callbacks.Item2.Invoke();
            Debug.Log($"I2C Transmission -> Device: {targetDeviceId}, Data: {data}");
            return data;
        }
        else
        {
            Debug.LogWarning($"I2C Transmission failed: Device {targetDeviceId} not found.");
        }

        return -1; // Indicate failure to find device
    }
}