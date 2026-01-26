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
        var discreteActions = actions.DiscreteActions;

        // Scale actions to motor speeds (do this first because reward uses resulting left/right speeds).
        float leftMotor = continuousActions[0] * rangerController.MAX_SPEED;
        float rightMotor = continuousActions[1] * rangerController.MAX_SPEED;
        rangerController.LeftMotorSpeed = leftMotor;
        rangerController.RightMotorSpeed = rightMotor;
/*        if (discreteActions[0] == 0)
        {
            rangerController.SetLidarAngle(rangerController.FORWARD_SCAN_ANGLE);
        }
        else if (discreteActions[0] == 1)
        {
            rangerController.SetLidarAngle(rangerController.WIDE_SCAN_ANGLE);
        }*/
        // -

        #region -- Calculate reward -- 

        // End episode on collision or too close to an obstacle
        int dist = rangerController.DistanceLidar;
        if ((dist != 0 && dist < rangerController.MIN_DISTANCE / 2) || collide)
        {
            SetReward(-1f);
            EndEpisode();
        }
        // --


        float reward;
        float L = rangerController.LeftMotorSpeed;
        float R = rangerController.RightMotorSpeed;
        float max = rangerController.MAX_SPEED;
        
        // Too close to an obstacle -> negative reward
        if (dist != 0 && dist < rangerController.MIN_DISTANCE * 2)
        {
            float rotationReward = -1f;
            // Reward is based on turning away from obstacle
            if (L * R < 0)
            {
                float rotation = Mathf.InverseLerp(0, 2 * max, Mathf.Abs(L) + Mathf.Abs(R));
                float distance = Mathf.InverseLerp(rangerController.MIN_DISTANCE / 2, rangerController.MIN_DISTANCE * 2, dist - rangerController.MIN_DISTANCE);
                rotationReward = rotation * distance * 0.02f;
            }
            reward = rotationReward;
        }
        else
        {
            float speedReward = -1f;
            // Reward is based on speed and going straight forward
            if (L > 0 && R > 0)
            {
                float avg = (L + R) * 0.5f;
                float forward = Mathf.Clamp01(avg / max);
                float straight = Mathf.Clamp((L + R) / (2f * max), -1, 1);
                speedReward = forward * straight * 0.02f;
            }
            reward = speedReward;
        }

        SetReward(reward);

        #endregion -- Calculate reward -- 
    }

    public override void OnEpisodeBegin()
    {
        transform.parent.transform.SetPositionAndRotation(FindFirstObjectByType<RangerSpawner>().rangerPosition[int.Parse(transform.parent.name.Split('_')[1])], Quaternion.Euler(0, 0, 0));
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
