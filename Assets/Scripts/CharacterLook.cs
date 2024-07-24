using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterLook : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;

    private float xRotation = 0f;
    private bool isDragging = false;

    void Start()
    {
        // Initialize xRotation based on the current camera rotation
        Vector3 angles = transform.localEulerAngles;
        xRotation = angles.x;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))  // Right mouse button to start dragging
        {
            isDragging = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Input.GetMouseButtonUp(1))  // Release the right mouse button to stop dragging
        {
            isDragging = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (isDragging)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            playerBody.Rotate(Vector3.up * mouseX);

            // Debug logs to trace the rotation values
            Debug.Log($"xRotation: {xRotation}, mouseX: {mouseX}, mouseY: {mouseY}");
        }
    }
}
