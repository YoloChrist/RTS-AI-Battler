using System;
using UnityEngine;

public class UnitRuntimeStats : MonoBehaviour, IDamageable
{
    [Tooltip("Authored base stats for this unit type.")]
    public UnitStats definition;

    [NonSerialized] public int MaxHealth;
    [NonSerialized] public int CurrentHealth;
    [NonSerialized] public int Attack;
    [NonSerialized] public int Defense;
    [NonSerialized] public float AttackRange;
    [NonSerialized] public float AttackSpeed;
    [NonSerialized] public float MovementSpeed;

    public event Action<UnitRuntimeStats> Died;
    public event Action<int> HealthChanged;
    
    void Awake()
    {
        if (definition != null)
        {
            MaxHealth = definition.baseHealth;
            CurrentHealth = MaxHealth;
            Attack = definition.baseAttack;
            Defense = definition.baseDefense;
            AttackRange = definition.baseAttackRange;
            AttackSpeed = definition.baseAttackSpeed;
            MovementSpeed = definition.baseMovementSpeed;
        }
        else
        {
            Debug.LogWarning($"{name}: No UnitStats definition assigned.");
            MaxHealth = CurrentHealth = 1;
            Attack = 0;
            Defense = 0;
            AttackRange = 0;
            AttackSpeed = 1;
            MovementSpeed = 1;
        }
    }

    public void ApplyDamage(int rawDamage)
    {
        int actual = Mathf.Max(1, rawDamage - Defense);
        CurrentHealth = Mathf.Max(0, CurrentHealth - actual);
        HealthChanged?.Invoke(CurrentHealth);

        if (CurrentHealth == 0)
        {
            Died?.Invoke(this);
            Destroy(gameObject); // Triggers Unit.OnDestroy which already cleans up selection.
        }
    }
}