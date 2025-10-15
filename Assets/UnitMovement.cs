using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class UnitMovement : MonoBehaviour
{
    InputAction rightClickAction; // Input action for right-clicking
    Camera cam; // Reference to the main camera
    NavMeshAgent agent; // Reference to the NavMeshAgent component
    public LayerMask ground; // Layer mask to specify what is considered ground

    private void Start()
    {
        rightClickAction = InputSystem.actions.FindAction("Right Click"); // Find the "Right Click" action from the Input System
        cam = Camera.main; // Get the main camera reference
        agent = GetComponent<NavMeshAgent>(); // Get the NavMeshAgent component reference
    }

    private void Update()
    {
        if (rightClickAction.IsPressed())
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue()); // Create a ray from the camera to the mouse position
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ground)) // Perform a raycast and check if it hits the ground layer
            {
                agent.SetDestination(hit.point); // Set the agent's destination to the ray's hit point
            }
        }
    }
}