using System;
using UnityEngine;
using TMPro;

[Serializable]
public class ButtonInfo
{
    public Sprite buttonImage;
    public string buttonName;
    public GameObject buildingPrefab;
    public string buildingHeader;
    public int rockAmount;
    public int waterAmount;
    public int woodAmount;
    public string buildingContent;
    public int amountNPC;
    public int remainingAmountNPC;

    // Method to set buildingContent
    public void UpdateBuildingContent()
    {
        var go = buildingPrefab.GetComponent<NPCSpawner>().npcName;
        if (go != null)
        {
            buildingContent = $"This building produce {go}" +
            $"<br>Requirements: " +
            $"<br>Rock: {rockAmount} <br>Water: {waterAmount} <br>Wood: {woodAmount}";
        }
        else
        {
            buildingContent = $"Rock: {rockAmount} <br>Water: {waterAmount} <br>Wood: {woodAmount}";
        }
        
    }
}
