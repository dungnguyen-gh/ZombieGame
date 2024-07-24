using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class HealthEntryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text idText;
    [SerializeField] private TMP_Text healthText;
    public void Initialize(string id, float maxHealth)
    {
        idText.text = id.Substring(0,10) + "...";
        healthText.text = id;
    }
    public void SetHealth(float health)
    {
        healthText.text = Math.Floor(health).ToString();
    }
}
