using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public class UnitAIController : MonoBehaviour
{
    public Transform targetToAttack;
    
    public float attackingDistance = 2.0f;
    public float updateInterval = 0.2f;

    private NavMeshAgent agent;
    private float lastUpdateTime;
    private AttackController attackController;
    private UnitMovement unitMovement;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        attackController = GetComponent<AttackController>();
        unitMovement = GetComponent<UnitMovement>();
    }


    void Update()
    {
        if (unitMovement != null && unitMovement.isCommanded)
            return;

        if (attackController == null || attackController.targetToAttack == null)
            return;

        Transform targetToAttack = attackController.targetToAttack;
        float distance = Vector3.Distance(transform.position, targetToAttack.position);

        //Update path at intervals

        if (Time.time - lastUpdateTime > updateInterval)
        {
            agent.SetDestination(targetToAttack.position);
            lastUpdateTime = Time.time;
        }

        // Face target
        Vector3 lookDir = (targetToAttack.position - transform.position).normalized;
        lookDir.y = 0;
        if (lookDir !=  Vector3.zero)
            transform.rotation = Quaternion.LookRotation(lookDir);

        if (distance < attackingDistance) // attack logic
        {
            Attack();
        }
    }

    private void Attack()
    {
        throw new NotImplementedException();
    }

    public void SetTarget(Transform target)
    {
        targetToAttack = target;
    }

    public void ClearTarget()
    {
        targetToAttack = null;
        agent.ResetPath();
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.CompareTag("Enemy") && targetToAttack == null)
    //    {
    //        targetToAttack = other.transform;
    //    }
    //}
    //
    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.CompareTag("Enemy") && targetToAttack != null)
    //        targetToAttack = null;
    //}
}
