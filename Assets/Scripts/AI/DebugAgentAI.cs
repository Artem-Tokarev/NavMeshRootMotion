using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugAgentAI : MonoBehaviour
{
    Camera camera_;
    Vector3 targetPosition;
    public AgentAI AgentAI;

    void Start()
    {
        camera_ = GetComponent<Camera>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = camera_.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                targetPosition = hit.point;

                AgentAI.GoTo(targetPosition, StateAI.Run);
            }
        }
    }

    void OnDrawGizmos()
    {
        Debug.DrawLine(targetPosition, targetPosition + Vector3.up * 5, Color.red);
    }
}