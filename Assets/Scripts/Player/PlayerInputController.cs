using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    [SerializeField] private LayerMask ground;
    [SerializeField] private LayerMask attackable;

    private Camera cam;
    private InputAction rightClickAction;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Start()
    {
        rightClickAction = InputSystem.actions.FindAction("Right Click");
        if (rightClickAction != null)
            rightClickAction.performed += OnRightClickPerformed;
    }

    private void OnDestroy()
    {
        if (rightClickAction != null)
            rightClickAction.performed -= OnRightClickPerformed;
    }

    private void OnRightClickPerformed(InputAction.CallbackContext _)
    {
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        // Attackable takes precedence
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, attackable))
        {
            foreach (var unitObj in UnitSelectionManager.instance.selectedUnits)
            {
                var sensor = unitObj.GetComponent<TargetSensor>();
                if (sensor != null)
                    sensor.targetToAttack = hit.transform;
            }
            return;
        }

        // Otherwise, move on ground
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, ground))
        {
            foreach (var unitObj in UnitSelectionManager.instance.selectedUnits)
            {
                var sensor = unitObj.GetComponent<TargetSensor>();
                if (sensor != null) sensor.targetToAttack = null;

                var movement = unitObj.GetComponent<UnitMovement>();
                if (movement != null) movement.IssueMove(destination: hit.point);
            }
        }
    }
}