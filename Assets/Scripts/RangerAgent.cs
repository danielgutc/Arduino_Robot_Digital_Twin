using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Sensors.Reflection;
using UnityEngine;

public class RangerAgent : Agent
{
    private AgenticController rangerController;
    private bool collide = false;

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
        CollisionSensor collisionSensor = GetComponentInParent<CollisionSensor>();
        collisionSensor.OnCollisionStateChanged += handleCollision;

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
        if ((dist != 0 && dist < rangerController.MIN_DISTANCE / 2) || collide)
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
        float speedReward = -1f;

        if (L > 0 && R > 0)
        {
            forward01 = Mathf.Clamp01(avg / max);
            straight01 = Mathf.Clamp((L + R) / (2f * max), -1, 1);
            speedReward = forward01 * straight01 * 0.02f;
        }

        if (dist != 0 && dist < rangerController.MIN_DISTANCE * 2)
        {
            float rotationDistance = dist - rangerController.MIN_DISTANCE;
            reward = Mathf.InverseLerp(0, rangerController.MIN_DISTANCE, rotationDistance) * -500f;
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
        collide = false;
    }

    private void handleCollision(bool isColliding)
    {
        if (isColliding)
        {
            collide = true;
        }
    }
}
