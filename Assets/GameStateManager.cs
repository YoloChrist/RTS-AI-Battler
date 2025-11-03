using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UnitPosition
{
    public float x;
    public float y;
    public float z;

    public UnitPosition(Vector3 position)
    {
        x = position.x;
        y = position.y;
        z = position.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
}

[Serializable]
public class MovementCommand
{
    public UnitPosition targetPosition;
    public float speed;
    public bool isActive;

    public MovementCommand(Vector3 target, float moveSpeed)
    {
        targetPosition = new UnitPosition(target);
        speed = moveSpeed;
        isActive = true;
    }
}

[Serializable]
public class AttackCommand
{
    public string targetUnitId;
    public float attackRange;
    public bool isActive;

    public AttackCommand(string target, float range)
    {
        targetUnitId = target;
        attackRange = range;
        isActive = true;
    }
}

[Serializable]
public class UnitData
{
    public string unitId;
    public string unitType; // "player" or "enemy"
    public string unitName;
    public UnitPosition currentPosition;
    public MovementCommand movementCommand;
    public AttackCommand attackCommand;
    public float health;
    public float maxHealth;
    public float moveSpeed;
    public float attackSpeed;

    public UnitData(string _id, string _type, string _name, Vector3 _position, float _hp, float _maxHp, float _speed, float _attackSpeed)
    {
        unitId = _id;
        unitType = _type;
        unitName = _name;
        currentPosition = new UnitPosition(_position);
        health = _hp;
        maxHealth = _maxHp;
        moveSpeed = _speed;
        attackSpeed = _attackSpeed;

    }
}

[Serializable]
public class GameStateData
{
    public List <UnitData> units = new List<UnitData>();
    public float gameTime;
}

public class GameStateManager : MonoBehaviour
{
    private GameStateData gameState = new GameStateData();
    private Dictionary<string, UnitData> unitLookup = new Dictionary<string, UnitData>();
    private Dictionary<string, Unit> unitReferences = new Dictionary<string, Unit>();
    private GameStateSerializer serializer;

    public static GameStateManager instance;

    private float saveTimer = 0f;
    [SerializeField] private float saveInterval = 10f;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }

        serializer = new GameStateSerializer(Application.persistentDataPath + "/gamestate.json");
        Unit.OnUnitSpawned += AddUnitEventHandler;
        Unit.OnUnitDestroyed += OnUnitDestroyedHandler;
    }

    private void OnDestroy()
    {
        Unit.OnUnitSpawned -= AddUnitEventHandler;
        Unit.OnUnitDestroyed -= OnUnitDestroyedHandler;
    }


    void Start()
    {
        gameState.gameTime = 0f;
    }

    private void Update()
    {
        gameState.gameTime += Time.deltaTime;
        //SaveGameState();
        saveTimer += Time.deltaTime;
        if (saveTimer >= saveInterval)
        {
            SaveGameState();
            saveTimer = 0f;
        }
            
    }

    public void AddUnitEventHandler(Unit unit)
    {
        string id = unit.UnitID;
        
        UnitData unitData = new UnitData(
            id,
            unit.UnitType,
            unit.UnitName,
            unit.Position,
            unit.CurrentHealth,
            unit.MaxHealth,
            unit.MovementSpeed,
            unit.AttackSpeed
        );
        
        gameState.units.Add(unitData);
        unitLookup[id] = unitData;
        unitReferences[id] = unit;
    }

    private void OnUnitDestroyedHandler(Unit unit)
    {
        if (unit != null)
        {
            RemoveUnit(unit.UnitID);
        }
    }

    public void RemoveUnit(string unitId)
    {
        if (unitLookup.TryGetValue(unitId, out UnitData unit))
        {
            gameState.units.Remove(unit);
            unitLookup.Remove(unitId);
            unitReferences.Remove(unitId);
        }
    }

    public void UpdateUnitPosition(string unitId, Vector3 newPosition)
    {
        if (unitLookup.TryGetValue(unitId, out UnitData unit))
        {
            unit.currentPosition = new UnitPosition(newPosition);
        }
    }

    public void SetAttackCommand(string unitId, string targetUnitId, float range)
    {
        if (unitLookup.TryGetValue(unitId, out UnitData unit))
        {
            Debug.Log("Setting attack command for unit " + unitId + " to target " + targetUnitId);
            unit.attackCommand = new AttackCommand(targetUnitId, range);
        }
        else
        {
            Debug.LogWarning("Unit with ID " + unitId + " not found for attack command.");
        }
    }

    public UnitData GetUnit(string unitId)
    {
        unitLookup.TryGetValue(unitId, out UnitData unit);
        return unit;
    }

    public List<UnitData> GetPlayerUnits()
    {
        return gameState.units.FindAll(unit => unit.unitType == "player");
    }

    public List<UnitData> GetEnemyUnits()
    {
        return gameState.units.FindAll(unit => unit.unitType == "enemy");
    }

    public void SaveGameState()
    {
        SyncAllUnitsFromReferences();
        serializer.Save(gameState);
    }
    
    public void LoadGameState()
    {
        gameState = serializer.Load();
        RebuildLookup();
    }

    private void RebuildLookup()
    {
        unitLookup.Clear();
        foreach (var unit in gameState.units)
        {
            unitLookup[unit.unitId] = unit;
        }
    }

    private void SyncAllUnitsFromReferences()
    {
        // Use a temporary list to collect IDs of destroyed units
        List<string> unitsToRemove = new List<string>();
        foreach (var kvp in unitReferences)
        {
            string id = kvp.Key;
            Unit unit = kvp.Value;
            
            if (unit == null)
            {
                unitsToRemove.Add(id);
                continue;
            }
            
            if (unitLookup.TryGetValue(id, out UnitData data))
            {
                data.currentPosition = new UnitPosition(unit.Position);
                data.health = unit.CurrentHealth;
                
                // Sync attack target
                TargetSensor targetSensor = unit.GetComponent<TargetSensor>();
                if (targetSensor != null && targetSensor.targetToAttack != null)
                {
                    Unit targetUnit = targetSensor.targetToAttack.GetComponent<Unit>();
                    if (targetUnit != null)
                    {
                        UnitRuntimeStats stats = unit.GetComponent<UnitRuntimeStats>();
                        float range = stats != null ? stats.AttackRange : 5f;
                        data.attackCommand = new AttackCommand(targetUnit.UnitID, range);
                    }
                }
                else
                {
                    data.attackCommand = null; // No active attack target
                }
                
                // Sync movement target
                UnitMovement movement = unit.GetComponent<UnitMovement>();
                if (movement!= null && movement.isCommanded)
                {
                    data.movementCommand = new MovementCommand(movement.TargetDestination, data.moveSpeed);
                }
                else
                {
                    data.movementCommand = null;
                }
            }
        }

        foreach (var id in unitsToRemove)
        {
            RemoveUnit(id);
        }
    }
}