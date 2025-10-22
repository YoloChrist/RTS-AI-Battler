using System.Runtime.CompilerServices;
using UnityEngine;

public class TargetSensor : MonoBehaviour
{
    public Transform targetToAttack;


    private void Start()
    {

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(checkIfOppositeTag(other)) && targetToAttack == null)
        {
            targetToAttack = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {


        if (other.CompareTag(checkIfOppositeTag(other)) && targetToAttack != null)
            targetToAttack = null;
    }

    private string checkIfOppositeTag(Collider other)
    {
        string oppositeTag = gameObject.CompareTag("Player") ? "Enemy" :
             gameObject.CompareTag("Enemy") ? "Player" : "";
        return oppositeTag;
    }
}
