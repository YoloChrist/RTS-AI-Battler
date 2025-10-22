using UnityEngine;

public class Unit : MonoBehaviour, ISelectable
{
    void Start()
    {
        if (gameObject.CompareTag("Player"))
            UnitSelectionManager.instance.playerUnits.Add(this.gameObject);
    }

    private void OnDestroy()
    {
        if (gameObject.CompareTag("Player"))
            UnitSelectionManager.instance.playerUnits.Remove(this.gameObject);
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
