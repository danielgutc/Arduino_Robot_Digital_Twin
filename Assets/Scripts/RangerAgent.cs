using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Sensors.Reflection;
using UnityEngine;

public class RangerAgent : Agent
{
    private AgenticController rangerController;
    private Rigidbody rangerRb;
    // Cached floats to avoid repeated casts/divisions
    private float maxSpeedF = 1f;
    private float invMaxSpeed = 1f; // 1 / maxSpeedF
    private float minDistanceF = 1f;
    private float halfMinDistanceF = 0.5f;

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Heuristic should write into the ActionBuffers when used by the agent.
        // But keep the original behavior of setting the controller motors for manual testing.
        rangerController.LeftMotorSpeed = rangerController.MAX_SPEED;
        rangerController.RightMotorSpeed = rangerController.MAX_SPEED;

        var continuousOut = actionsOut.ContinuousActions;
        continuousOut[0] = 1f;
        continuousOut[1] = 1f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(rangerRb.linearVelocity);
        sensor.AddObservation(rangerController.DistanceLidar);
        sensor.AddObservation(rangerController.Angle);
    }

    public override void Initialize()
    {
        rangerController = GetComponentInParent<AgenticController>();
        if (rangerController != null)
        {
            rangerRb = rangerController.GetComponent<Rigidbody>();
            if (rangerRb == null)
            {
                throw new EntryPointNotFoundException("RigitBody Not found");
            }
            maxSpeedF = rangerController.MAX_SPEED > 0 ? (float)rangerController.MAX_SPEED : 1f;
            invMaxSpeed = 1f / maxSpeedF;
            minDistanceF = rangerController.MIN_DISTANCE;
            halfMinDistanceF = minDistanceF * 0.5f;
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var continuousActions = actions.ContinuousActions;

        // Scale actions to motor speeds (do this first because reward uses resulting left/right speeds).
        float leftMotor = continuousActions[0] * rangerController.MAX_SPEED;
        float rightMotor =continuousActions[1] * rangerController.MAX_SPEED;
        rangerController.LeftMotorSpeed = leftMotor;
        rangerController.RightMotorSpeed = rightMotor;
        // -

        #region Calculate reward
        // Early checks and local copies to reduce property access overhead
        int dist = rangerController.DistanceLidar;
        float reward;

        // Immediate failure condition: too close -> maximal negative reward + end episode
        if (dist != 0 && dist < halfMinDistanceF)
        {
            SetReward(-1f);
            EndEpisode();
            return;
        }

        // Compute forward speed projection (only positive forward movement yields positive reward)
        float forwardSpeed = 0f;
        // Dot with forward gives signed forward velocity
        forwardSpeed = Vector3.Dot(rangerRb.linearVelocity, rangerController.transform.forward);
        if (forwardSpeed < 0f)
        {
            forwardSpeed = 0f;
        }
        
        // Normalize forward speed using cached inverse max speed (faster than division)
        float normalizedForward = forwardSpeed * invMaxSpeed;
        if (normalizedForward > 1f)
        {
            normalizedForward = 1f;
        }
        else if (normalizedForward < 0f)
        {
            normalizedForward = 0f;
        }

        // Velocity-based reward scaled to maximum 0.1
        const float maxVelocityReward = 0.1f;
        float velocityReward = normalizedForward * maxVelocityReward;

        // If distance sensor reports an obstacle inside MIN_DISTANCE, interpolate reward toward -1
        if (dist != 0 && dist < minDistanceF)
        {
            // t = 0 at halfMinDistanceF, t = 1 at minDistanceF
            float t = Mathf.Clamp01((dist - halfMinDistanceF) / (minDistanceF - halfMinDistanceF));
            // Interpolate from -1 (at halfMinDistanceF) to velocityReward (at minDistanceF)
            reward = Mathf.Lerp(-1f, velocityReward, t);
        }
        else
        {
            // Safe distance or unknown distance -> reward based purely on forward velocity
            reward = velocityReward;
        }

        // Clamp final reward to the allowed range [-1, 0.1]
        reward = Mathf.Clamp(reward, -1f, maxVelocityReward);

        SetReward(reward);
        #endregion
    }

    public override void OnEpisodeBegin()
    {
        rangerController.transform.position = RangerSpawner.rangerPosition;
    }
}
