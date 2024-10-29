using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Bot : MonoBehaviour
{
    NavMeshAgent agent;
    public GameObject target;
    Drive targetMotion;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        targetMotion = target.GetComponent<Drive>();
    }

    void Seek(Vector3 location)
    {
        agent.SetDestination(location);
    }

    void Flee(Vector3 location)
    {
        Vector3 fleeVector = location - transform.position;
        agent.SetDestination(transform.position - fleeVector);
    }

    void Pursue(Vector3 location)
    {
        Vector3 targetDir = location - this.transform.position;

        float relativeHeading = Vector3.Angle(transform.forward, transform.TransformVector(target.transform.forward));
        float toTarget = Vector3.Angle(transform.forward, transform.TransformVector(targetDir));

        if (toTarget<90 && relativeHeading<20 || targetMotion.currentSpeed < 0.01f)
        {
            Seek(location);
            return;
        }

        float lookAhead = targetDir.magnitude / (agent.speed + targetMotion.currentSpeed);

        Seek(location + target.transform.forward * lookAhead);
    }

    void Evade(Vector3 location)
    {
        Vector3 targetDir = location - this.transform.position;

        float relativeHeading = Vector3.Angle(transform.forward, transform.TransformVector(target.transform.forward));
        float toTarget = Vector3.Angle(transform.forward, transform.TransformVector(targetDir));

        if (toTarget < 90 && relativeHeading < 20 ||targetMotion.currentSpeed < 0.01f)
        {
            Flee(location);
            return;
        }

        float lookAhead = targetDir.magnitude / (agent.speed + targetMotion.currentSpeed);

        Flee(location + target.transform.forward * lookAhead);
    }

    // Update is called once per frame
    void Update()
    {
        Evade(target.transform.position);
    }
}
