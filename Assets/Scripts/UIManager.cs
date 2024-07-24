using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIManager : MonoBehaviour
{
    public List<ButtonInfo> infoButtons;
    List<GameObject> spawnedButtons;
    [SerializeField] GameObject buttonsContainer, buttonTemplate;
    private PlayerController playerController;

    //for gameover
    public GameObject gameOverPanel;
    public Button reviveButton;
    private CharacterMovement characterMovement;
    void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        SpawnButton();
        reviveButton.onClick.AddListener(OnReviveButtonClicked);
        characterMovement = FindObjectOfType<CharacterMovement>();
        HideGameOverPanel();
    }
    public void SpawnButton()
    {
        spawnedButtons = new List<GameObject>();
        foreach (var button in infoButtons)
        {
            var b = Instantiate(buttonTemplate);
            b.transform.SetParent(buttonsContainer.transform);
            b.GetComponent<ButtonSettings>().SetData(button);
            b.GetComponent<TooltipTrigger>().SetRequirements(button);
            b.gameObject.SetActive(true);
            b.GetComponent<Button>().onClick.AddListener(() =>
            {
                OnButtonClicked(button);
                Debug.Log($"building {button.buttonName}");
            });
            spawnedButtons.Add(b);
        }
    }
    private void OnButtonClicked(ButtonInfo buttonInfo)
    {
        if (playerController != null)
        {
            //check is having enough resources to build
            if (ResourceManager.Instance.HasEnoughResources(buttonInfo.rockAmount, buttonInfo.waterAmount, buttonInfo.woodAmount))
            {
                //set resource amount inside player controller
                playerController.SetResourceAmounts(buttonInfo.rockAmount, buttonInfo.waterAmount, buttonInfo.woodAmount);
                //generate ID for prefab
                string prefabId = GenerateUniqueID();
                playerController.SpawnPrefab(buttonInfo.buildingPrefab, prefabId);
                Debug.Log("2222enough resources to build " + buttonInfo.buildingPrefab);
            }
            else
            {
                Debug.Log("2222Not enough resources to build");
            }
        }
    }
    private string GenerateUniqueID()
    {
        //generate a guid as the unique ID
        return Guid.NewGuid().ToString();
    }
    private void OnReviveButtonClicked()
    {
        characterMovement.BackToLife();
        HideGameOverPanel();
    }
    public void ShowGameOverPanel()
    {
        gameOverPanel.SetActive(true);
    }
    public void HideGameOverPanel()
    {
        gameOverPanel.SetActive(false);
    }

}
