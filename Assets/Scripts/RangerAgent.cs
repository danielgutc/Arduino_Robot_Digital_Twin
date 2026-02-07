using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Sensors.Reflection;
using UnityEngine;

public class RangerAgent : Agent
{
    //private AgenticController rangerController;
    private ArduinoController rangerController;
    private bool heuristicMode;
    private bool collide = false;

    /**
     * 
     * Heuristic can support 2 cases:
     *  1. Use ArduinoController simulated logic and telemerty 
     *  2. Use ArduinoController as proxy of physical telemetry beings sent by the physical Ranger
     *
     */
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousOut = actionsOut.ContinuousActions;

        continuousOut[0] = rangerController.LeftMotorSpeed / rangerController.MAX_SPEED;
        continuousOut[1] = rangerController.RightMotorSpeed/ rangerController.MAX_SPEED;
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

        rangerController = GetComponentInParent<ArduinoController>();
        CollisionSensor collisionSensor = GetComponentInParent<CollisionSensor>();
        collisionSensor.OnCollisionStateChanged += handleCollision;
        heuristicMode = GetComponent<BehaviorParameters>().IsInHeuristicMode();

        //Arduino controller is flagged as being agent controlled unless the agent is in heuristic mode
        if (!heuristicMode)
        {
            rangerController.agentControlled = true;
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var continuousActions = actions.ContinuousActions;
        var discreteActions = actions.DiscreteActions;

        // Scale actions to motor speeds (do this first because reward uses resulting left/right speeds).
        float leftMotorSpeed = continuousActions[0] * rangerController.MAX_SPEED;
        float rightMotorSpeed = continuousActions[1] * rangerController.MAX_SPEED;

        // Do not control the ranger in heuristic mode
        if (!heuristicMode)
        {   
            // Hack: send negative left speed to avoid changes in the arduino controller code
            rangerController.Move(-leftMotorSpeed, rightMotorSpeed);
        }

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
