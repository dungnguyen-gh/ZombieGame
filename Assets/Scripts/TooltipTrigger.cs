using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private static LTDescr delay;

    public string content;
    public string header;
    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipSystem.Show(content, header);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipSystem.Hide();
    }
    private void OnMouseEnter()
    {
        delay = LeanTween.delayedCall(0.5f, () => 
        {
            TooltipSystem.Show(content, header);
        });
    }
    private void OnMouseExit()
    {
        LeanTween.cancel(delay.uniqueId);
        TooltipSystem.Hide();
    }
    public void SetRequirements(ButtonInfo info)
    {
        info.UpdateBuildingContent();
        content = info.buildingContent;
        header = info.buildingHeader;
    }
}
