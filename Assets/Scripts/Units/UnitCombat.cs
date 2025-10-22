using UnityEngine;

[RequireComponent(typeof(UnitRuntimeStats))]
public class UnitCombat : MonoBehaviour
{
    private float _nextAttackTime;
    private UnitRuntimeStats _stats;

    public float AttackRange => _stats.AttackRange;

    private void Awake()
    {
        _stats = GetComponent<UnitRuntimeStats>();
        if (_stats == null)
            Debug.LogWarning($"{name}: UnitRuntimeStats component not found.");
    }

    public bool InRange(Transform target)
    {
        if (target == null) return false;
        return Vector3.Distance(transform.position, target.position) <= _stats.AttackRange;
    }

    // Returns true if an attack was performed.
    public bool TryAttack(Transform target)
    {        if (target == null) return false;
        if (Time.time < _nextAttackTime) return false;
        if (!InRange(target)) return false;

        var damageable = target.GetComponentInParent<IDamageable>();
        if (damageable == null) return false;

        int damage = _stats != null ? _stats.Attack : 1;
        damageable.ApplyDamage(damage);

        _nextAttackTime = Time.time + _stats.AttackSpeed;

        return true;
    }
}