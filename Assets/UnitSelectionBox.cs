using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSelectionBox : MonoBehaviour
{
    public RectTransform selectionBox; // Reference to the UI element representing the selection box
    private Vector2 startPos; // Starting position of the mouse when the selection begins

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            startPos = Mouse.current.position.ReadValue();
            selectionBox.gameObject.SetActive(true);
        }
        else if (Mouse.current.leftButton.isPressed)
        {
            UpdateSelectionBox(Mouse.current.position.ReadValue()); // Update the selection box size and position
            if (selectionBox.rect.width > 0 || selectionBox.rect.height > 0)
            {
                if (!Keyboard.current.leftShiftKey.isPressed) // If left shift is not pressed, deselect all previously selected units
                    UnitSelectionManager.instance.DeselectAll();
                CountUnitsInSelection(); // Count and select units within the selection box
            }
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            CountUnitsInSelection();
            OnDisable(); // Hide the selection box when the mouse button is released
        }

    }

    private void CountUnitsInSelection() // Check which units are within the selection box
    {
        Vector2 min = selectionBox.anchoredPosition - (selectionBox.sizeDelta / 2);
        Vector2 max = selectionBox.anchoredPosition + (selectionBox.sizeDelta / 2);

        foreach(var unit in UnitSelectionManager.instance.playerUnits)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(unit.transform.position);

            if (screenPos.x > min.x && screenPos.x < max.x && screenPos.y > min.y && screenPos.y < max.y)
            {
                UnitSelectionManager.instance.DragSelect(unit);
            }
        }
    }

    private void UpdateSelectionBox(Vector2 mousePos) // Update the size and position of the selection box based on mouse movement
    {
        if (!selectionBox.gameObject.activeInHierarchy)
        {
            selectionBox.gameObject.SetActive(true);
        }

        float width = mousePos.x - startPos.x;
        float height = mousePos.y - startPos.y;

        selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));
        selectionBox.anchoredPosition = startPos + new Vector2(width / 2, height / 2);
    }

    private void OnDisable() // Hide the selection box
    {
        selectionBox.gameObject.SetActive(false);
    }
}
