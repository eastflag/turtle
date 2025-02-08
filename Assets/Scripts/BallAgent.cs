using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class BallAgent : Agent
{
    private Rigidbody ballRigidbody; // 볼의 리지드바디
    public GameObject pivotTransform; // 위치의 기준점
    public GameObject target; // 목표 아이템
    public float moveForce = 1f; // 이동시킬 힘
    private bool targetEaten = false;  // 목표를 먹었는지
    private bool dead = false; // 사망상태

    protected override void Awake()
    {
        ballRigidbody = GetComponent<Rigidbody>();
    }

    public override void Initialize()
    {
        ResetTarget();
        ResetBall();
    }

    private void ResetTarget()
    {
        targetEaten = false;
        Vector3 randomPos = new Vector3(Random.Range(-8f, 8f), 0.5f, Random.Range(-8f, 8f));
        target.transform.position = randomPos + pivotTransform.transform.position;
    }
    
    private void ResetBall()
    {
        Vector3 randomPos = new Vector3(Random.Range(-8f, 8f), 0.5f, Random.Range(-8f, 8f));
        transform.position = randomPos + pivotTransform.transform.position;
        
        dead = false;
        ballRigidbody.linearVelocity = Vector3.zero;
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(Mathf.Clamp(transform.position.x / 10f, -1f, 1f));
        sensor.AddObservation(Mathf.Clamp(transform.position.z / 10f, -1f, 1f));

        sensor.AddObservation(Mathf.Clamp(target.transform.position.x / 10f, -1f, 1f));
        sensor.AddObservation(Mathf.Clamp(target.transform.position.z / 10f, -1f, 1f));
        
        Vector3 distanceToTarget = target.transform.position - transform.position;
        
        // 정규화: -5 ~ +5 => -1 ~ 1
        sensor.AddObservation(Mathf.Clamp(distanceToTarget.x / 10f, -1f, 1f));
        sensor.AddObservation(Mathf.Clamp(distanceToTarget.z / 10f, -1f, 1f));
        
        // Vector3 relativePos = transform.position - pivotTransform.transform.position;
        
        // 정규화: -5 ~ +5 => -1 ~ 1
        // sensor.AddObservation(Mathf.Clamp(relativePos.x / 10f, -1f, 1f));
        // sensor.AddObservation(Mathf.Clamp(relativePos.z / 10f, -1f, 1f));
        
        // 정규화
        // sensor.AddObservation(Mathf.Clamp(ballRigidbody.linearVelocity.x / 10f, -1f, 1f));
        // sensor.AddObservation(Mathf.Clamp(ballRigidbody.linearVelocity.z / 10f, -1f, 1f));
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Debug.Log("OnActionReceived DiscreteActions: " + actions.DiscreteActions[0]);
        
        AddReward(-0.001f);

        // float horizontalInput = actions.ContinuousActions[0];
        // float verticalInput = actions.ContinuousActions[1];
        // Debug.Log("OnActionReceived ContinuousActions: " + horizontalInput + " " + verticalInput);
        
        if (targetEaten)
        {
            Debug.Log("targetEaten");
            AddReward(1.0f);
            EndEpisode();
        } else if (dead)
        {
            Debug.Log("dead");
            AddReward(-1.0f);
            EndEpisode();
        }
        
        MoveAgent(actions.DiscreteActions);
    }
    
    public void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var action = act[0];

        switch (action)
        {
            case 1:
                dirToGo = transform.forward * 1f;
                break;
            case 2:
                dirToGo = transform.forward * -1f;
                break;
            case 3:
                rotateDir = transform.up * 1f;
                break;
            case 4:
                rotateDir = transform.up * -1f;
                break;
            case 5:
                dirToGo = transform.right * -0.75f;
                break;
            case 6:
                dirToGo = transform.right * 0.75f;
                break;
        }
        transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f);
        ballRigidbody.AddForce(dirToGo * moveForce,
            ForceMode.VelocityChange);
    }
    
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("dead"))
        {
            dead = true;
        } else if (col.gameObject.CompareTag("goal"))
        {
            targetEaten = true;
        }
    }

    public override void OnEpisodeBegin()
    {
        ResetTarget();
        ResetBall();
    }
}
