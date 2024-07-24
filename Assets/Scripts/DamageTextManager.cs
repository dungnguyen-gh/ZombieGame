using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamageTextManager : MonoBehaviour
{
    public static DamageTextManager instance;
    public GameObject damageTextPrefab;
    public Transform damageTextContainer;
    private void Awake()
    {
        instance = this;
    }
    public void ShowDamageText(float damage, Vector3 position)
    {
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(position);
        GameObject damageTextObject = Instantiate(damageTextPrefab, damageTextContainer);
        damageTextObject.SetActive(true);
        RectTransform rectTransform = damageTextObject.GetComponent<RectTransform>();
        rectTransform.position = screenPosition;

        TMP_Text damageText = damageTextObject.GetComponent<TMP_Text>();
        damageText.text = Mathf.RoundToInt(damage).ToString();
        // Set color based on damage value
        if (damage >= 60)
        {
            damageText.color = Color.red; // Red color for damage >= 50
        }
        else
        {
            damageText.color = Color.white; // Default color for lower damage
        }
        // Animate the damage text using LeanTween
        Vector2 endPosition = rectTransform.anchoredPosition + new Vector2(0, 80);

        LeanTween.move(rectTransform, endPosition, 1f).setEase(LeanTweenType.easeOutQuad);
        LeanTween.alphaText(rectTransform, 0, 1f).setEase(LeanTweenType.easeOutQuad).setOnComplete(() => Destroy(damageTextObject));
    }
}
