using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ArrowHelperTest : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject arrow;
    public void OnBeginDrag(PointerEventData pointerEventData)
    {
        Cursor.visible = false;

    }
    public void OnDrag(PointerEventData pointerEventData)
    {
        arrow.GetComponent<ArrowScript>().DrawArrow(transform.Find("ArrowOrigin").position);
    }
    public void OnEndDrag(PointerEventData pointerEventData)
    {
        arrow.GetComponent<ArrowScript>().HideArrow();
        Cursor.visible = true;

    }
}
