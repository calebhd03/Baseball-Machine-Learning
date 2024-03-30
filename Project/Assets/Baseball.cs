using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Baseball : MonoBehaviour
{
    public BaseballAgentController agent;
    [HideInInspector] public Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if(transform.position.y <= 0)
        {
            GiveReward();
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!other.gameObject.CompareTag("Player"))
        {
            GiveReward();
        }
    }

    void GiveReward()
    {
        agent.GiveReward(Vector3.Distance(agent.transform.localPosition, transform.localPosition) * transform.localPosition.x);
    }
}
