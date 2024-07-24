using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    private GameObject currentPrefabInstance;
    private Camera mainCamera;
    public LayerMask mask;
    private Vector3 prefabDefaultPosition = new Vector3(0, 0.25f, 0);
    private Material[] defaultMaterials;
    public Material invalidMaterial;
    private bool isPlaceable;
    public string[] tagsToCheck;

    private Vector3 previousPrefabPosition;
    private bool isMoveCurrent = false;

    public PrefabOptions prefabOptions;
    private string selectedPrefabID;
    //store resource amount needed
    private int rockAmount;
    private int waterAmount;
    private int woodAmount;
    void Start()
    {
        mainCamera = Camera.main;
    }
    public void SpawnPrefab(GameObject prefab, string id = null)
    {
        if (currentPrefabInstance != null)
        {
            Destroy(currentPrefabInstance);
        }
        currentPrefabInstance = Instantiate(prefab);
        //NPCSpawner npc = currentPrefabInstance.GetComponent<NPCSpawner>();
        //npc.SetBuildingID(currentPrefabInstance);
        StoreDefaultMaterials();
        //assign the ID to the prefab
        if (!string.IsNullOrEmpty(id))
        {
            selectedPrefabID = id;
            var identifier = currentPrefabInstance.GetComponent<BuildingIdentifier>();
            if (identifier == null)
            {
                identifier = currentPrefabInstance.AddComponent<BuildingIdentifier>();
            }
            identifier.prefabID = id;
        }
        else
        {
            var identifier = currentPrefabInstance.GetComponent<BuildingIdentifier>();
            if (identifier != null)
            {
                selectedPrefabID = identifier.prefabID;
            }
        }
        isMoveCurrent = false;
        //move prefab 
        StartCoroutine(MovePrefabWithMouse());
    }
    private void StoreDefaultMaterials()
    {
        Renderer[] renderers = currentPrefabInstance.GetComponentsInChildren<Renderer>();
        List<Material> materials = new List<Material>();

        foreach (Renderer renderer in renderers)
        {
            materials.AddRange(renderer.materials);
        }
        defaultMaterials = materials.ToArray();
    }
    private IEnumerator MovePrefabWithMouse()
    {
        while (currentPrefabInstance != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                //check clicking to the UI canvas
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    HandleInvalidPlacePrefab();
                    //Destroy(currentPrefabInstance);
                    //currentPrefabInstance = null;
                    yield break;
                }
                //check mouse outside floor and raycast doesn't hit anything
                if (isPlaceable)
                {
                    if (!isMoveCurrent)
                    {
                        PlaceNewPrefab();
                    }
                    else
                    {
                        PlaceMovedPrefab();
                    }
                    ////start spawn npc
                    //NPCSpawner npc = currentPrefabInstance.GetComponent<NPCSpawner>();
                    //npc.SetBuildingID(currentPrefabInstance);
                    //npc.SetNPCAmountDefault();
                    //if (npc != null)
                    //{
                    //    npc.SpawnNPC();
                    //}
                    ////place prefab sync server
                    //PlacePrefab();
                }
                else
                {
                    HandleInvalidPlacePrefab();
                    //Destroy(currentPrefabInstance);
                    //currentPrefabInstance = null;
                }
                yield break;
            }
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
            {
                currentPrefabInstance.transform.position = hit.point;
                CheckPlacement();
            }
            //check mouse outside floor and raycast doesn't hit anything
            else
            {
                currentPrefabInstance.transform.position = prefabDefaultPosition;
                isPlaceable = false;
                SetInvalidMaterial(invalidMaterial);
            }
            yield return null;
        }
    }
    public void MovePrefab(GameObject prefab)
    {
        var identifier = prefab.GetComponent<BuildingIdentifier>();
        if (identifier != null)
        {
            selectedPrefabID = identifier.prefabID;
        }
        isMoveCurrent = true;
        previousPrefabPosition = prefab.transform.position;
        currentPrefabInstance = prefab;
        StoreDefaultMaterials();
        StartCoroutine(MovePrefabWithMouse());
    }
    private void Update()
    {
        //detect click
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay (Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray,out hit, Mathf.Infinity))
            {
                GameObject hitObject = hit.collider.gameObject;
                if (hitObject.CompareTag("PlaceBuilding"))
                {
                    prefabOptions.ShowOptions(hitObject);
                }
                else if (hitObject.GetComponent<PrefabOptions>() == null)
                {
                    prefabOptions.HideOptions();
                }
            }
            else
            {
                prefabOptions.HideOptions();
            }
        }
    }
    private void PlaceNewPrefab()
    {
        NPCSpawner npc = currentPrefabInstance.GetComponent<NPCSpawner>();
        npc.SetBuildingID(currentPrefabInstance);
        npc.SetNPCAmountDefault();
        if (npc != null)
        {
            npc.SpawnNPC();
        }
        PlacePrefab(true);
    }
    private void PlaceMovedPrefab()
    {
        PlacePrefab(false);
    }
    private void PlacePrefab(bool consumeResource)
    {
        if (isPlaceable)
        {
            Vector3 position = currentPrefabInstance.transform.position;
            string buildingName = currentPrefabInstance.name.Replace("(Clone)", "").Trim();

            //post building data to the server
            if (!string.IsNullOrEmpty(selectedPrefabID))
            {
                NPCSpawner npcSpawner = currentPrefabInstance.GetComponent<NPCSpawner>();
                int remainingAmountNPC = npcSpawner != null ? npcSpawner.remainingAmountNPC : 0;
                ServerTalker.Instance.SaveBuildingData(buildingName, position, selectedPrefabID, remainingAmountNPC);
                //ServerTalker.Instance.SaveBuildingData(buildingName, position, selectedPrefabID);
                if (consumeResource)
                {
                    ResourceManager.Instance.ConsumeResource(rockAmount, waterAmount, woodAmount);
                }
                
                //ServerTalker.Instance.UpdateBuildingData(selectedPrefabID, position);
                //ServerTalker.Instance.PostBuildingData(buildingName, position);
            }
            else
            {
                Debug.LogError("Prefab ID is null or empty!");
                //ServerTalker.Instance.PostBuildingData(buildingName, position, selectedPrefabID);
                //ServerTalker.Instance.UpdateBuildingData(selectedPrefabID, position);
            }
            currentPrefabInstance = null;
        }
    }
    private void CheckPlacement()
    {
        Collider[] colliders = Physics.OverlapBox(currentPrefabInstance.transform.position, 
            currentPrefabInstance.transform.localScale / 2);
        foreach (var collider in colliders)
        {
            if (collider.gameObject != currentPrefabInstance && CompareTags(collider.gameObject))
            {
                isPlaceable = false;
                Debug.Log($"222colliding with {collider.name}");
                //material invalid
                SetInvalidMaterial(invalidMaterial);
                return;
            }
        }
        isPlaceable = true;
        //material default
        SetDefaultMaterial(defaultMaterials);
    }
    private bool CompareTags(GameObject obj)
    {
        foreach (string tag in tagsToCheck)
        {
            if (obj.CompareTag(tag))
            {
                return true;
            }
        }
        return false;
    }
    private void SetInvalidMaterial(Material material)
    {
        Renderer[] renderers = currentPrefabInstance.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = new Material[renderer.materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = material;
            }
            renderer.materials = materials;
        }
    }
    private void SetDefaultMaterial(Material[] materials)
    {
        Renderer[] renderers = currentPrefabInstance.GetComponentsInChildren<Renderer>();
        int index = 0;
        foreach (Renderer renderer in renderers)
        {
            Material[] rendererMaterials = new Material[renderer.materials.Length];
            for (int i = 0; i < rendererMaterials.Length; i++)
            {
                rendererMaterials[i] = materials[index++];
            }
            renderer.materials = rendererMaterials;
        }
    }
    public void SetResourceAmounts(int rock, int water, int wood)
    {
        rockAmount = rock;
        waterAmount = water;
        woodAmount = wood;
    }
    private void OnDisable()
    {
        if (currentPrefabInstance != null)
        {
            Destroy(currentPrefabInstance);
        }
    }
    private void DestroyAllBuildings()
    {
        GameObject[] buildings = GameObject.FindGameObjectsWithTag("PlaceBuilding");
        foreach (GameObject building in buildings)
        {
            Destroy(building.gameObject);
        }
    }

    private void DestroyAllNPCs()
    {
        GameObject[] NPCs = GameObject.FindGameObjectsWithTag("NPC");
        foreach (GameObject npc in NPCs)
        {
            Destroy(npc.gameObject);
        }
    }
    public void DestroyCurrentAssets()
    {
        DestroyAllBuildings();
        DestroyAllNPCs();
    }
    private void HandleInvalidPlacePrefab()
    {
        if (currentPrefabInstance != null)
        {
            if (isMoveCurrent)
            {
                currentPrefabInstance.transform.position = previousPrefabPosition;
                SetDefaultMaterial(defaultMaterials);
            }
            else
            {
                Destroy(currentPrefabInstance);
            }
        }
        currentPrefabInstance = null;
        prefabOptions.gameObject.SetActive(false);
    }
}
