using System;
using System.Collections;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

public class TurtleAgent : Agent
{
    [SerializeField] private Transform _goal;
    [SerializeField] private float _moveSpeed = 1.5f;
    [SerializeField] private float _rotationSpeed = 180f;
    [SerializeField] private GameObject _ground;

    private Renderer _renderer;
    private Renderer _groundRenderer;
    private Material _groundMaterial;

    private int _currentEpisode = 0;
    private float _cumulativeReward = 0f;
    
    StatsRecorder m_statsRecorder;
    
    public override void Initialize()
    {
        Debug.Log("Initialize()");
        
        _renderer = GetComponent<Renderer>();
        _currentEpisode = 0;

        _groundRenderer = _ground.GetComponent<Renderer>();
        _groundMaterial = _groundRenderer.material;
        _groundMaterial.color = Color.gray;

        m_statsRecorder = Academy.Instance.StatsRecorder;
    }

    public override void OnEpisodeBegin()
    {
        _currentEpisode++;
        _cumulativeReward = 0;
        _renderer.material.color = Color.blue;

        SpawnObjects();
        
        m_statsRecorder.Add("Goal/Correct", 0, StatAggregationMethod.Sum);
        m_statsRecorder.Add("Goal/Wrong", 0, StatAggregationMethod.Sum);
    }

    private void SpawnObjects()
    {
        // turtle position
        transform.localRotation = Quaternion.identity;
        transform.localPosition = new Vector3(0f, 0.3f, 0f);
        
        // Randomize the direction on the Y-axis (angle in degrees)
        float randomAngle = Random.Range(0f, 360f);
        Vector3 randomDirection = Quaternion.Euler(0f, randomAngle, 0f) * Vector3.forward;
        
        // Randomize the distance within the range [1, 2.5]
        float randomDistance = Random.Range(1f, 2.5f);
        
        // Calculate the goal's position
        Vector3 goalPosition = transform.localPosition + randomDirection * randomDistance;
        
        // Apply the calculated position to the goal
        _goal.localPosition = new Vector3(goalPosition.x, 0.3f, goalPosition.z);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // The Goal's postion
        float goalPosX_normalized = _goal.localPosition.x / 5f;
        float goalPosZ_normalized = _goal.localPosition.z / 5f;
        
        // The Turtle's position
        float turtlePosX_normalized = transform.localPosition.x / 5f;
        float turtlePosZ_normalized = transform.localPosition.z / 5f;
        
        // The Turtle's direction (on the Y axis) => -1.0 ~ 1.0
        float turtleRotation_normalized = (transform.localRotation.eulerAngles.y / 360f) * 2f - 1f;
        
        sensor.AddObservation(goalPosX_normalized);
        sensor.AddObservation(goalPosZ_normalized);
        sensor.AddObservation(turtlePosX_normalized);
        sensor.AddObservation(turtlePosZ_normalized);
        sensor.AddObservation(turtleRotation_normalized);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Move the agent using the action
        MoveAgent(actions.DiscreteActions);
        
        // Penalty given each step to encourage agent to finish task quickly.
        AddReward(-2f / MaxStep);
        
        // Update the cumulative reward after adding the step penalty.
        _cumulativeReward = GetCumulativeReward();
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        var action = act[0];
        switch (action)
        {
            // case 0: nothing
            case 1: // Move forward
                transform.position += transform.forward * _moveSpeed * Time.deltaTime;
                break;
            case 2: // Rotate Left
                transform.Rotate(0f, -_rotationSpeed * Time.deltaTime, 0f);
                break;
            case 3: // Rotate Right
                transform.Rotate(0f, _rotationSpeed * Time.deltaTime, 0f);
                break;
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Goal"))
        {
            GoalReached();
        } else if (other.gameObject.CompareTag("Wall"))
        {
            WallReached();
        }
    }

    private void GoalReached()
    {
        // Large reward for reaching the goal
        AddReward(1.0f);
        _cumulativeReward = GetCumulativeReward();
        
        EndEpisode();
        
        // Swap ground material for a bit to indicate we scored.
        StartCoroutine(GoalScoredSwapGroundMaterial(Color.green, 0.5f));
        
        m_statsRecorder.Add("Goal/Correct", 1, StatAggregationMethod.Sum);
    }
    
    private void WallReached()
    {
        // Large reward for reaching the goal
        AddReward(-1.0f);
        _cumulativeReward = GetCumulativeReward();
        
        EndEpisode();
        
        // Swap ground material for a bit to indicate we scored.
        StartCoroutine(GoalScoredSwapGroundMaterial(Color.red, 0.5f));
        
        m_statsRecorder.Add("Goal/Wrong", 1, StatAggregationMethod.Sum);
    }
    
    IEnumerator GoalScoredSwapGroundMaterial(Color color, float time)
    {
        _groundRenderer.material.color = color;
        yield return new WaitForSeconds(time); // Wait for 2 sec
        _groundRenderer.material.color = Color.gray;
    }
}
