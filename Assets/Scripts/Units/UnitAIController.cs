using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(UnitRuntimeStats))]
public class UnitAIController : MonoBehaviour
{
    public float updateInterval = 0.2f;

    private NavMeshAgent agent;
    private float lastUpdateTime;
    private TargetSensor targetSensor;
    private UnitMovement unitMovement;
    private UnitCombat combat;
    private UnitRuntimeStats _stats;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        targetSensor = GetComponent<TargetSensor>();
        unitMovement = GetComponent<UnitMovement>();
        combat = GetComponent<UnitCombat>();
        _stats = GetComponent<UnitRuntimeStats>();
    }

    void Update()
    {
        if (unitMovement != null && unitMovement.isCommanded)
        {
            return;
        }

        if (targetSensor == null || targetSensor.targetToAttack == null)
        {
            return;
        }

        Transform targetToAttack = targetSensor.targetToAttack;

        bool inRange = combat != null ? combat.InRange(targetToAttack)
                                      : Vector3.Distance(transform.position, targetToAttack.position) <= _stats.AttackRange;

        // Move while out of range
        if (!inRange && Time.time - lastUpdateTime > updateInterval)
        {
            if (agent != null)
            {
                agent.SetDestination(targetToAttack.position);
            }
            lastUpdateTime = Time.time;
        }
        else if (inRange && agent != null && agent.hasPath)
        {
            // Stop pathing when in range to avoid jitter
            agent.ResetPath();
        }

        // Face target
        Vector3 lookDir = (targetToAttack.position - transform.position).normalized;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(lookDir);

        // Attack when in range
        if (inRange)
        {
            Attack();
        }
    }

    private void Attack()
    {
        if (combat == null || targetSensor == null) return;
        combat.TryAttack(targetSensor.targetToAttack);
    }

    // Make AI delegate target ownership to TargetSensor
    public void SetTarget(Transform target)
    {
        if (targetSensor != null)
            targetSensor.targetToAttack = target;
    }

    public void ClearTarget()
    {
        if (targetSensor != null)
            targetSensor.targetToAttack = null;

        if (agent != null)
            agent.ResetPath();
    }
}
