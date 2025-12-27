using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;


public  class RangerAgent : Agent
{
    private ArduinoController rangerController;

    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        base.Heuristic(actionsOut);
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var actionLeft = 2f * Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        var actionRight = 2f * Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
    }

    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();
    }
}
