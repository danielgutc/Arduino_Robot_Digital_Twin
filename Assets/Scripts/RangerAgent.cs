using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Sensors.Reflection;
using UnityEngine;

public class RangerAgent : Agent
{
    private AgenticController rangerController;

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
        sensor.AddObservation(rangerController.LeftMotorSpeed);
        sensor.AddObservation(rangerController.RightMotorSpeed);
        sensor.AddObservation(rangerController.DistanceLidar);
        sensor.AddObservation(rangerController.Angle);
    }

    public override void Initialize()
    {
        rangerController = GetComponentInParent<AgenticController>();
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var continuousActions = actions.ContinuousActions;

        // Scale actions to motor speeds (do this first because reward uses resulting left/right speeds).
        float leftMotor = continuousActions[0] * rangerController.MAX_SPEED;
        float rightMotor = continuousActions[1] * rangerController.MAX_SPEED;
        rangerController.LeftMotorSpeed = leftMotor;
        rangerController.RightMotorSpeed = rightMotor;
        // -

        #region -- Calculate reward -- 

        // Early checks and local copies to reduce property access overhead
        float halfMinDistance = rangerController.MIN_DISTANCE / 2;

        int dist = rangerController.DistanceLidar;
        // Immediate failure condition: too close -> maximal negative reward + end episode
        if (dist != 0 && dist < rangerController.MIN_DISTANCE / 2)
        {
            SetReward(-1f);
            EndEpisode();
        }

        float reward;
        float L = rangerController.LeftMotorSpeed;
        float R = rangerController.RightMotorSpeed;
        float max = rangerController.MAX_SPEED;
        float avg = (L + R) * 0.5f;
        float forward01 = 0f;
        float straight01 = 0f;

        forward01 = Mathf.Clamp01(avg / max);
        // Don't reward move backwards
        if (L > 0 || R > 0)
        {
            straight01 = 1f - Mathf.Clamp01(Mathf.Abs(L - R) / (4f * max));
        }

        float speedReward = forward01 * straight01 * 0.02f - 0.0002f;

        // Penalize values close to zero but keeping the sign
        //speedReward = Mathf.Abs(speedReward) * speedReward;
        
        if (dist != 0 && dist < rangerController.MIN_DISTANCE)
        {
            // t = 0 at halfMinDistanceF, t = 1 at minDistanceF
            float t = Mathf.Clamp01((dist - halfMinDistance) / (rangerController.MIN_DISTANCE - halfMinDistance));
            // Interpolate from -1 (at halfMinDistanceF) to speedReward (at minDistanceF)
            reward = Mathf.Lerp(-1f, speedReward, t);
        }
        else
        {
            // Safe distance or unknown distance -> reward based purely on speed
            reward = speedReward;
        }

        SetReward(reward);

        #endregion -- Calculate reward -- 
    }

    public override void OnEpisodeBegin()
    {
        rangerController.transform.position = RangerSpawner.rangerPosition;
    }
}
