using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ExitGraveyard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool hover;
    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && hover)
        {
            transform.parent.SetAsFirstSibling();
        }
    }
    public virtual void OnPointerEnter(PointerEventData pointer)
    {
        hover = true;
    }
    public virtual void OnPointerExit(PointerEventData pointer)
    {

        hover = false;
    }
}
