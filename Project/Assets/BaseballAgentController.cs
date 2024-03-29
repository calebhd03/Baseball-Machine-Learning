using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class BaseballAgentController : Agent
{
    [SerializeField] Baseball ball;
    [SerializeField] Vector3 baseballSpawnPoint;
    [SerializeField] Vector2 baseballVelocityRange;
    [SerializeField] Vector2 baseballSpawnAngleXRange;
    [SerializeField] Vector2 baseballSpawnAngleYRange;
    [SerializeField] Vector3 baseballSpawnPointDir;

    //[SerializeField] Transform bat;
    [SerializeField] ConfigurableJoint batHinge;
    [SerializeField] Rigidbody batRB;
    [SerializeField] float batMaxSpeed;
    [SerializeField] float batSwingSpeed;

    public override void Initialize()
    {
    }

    public override void OnEpisodeBegin()
    {
        // Set starting baseball values
        ball.rb.velocity = ball.transform.forward * UnityEngine.Random.Range(baseballVelocityRange.x, baseballVelocityRange.y);
        ball.transform.localPosition = baseballSpawnPoint;
        ball.transform.rotation = Quaternion.LookRotation(baseballSpawnPointDir, ball.transform.up);
        ball.transform.localPosition += Vector3.right * UnityEngine.Random.Range(baseballSpawnAngleXRange.x, baseballSpawnAngleXRange.y);
        ball.transform.localPosition += Vector3.up * UnityEngine.Random.Range(baseballSpawnAngleYRange.x, baseballSpawnAngleYRange.y);

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        /*sensor.AddObservation(ball.transform.localPosition);
        sensor.AddObservation(ball.rb.velocity);
        sensor.AddObservation(bat.localPosition);
        sensor.AddObservation(batRB.velocity);*/
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        //Quaternion batTargetRot = new Quaternion(actionBuffers.ContinuousActions[0], actionBuffers.ContinuousActions[1], actionBuffers.ContinuousActions[2], actionBuffers.ContinuousActions[3]);
        //Vector3 targetEuler = batTargetRot.eulerAngles;
        //bat.eulerAngles = new Vector3 (targetEuler.x, 0, targetEuler.z);


        Debug.Log("0 : " + actionBuffers.ContinuousActions[0]);
        Debug.Log("1 : " + actionBuffers.ContinuousActions[1]);
        Debug.Log("2 : " + actionBuffers.ContinuousActions[2]);
        Debug.Log("Bat velocity : " + batRB.velocity);

        float batDistanceSpeed = 10f;
        // Bat distance from body
        float batDistance = Mathf.SmoothStep(batHinge.targetPosition.x, actionBuffers.ContinuousActions[0] * batDistanceSpeed, Time.deltaTime);

        // Bat height
        float batHeight = Mathf.SmoothStep(batHinge.targetPosition.y, actionBuffers.ContinuousActions[1] * batDistanceSpeed, Time.deltaTime);

        // Apply bat movements
        batHinge.targetPosition = new Vector3(batDistance, batHeight, 0);
        batHinge.anchor = Vector3.zero;

        // Swing bat forward
        batRB.AddForce(batRB.transform.forward * actionBuffers.ContinuousActions[2] * batSwingSpeed * Time.deltaTime, ForceMode.Force);
        batRB.velocity = Vector3.ClampMagnitude(batRB.velocity, batMaxSpeed);

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");


        if(Input.GetButton("Jump"))
        {
            continuousActionsOut[2] = 1;
        }
        else if(Input.GetButton("Crouch"))
        {
            continuousActionsOut[2] = -1;
        }
        else
        {
            continuousActionsOut[2] = 0;
        }
    }

    internal void GiveReward(float v)
    {
        SetReward(v);
        Debug.Log("end episode");
        EndEpisode();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(baseballSpawnPoint, .5f);
        Gizmos.DrawRay(new Ray(baseballSpawnPoint, baseballSpawnPointDir));
    }
}
