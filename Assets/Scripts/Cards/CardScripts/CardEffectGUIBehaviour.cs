using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardEffectGUIBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler
{
    public Image border;
    public int effectNumber;
    public GameObject card;
    public CardEffect cardEffect;
    public Color originalColor;
    private void Start()
    {
        border = transform.Find("Border").GetComponent<Image>();
        originalColor = border.color;
        card = transform.parent.parent.gameObject;
        cardEffect = card.GetComponent<CardDisplay>().card.cardEffect;
    }
    public virtual void OnPointerEnter(PointerEventData pointerEventData)
    {
        border.color = Color.white;
    }
    public virtual void OnPointerExit(PointerEventData pointerEventData)
    {
        border.color = originalColor;
    }
    public virtual void OnDrop(PointerEventData pointerEventData)
    {
        if (pointerEventData.dragging == card)
        {
            switch (effectNumber){
                case 0: cardEffect.ActivatedEffect_1();
                break;
                case 1: cardEffect.ActivatedEffect_2();
                break;
                case 2: cardEffect.ActivatedEffect_3();
                break;
                case 3: cardEffect.Deathwalk();
                break;
                case 4: cardEffect.Shroud();
                break;
            }
            border.color = originalColor;
            gameObject.SetActive(false);
        }
    }
}
