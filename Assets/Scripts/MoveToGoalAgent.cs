using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class MoveToGoalAgent : Agent
{
    public override void OnActionReceived(ActionBuffers actions)
    {
        Debug.Log("DiscreteActions: " + actions.DiscreteActions[0]);
        // Debug.Log("ContinuousActions: " + actions.ContinuousActions[0]);
    }
}
