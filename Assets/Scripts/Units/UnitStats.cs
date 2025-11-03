using UnityEngine;

[CreateAssetMenu(fileName = "UnitStats", menuName = "Game/Units/Unit Stats")]
public class UnitStats : ScriptableObject
{
    [Header("Identity")]
    public string displayName;

    [Header("Combat")]
    public int baseHealth = 100;
    public int baseAttack = 10;
    public int baseDefense = 5;
    public float baseAttackRange = 1;
    public float baseAttackSpeed = 3;
    public float baseMovementSpeed = 3.5f;
}