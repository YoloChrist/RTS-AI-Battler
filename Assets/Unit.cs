using UnityEngine;

public class Unit : MonoBehaviour
{
    void Start()
    {
        UnitSelectionManager.instance.playerUnits.Add(this.gameObject); // Add this unit to the list of player units
    }

    private void OnDestroy()
    {
        UnitSelectionManager.instance.playerUnits.Remove(this.gameObject); // Remove this unit from the list of player units
    }
}
