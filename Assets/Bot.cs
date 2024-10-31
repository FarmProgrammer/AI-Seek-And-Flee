using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Bot : MonoBehaviour
{
    NavMeshAgent agent;
    public GameObject target;
    Drive targetMotion;
    Vector3 wanderTarget = Vector3.zero;
    bool cooldown = false;
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

    void Wander()
    {
        float wanderRadius = 10;
        float wanderDist = 10;
        float wanderJit = 1;

        wanderTarget += new Vector3(Random.Range(-1, 1) * wanderJit, 0 , Random.Range(-1, 1) * wanderJit);
        wanderTarget.Normalize();
        wanderTarget *= wanderRadius;

        Vector3 targetLocal = wanderTarget + new Vector3(0, 0, wanderDist);
        Vector3 targetWorld = transform.InverseTransformVector(targetLocal);
        Seek(targetWorld);
    }

    void CleverHide()
    {
        float dist = Mathf.Infinity;
        Vector3 chosenSpot = Vector3.zero;
        Vector3 chosenDir = Vector3.zero;
        GameObject chosenGO = World.Instance.GetHidingSpots()[0];

        for (int i = 0; i < World.Instance.GetHidingSpots().Length; i++)
        {
            Vector3 hideDir = World.Instance.GetHidingSpots()[i].transform.position - target.transform.position;
            Vector3 hidePos = World.Instance.GetHidingSpots()[i].transform.position + hideDir.normalized * 5;

            if(Vector3.Distance(transform.position, hidePos) < dist)
            {
                chosenSpot = hidePos;
                chosenDir = hideDir;
                chosenGO = World.Instance.GetHidingSpots()[i];

                dist = Vector3.Distance(this.transform.position, hidePos);
            }
        }

        Collider hideCol = chosenGO.GetComponent<Collider>();
        Ray backRay = new Ray(chosenSpot, -chosenDir.normalized);
        RaycastHit info;
        float distance = 100;
        hideCol.Raycast(backRay, out info, distance);

        Seek(info.point + chosenDir.normalized * 5);
    }

    void Hide()
    {
        float dist = Mathf.Infinity;
        Vector3 chosenSpot = Vector3.zero;

        for (int i = 0; i < World.Instance.GetHidingSpots().Length; i++)
        {
            Vector3 hideDir = World.Instance.GetHidingSpots()[i].transform.position - target.transform.position;
            Vector3 hidePos = World.Instance.GetHidingSpots()[i].transform.position + hideDir.normalized * 5;

            if (Vector3.Distance(transform.position, hidePos) < dist)
            {
                chosenSpot = hidePos;
                dist = Vector3.Distance(this.transform.position, hidePos);
            }
        }

        Seek(chosenSpot);
    }

    bool CanSeeTarget()
    {
        RaycastHit raycastInfo;
        Vector3 rayToTarget = target.transform.position - transform.position;

        float lookAngle = Vector3.Angle(transform.forward, rayToTarget);

        if (lookAngle < 60 && Physics.Raycast(transform.position, rayToTarget, out raycastInfo))
        {
            if(raycastInfo.transform.gameObject.tag == "cop")
            {
                return true;
            }
        }
        return false;
    }

    bool CanSeeMe()
    {
        RaycastHit raycastInfo;
        Vector3 lineFromTarget = transform.position - target.transform.position;

        float lookAngle = Vector3.Angle(target.transform.forward, lineFromTarget);


        if (lookAngle < 60 && Physics.Raycast(target.transform.position, lineFromTarget, out raycastInfo))
        {
            if (raycastInfo.transform.gameObject.tag == "robber")
            {
                Debug.Log("Cop can see robber");
                return true;
            }
        }
        return false;
    }

    void BehaviousCooldown()
    {
        cooldown = false;
    }
    // Update is called once per frame
    void Update()
    {
        if (cooldown)
        {
            if (CanSeeTarget() && CanSeeMe())
            {
                CleverHide();
                cooldown = true;
                Invoke("BehaviousCooldown", 5);
            }
            else
            {
                Pursue(target.transform.position);
            }
        }
    }
}
