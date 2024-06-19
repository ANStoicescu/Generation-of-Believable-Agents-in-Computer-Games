using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraControl : MonoBehaviour
{
    float moveSpeed = 250.0f;
    float rotationSpeed = 500.0f;
    float zoomSpeed = 10.0f; // Speed of zoom effect
    float minFov = 10.0f; // Minimum field of view
    float maxFov = 80.0f; // Maximum field of view

    [SerializeField]
    Camera mainCamera;

    void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButton(0))
                MoveCamera();
            if (Input.GetMouseButton(1))
                RotateCamera();
            if (Math.Abs(Input.GetAxis("Mouse ScrollWheel")) > 0)
                ZoomCamera();
        }
    }

    private void RotateCamera()
    {
        float axisXInput = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        float axisYInput = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
        if (Math.Abs(axisXInput) > 3 || Math.Abs(axisXInput) > 3)
            return;
        
        transform.Rotate(Vector3.up, -axisXInput, Space.World);
        transform.Rotate(Vector3.left, -axisYInput);
    }
    
    private void MoveCamera()
    {
        float axisXInput = Input.GetAxis("Mouse X") * moveSpeed * Time.deltaTime;
        float axisYInput = Input.GetAxis("Mouse Y") * moveSpeed * Time.deltaTime;
        if (Math.Abs(axisXInput) > 3 || Math.Abs(axisXInput) > 3)
            return;
        
        Vector3 movement = (-axisXInput * transform.right + -axisYInput * transform.forward);
        movement.y = 0;

        transform.Translate(movement, Space.World);
    }
    
    private void ZoomCamera()
    {
        float scrollAmount = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        mainCamera.fieldOfView = Mathf.Clamp(mainCamera.fieldOfView - scrollAmount, minFov, maxFov);
    }
}
