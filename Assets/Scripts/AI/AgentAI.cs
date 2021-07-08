using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
public class AgentAI : MonoBehaviour
{
    Animator animator;
    NavMeshAgent agent;
    Vector3 targetPosition;
    Vector3 nextPointPosition;
    Transform targetPursuit;
    bool isMoving;
    bool isRotating;
    float rotationSpeed;
    StateAI beginAnimation;
    StateAI endAnimation;

    void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
    }

    public void GoTo(Vector3 target, StateAI beginAnimation, StateAI endAnimation = StateAI.Idle)
    {
        if (!isMoving)
        {
            agent.ResetPath();
            agent.SetDestination(target);
            targetPosition = new Vector3(target.x, 0, target.z);
            animator.SetTrigger(beginAnimation.ToString());
            this.endAnimation = endAnimation;
            isMoving = true;
            StartCoroutine(IsPathBuilded());
        }
        else
        {
            targetPosition = new Vector3(target.x, 0, target.z);
            agent.SetDestination(target);
        }
    }

    IEnumerator IsPathBuilded()
    {
        while (agent.pathPending)
        {
            yield return new WaitForEndOfFrame();
        }
        StartCoroutine(CheckNewPointPath());
        StartCoroutine(RotationAgentToPoint());
    }

    IEnumerator CheckNewPointPath()
    {
        while (IsNotPathFinished())
        {
            if (IsGoToNewPoint())
            {
                nextPointPosition = agent.path.corners[1];
                CalculateRotationSpeed();
            }
            yield return new WaitForEndOfFrame();
        }
        isMoving = false;
        animator.SetTrigger(endAnimation.ToString());
    }

    bool IsNotPathFinished()
    {
        Vector3 agentPos = new Vector3(transform.position.x, 0, transform.position.z);
        float distance = Vector3.Distance(targetPosition, agentPos);
        if (distance > agent.stoppingDistance)
            return true;
        else
            return false;
    }

    bool IsGoToNewPoint()
    {
        int cornersLength = agent.path.corners.Length;
        if (cornersLength > 1 && nextPointPosition != agent.path.corners[1])
        {
            isRotating = true;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void CalculateRotationSpeed()
    {
        rotationSpeed = Vector3.Angle(nextPointPosition - transform.position, transform.forward) * 2;
    }

    IEnumerator RotationAgentToPoint()
    {
        while (isMoving)
        {
            if (isRotating)
            {
                Vector3 direction = (nextPointPosition - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                if (transform.rotation != lookRotation)
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
                else
                    isRotating = false;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    public void Pursuit(Transform target, StateAI beginAnimation, StateAI endAnimation = StateAI.Idle)
    {
        if (isMoving)
        {
            targetPursuit = target;
            this.endAnimation = endAnimation;
        }
        else
        {
            targetPursuit = target;
            this.beginAnimation = beginAnimation;
            this.endAnimation = endAnimation;
            GoTo(targetPursuit.position, beginAnimation, endAnimation);
            StartCoroutine(ThePursuit());
        }
    }

    IEnumerator ThePursuit()
    {
        yield return new WaitForSeconds(0.1f);
        while (isMoving)
        {
            GoTo(targetPursuit.position, beginAnimation, endAnimation);
            yield return new WaitForSeconds(0.1f);
        }
    }

    void OnDrawGizmos()
    {
        if (agent != null)
            for (int i = 0; i < agent.path.corners.Length - 1; i++)
                Debug.DrawLine(agent.path.corners[i], agent.path.corners[i + 1], Color.red);
    }
}