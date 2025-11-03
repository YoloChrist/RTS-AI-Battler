using System;
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour, ISelectable
{
    private UnitRuntimeStats _stats;
    private string unitID;
    private NavMeshAgent _navMeshAgent;

    public static event Action<Unit> OnUnitSpawned;
    public static event Action<Unit> OnUnitDestroyed;

    public string UnitID => unitID;
    public string UnitType => gameObject.tag;
    public string UnitName => gameObject.name;
    public Vector3 Position => transform.position;
    //public float CurrentHealth => _stats != null ? _stats.CurrentHealth : 0f;
    public float CurrentHealth { get; private set; }
    public float MaxHealth => _stats != null ? _stats.MaxHealth : 0f;
    public float MovementSpeed => _stats != null ? _stats.MovementSpeed : 0f;
    public float AttackSpeed => _stats != null ? _stats.AttackSpeed : 0f;

    void Awake()
    {
        _stats = GetComponent<UnitRuntimeStats>();
        unitID = Guid.NewGuid().ToString();
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            _navMeshAgent = agent;
            _navMeshAgent.speed = MovementSpeed;
        }
    }

    void Start()
    {
        OnUnitSpawned?.Invoke(this);
    }

    private void OnDestroy()
    {
        OnUnitDestroyed?.Invoke(this);
    }

    private void Update()
    {
        CurrentHealth = _stats.CurrentHealth;
    }

    public void OnSelected()
    {
        var highlight = GetComponentInChildren<SelectionHighlight>();
        if (highlight != null) highlight.SetVisible(true);

        var movement = GetComponent<UnitMovement>();
        if (movement != null) movement.enabled = true;
    }

    public void OnDeselected()
    {
        var highlight = GetComponentInChildren<SelectionHighlight>();
        if (highlight != null) highlight.SetVisible(false);

        var movement = GetComponent<UnitMovement>();
        if (movement != null) movement.enabled = false;
    }
}
