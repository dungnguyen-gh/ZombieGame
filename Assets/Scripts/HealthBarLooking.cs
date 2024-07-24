using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarLooking : MonoBehaviour
{
    private Camera mainCamera;
    void Start()
    {
        mainCamera = Camera.main;
    }
    private void LookAtCamera()
    {
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.forward);
        }
    }
    private void LateUpdate()
    {
        LookAtCamera();
    }
}
