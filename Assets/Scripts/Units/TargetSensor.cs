using System.Runtime.CompilerServices;
using UnityEngine;

public class TargetSensor : MonoBehaviour
{
    public Transform targetToAttack;
    private UnitRuntimeStats _targetStats;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(checkIfOppositeTag()) && targetToAttack == null)
            SetTarget(other.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(checkIfOppositeTag()) && targetToAttack == other.transform)
            ClearTarget();
    }

    private string checkIfOppositeTag()
    {
        string oppositeTag = gameObject.CompareTag("Player") ? "Enemy" :
             gameObject.CompareTag("Enemy") ? "Player" : "";
        return oppositeTag;
    }

    public void SetTarget(Transform target)
    {
        // Unsubscribe from previous
        ClearTarget();

        targetToAttack = target;

        if (targetToAttack != null)
        {
            // Subscribe to target death
            _targetStats = targetToAttack.GetComponentInParent<UnitRuntimeStats>();
            if (_targetStats != null)
            {
                _targetStats.Died += OnTargetDied;
            }
        }
    }

    public void ClearTarget()
    {
        if (_targetStats != null)
        {
            _targetStats.Died -= OnTargetDied;
            _targetStats = null;
        }

        targetToAttack = null;
    }

    private void OnTargetDied(UnitRuntimeStats dead)
    {
        ClearTarget();
    }

    private void OnDestroy()
    {
        ClearTarget();
    }
}