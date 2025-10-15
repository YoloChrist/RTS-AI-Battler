using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSelectionManager : MonoBehaviour
{
    public static UnitSelectionManager instance { get; set; } // Singleton instance

    public List<GameObject> selectedUnits = new List<GameObject>(); // List to hold currently selected units
    public List<GameObject> playerUnits = new List<GameObject>(); // List to hold all player units in the scene

    InputAction clickAction;
    InputAction shiftAction;

    public LayerMask ground; // Layer mask to specify what is considered ground
    public LayerMask clickableArea; // Layer mask to specify what is considered clickable area
    public GameObject groundMarker;

    private Camera cam;

    private void Awake()
    {
        if (instance != null && instance != this) // Ensure only one instance of the singleton exists
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        clickAction = InputSystem.actions.FindAction("Click");                   
        shiftAction = InputSystem.actions.FindAction("MultiSelect");             
        cam = Camera.main; // Get the main camera reference

        clickAction.performed += OnClickPerformed; // Subscribe to the click action event
    }

    private void OnDestroy()
    {
        if (clickAction != null) // Unsubscribe from the click action event to prevent memory leaks
            clickAction.performed -= OnClickPerformed;
    }

    private void OnClickPerformed(InputAction.CallbackContext context) // Handle click action
    {
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue()); // Create a ray from the camera to the mouse position
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, clickableArea)) // Perform a raycast and check if it hits the clickableArea layer
        {
            if (shiftAction.IsPressed())
            {
                SelectMultiple(hit.collider.gameObject);
            }
            else
            {
                SelectByClick(hit.collider.gameObject);
            }
        }
        else //if (Physics.Raycast(ray, out hit, Mathf.Infinity, ground)) // Perform a raycast and check if it hits the ground layer
        {
            if (!shiftAction.IsPressed())
                DeselectAll();
        }
    }

    private void SelectMultiple(GameObject gameObject) // Select or deselect multiple units
    {
        if (!selectedUnits.Contains(gameObject))
        {
            selectedUnits.Add(gameObject);
            SelectUnit(gameObject, true);
        }
        else
        {
            SelectUnit(gameObject, false);
            selectedUnits.Remove(gameObject);
        }
    }

    public void DeselectAll() // Deselect all currently selected units
    {
        foreach (var unit in selectedUnits)
        {
            SelectUnit(unit, false);

        }

        selectedUnits.Clear();
    }

    private void SelectByClick(GameObject unit) // Select a single unit by clicking
    {
        DeselectAll();

        selectedUnits.Add(unit);
        SelectUnit(unit, true);
    }

    private void EnableUnitMovement(GameObject unit, bool shouldMove) // Enable or disable unit movement
    {
        unit.GetComponent<UnitMovement>().enabled = shouldMove;
    }

    private void TriggerSelectionHighlight(GameObject unit, bool isVisible) // Show or hide selection highlight
    {
            unit.transform.GetChild(0).gameObject.SetActive(isVisible);
    }

    public void DragSelect(GameObject unit) // Select a unit via drag selection
    {
        if (!selectedUnits.Contains(unit))
        {
            selectedUnits.Add(unit);
            SelectUnit(unit, true);
        }
    }

    private void SelectUnit(GameObject unit, bool isSelected) // Handle the selection or deselection of a unit
    {
        TriggerSelectionHighlight(unit, isSelected);
        EnableUnitMovement(unit, isSelected);
    }
}