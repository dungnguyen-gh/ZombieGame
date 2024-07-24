using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSettings : MonoBehaviour
{
    public Image itemImage;
    public TMP_Text itemName;
    public void SetData(ButtonInfo info)
    {
        itemImage.sprite = info.buttonImage;
        itemName.text = info.buttonName;
    }
}
