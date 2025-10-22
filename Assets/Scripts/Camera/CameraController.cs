using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class CameraController : MonoBehaviour
{
    public float moveSpeed;
    public float zoomSpeed;

    public float minZoomDist;
    public float maxZoomDist;

    private Camera cam;

    InputAction moveAction;
    InputAction zoomAction;

    void Awake()
    {
        cam = Camera.main;
    }

    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        zoomAction = InputSystem.actions.FindAction("Scroll");
    }
    void Update()
    {
        Move();
        Zoom();
    }

    private void Zoom()
    {
        float scrollInput = zoomAction.ReadValue<Vector2>().y; // Get the scroll input value
        float scrollDist = transform.position.y; // Current distance from the ground (assuming y=0 is ground level)

        if (scrollDist < minZoomDist && scrollInput > 0.0f)
        {
            return;
        }
        else if (scrollDist > maxZoomDist && scrollInput < 0.0f)
        {
            return;
        }

        transform.position += cam.transform.forward * scrollInput * zoomSpeed;
    }

    private void Move()
    {
        float xInput = moveAction.ReadValue<Vector2>().x;
        float zInput = moveAction.ReadValue<Vector2>().y;

        Vector3 moveDir = transform.forward * zInput + transform.right * xInput;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }
}
