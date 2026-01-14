using UnityEngine;
using System;

public class CollisionSensor : MonoBehaviour
{
    public event Action<bool> OnCollisionStateChanged;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag.Equals("Obstacle"))
        {
            OnCollisionStateChanged?.Invoke(true);
        }
        
    }

    void OnCollisionExit(Collision collision)
    {
        OnCollisionStateChanged?.Invoke(false);
    }
}
