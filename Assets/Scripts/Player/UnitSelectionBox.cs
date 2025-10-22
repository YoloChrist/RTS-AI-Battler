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
            Vector2 mousePos = Mouse.current.position.ReadValue();
            UpdateSelectionBox(mousePos);

            if (selectionBox.rect.width > 0 || selectionBox.rect.height > 0)
            {
                bool additive = Keyboard.current.leftShiftKey.isPressed; // Check if Shift key is held for additive selection
                Rect rect = GetCurrentScreenRect();
                UnitSelectionManager.instance.SelectUnitsInScreenRect(rect, additive); // Select units within the selection box
            }
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            HideSelectionBox(); // Hide the selection box when the mouse button is released
        }
    }

    private Rect GetCurrentScreenRect()
    {
        Vector2 min = selectionBox.anchoredPosition - (selectionBox.sizeDelta / 2);
        Vector2 max = selectionBox.anchoredPosition + (selectionBox.sizeDelta / 2);
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    private void UpdateSelectionBox(Vector2 mousePos)
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

    private void HideSelectionBox()
    {
        selectionBox.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        HideSelectionBox();
    }
}
