using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class HealthSystemManager : MonoBehaviour
{
    public static HealthSystemManager Instance;

    [SerializeField] private Transform healthContainer;
    [SerializeField] private GameObject healthEntryPrefab;
    [SerializeField] private TMP_Text playerHealthText;
    [SerializeField] private HealthBar healthBarUI;
    public Dictionary<string, HealthEntryUI> npcHealthEntries = new Dictionary<string, HealthEntryUI>();


    private float playerHealth;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    public void RegisterPlayer(float health)
    {
        playerHealth = health;
        UpdatePlayerHealthUI();
    }
    public void RegisterNPC(string id, NPCBase npc)
    {
        if (!npcHealthEntries.ContainsKey(id))
        {
            GameObject healthEntryObject = Instantiate(healthEntryPrefab, healthContainer);
            healthEntryObject.SetActive(true);
            HealthEntryUI healthEntryUI = healthEntryObject.GetComponent<HealthEntryUI>();
            healthEntryUI.Initialize(id, npc.maxHealth);
            npcHealthEntries[id] = healthEntryUI;
            //set
            npc.SetHealthEntryUI(healthEntryUI);
        }
    }
    public void UpdateNPCHealth(string id, float health)
    {
        if (npcHealthEntries.ContainsKey(id))
        {
            npcHealthEntries[id].SetHealth(health);
        }
    }
    public void UnregisterNPC(string id)
    {
        if (npcHealthEntries.ContainsKey(id))
        {
            Destroy(npcHealthEntries[id].gameObject);
            npcHealthEntries.Remove(id);
        }
    }
    private void UpdatePlayerHealthUI()
    {
        if (playerHealthText != null)
        {
            playerHealthText.text = $"{Math.Ceiling(playerHealth).ToString()}/100";
        }
        if (healthBarUI != null)
        {
            healthBarUI.SetHealth(playerHealth);
        }
    }
    public void UpdatePlayerHealth(float health)
    {
        playerHealth = health;
        UpdatePlayerHealthUI();
    }
}
