using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrefabOptions : MonoBehaviour
{
    public Button moveButton;
    public Button deleteButton;
    private GameObject targetPrefab;
    private PlayerController playerController;
    private Camera mainCamera;
    void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        gameObject.SetActive(false);
        mainCamera = Camera.main;
        moveButton.onClick.AddListener(MovePrefab);
        deleteButton.onClick.AddListener(DeletePrefab);
    }
    public void ShowOptions(GameObject prefab)
    {
        targetPrefab = prefab;
        transform.position = prefab.transform.position + new Vector3 (0, 5 ,-2);
        gameObject.SetActive(true);
    }
    public void HideOptions()
    {
        if (targetPrefab != null)
        {
            targetPrefab = null;
            transform.position = new Vector3(0, 3, 0);
            gameObject.SetActive(false);
        }
    }
    private void MovePrefab()
    {
        playerController.MovePrefab(targetPrefab);
        gameObject.SetActive(false);
    }
    private void DeletePrefab()
    {
        string id = targetPrefab.GetComponent<BuildingIdentifier>().prefabID; 
        ServerTalker.Instance.DeleteBuildingData(id);
        Destroy(targetPrefab);
        gameObject.SetActive(false);
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
