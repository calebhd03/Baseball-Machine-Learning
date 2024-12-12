using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class BaseballAgentController : Agent
{
    [Header("Baseball Values")]
    [SerializeField] Baseball ball;
    [SerializeField] Vector3 baseballSpawnPoint;
    [SerializeField] Vector2 baseballVelocityRange;
    [SerializeField] Vector2 baseballSpawnAngleXRange;
    [SerializeField] Vector2 baseballSpawnAngleYRange;
    [SerializeField] Vector3 baseballSpawnPointDir;

    [Header("References")]
    //[SerializeField] Transform bat;
    [SerializeField] Transform batRotObj;
    [SerializeField] Rigidbody batRB;
    [SerializeField] Rigidbody batHingeRB;
    [SerializeField] HingeJoint hinge;
    [SerializeField] Transform bat;
    [SerializeField] float batSwingSpeed; // Swing Speed Multiplier
    [SerializeField] float batMaxDistance; // Max Horizontal distance bat can extend
    [SerializeField] float batMaxHeight; // Max Vertical distance bat can extend


    // Begining of each agent life
    public override void OnEpisodeBegin()
    {
        // Set bat values
        batRotObj.localRotation = Quaternion.identity;
        batHingeRB.velocity = Vector3.zero;
        batRB.velocity = Vector3.zero;
        hinge.useMotor = true;
        batRB.transform.localRotation = Quaternion.identity;
        batRB.transform.localPosition = Vector3.back;
        bat.transform.localPosition = Vector3.zero;
        bat.gameObject.SetActive(false);
        ball.gameObject.SetActive(false);

        StartCoroutine(ResetBat());
        StartCoroutine(ThrowBall());
    }

    IEnumerator ResetBat()
    {
        yield return new WaitForSeconds(.5f);

        // Reset Bat Values
        bat.gameObject.SetActive(true);
        batRotObj.transform.localRotation = Quaternion.identity;
        batHingeRB.velocity = Vector3.zero;
        batRB.velocity = Vector3.zero;
        batRB.transform.localRotation = Quaternion.identity;
        batRB.transform.localPosition = Vector3.back;
        bat.transform.localPosition = Vector3.zero;
        hinge.useMotor = false;
    }

    // Initizalizes Baseball values
    IEnumerator ThrowBall()
    {
        yield return new WaitForSeconds(1f);


        // Set starting baseball values
        ball.transform.localPosition = baseballSpawnPoint;
        ball.transform.localRotation = Quaternion.LookRotation(baseballSpawnPointDir, ball.transform.up);
        ball.transform.localPosition += Vector3.right * UnityEngine.Random.Range(baseballSpawnAngleXRange.x, baseballSpawnAngleXRange.y);
        ball.transform.localPosition += Vector3.up * UnityEngine.Random.Range(baseballSpawnAngleYRange.x, baseballSpawnAngleYRange.y);
        ball.rb.velocity = ball.transform.forward * UnityEngine.Random.Range(baseballVelocityRange.x, baseballVelocityRange.y);

        ball.gameObject.SetActive(true);
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(ball.transform.localPosition);
        sensor.AddObservation(ball.rb.velocity);
        sensor.AddObservation(bat.localPosition);
        sensor.AddObservation(batRB.velocity);
        sensor.AddObservation(batHingeRB.velocity);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        float batDistanceSpeed = 10f;
        // Bat distance from body
        float batDistance = bat.localPosition.x + actionBuffers.ContinuousActions[0] * batDistanceSpeed * Time.deltaTime;
        batDistance = Mathf.Clamp(batDistance, -batMaxDistance, batMaxDistance);


        float batHeightSpeed = 100;
        // Bat height
        float batHeight = batRotObj.eulerAngles.z + -actionBuffers.ContinuousActions[1] * batHeightSpeed * Time.deltaTime;
        if (batHeight > 180)
        {
            batHeight -= 360;
        }
        batHeight = Mathf.Clamp(batHeight, -30, 30);

        // Apply bat movements
        bat.localPosition = new Vector3(batDistance, 0, 0);
        batRotObj.transform.localRotation = Quaternion.Euler(0, 0, batHeight);


        // Swing bat forward
        batRB.AddForce(batRB.transform.forward * actionBuffers.ContinuousActions[2] * batSwingSpeed * Time.deltaTime, ForceMode.Acceleration);

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
        ball.gameObject.SetActive(false);
        EndEpisode();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(baseballSpawnPoint, .5f);
        Gizmos.DrawRay(new Ray(baseballSpawnPoint, baseballSpawnPointDir));
    }
}
