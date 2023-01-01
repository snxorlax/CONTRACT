using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[ExecuteInEditMode]
public class ArrowHelperTest : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject arrow;
    public void OnBeginDrag(PointerEventData pointerEventData)
    {
        Cursor.visible = false;

    }
    public void OnDrag(PointerEventData pointerEventData)
    {
        arrow.GetComponent<ArrowScript>().DrawArrow(transform.position);
    }
    public void OnEndDrag(PointerEventData pointerEventData)
    {
        // arrow.GetComponent<ArrowScript>().HideArrow();
        Cursor.visible = true;

    }
}
