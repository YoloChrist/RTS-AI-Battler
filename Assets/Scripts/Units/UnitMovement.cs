using UnityEngine;
using UnityEngine.AI;

public class UnitMovement : MonoBehaviour
{
    private Camera cam;
    private NavMeshAgent agent;
    public LayerMask ground;

    public bool isCommanded;

    private void Start()
    {
        cam = Camera.main;
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (agent.hasPath == false || agent.remainingDistance <= agent.stoppingDistance)
        {
            isCommanded = false;
        }
    }

    public void IssueMove(Vector3 destination)
    {
        if (agent == null) return;
        agent.SetDestination(destination);
        isCommanded = true;
    }
}