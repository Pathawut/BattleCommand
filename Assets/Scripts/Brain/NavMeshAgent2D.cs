using UnityEngine;
using UnityEngine.AI;

public class NavMeshAgent2D : MonoBehaviour
{
    public Transform target;

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // ปิดการหมุนแบบ 3D
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    void Update()
    {
        if (target != null)
        {
            agent.SetDestination(target.position);
        }
    }
}