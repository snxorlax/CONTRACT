using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Mirror;

public class PlayerAvatarBehaviour : NetworkBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public GameManager gameManager;
    public PlayerManager player;
    public bool effectSelectable;
    public bool hover;
    private void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        hover = false;

    }
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        hover = true;
    }
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        hover = false;
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && effectSelectable && hover)
        {
            player = gameManager.playerManager;
            if (player.currentEffect.Count > 0)
            {
                ActivateEffect(player.currentEffect[0]);
            }
        }
    }
    public void ActivateEffect(GameObject obj)
    {
                GameObject tempSpell = obj;
                switch (obj.GetComponent<CardDisplay>().card.cardEffect?.currentEffect)
                {
                    case (CardEffect.effect.Play):
                        obj.GetComponent<CardDisplay>().card.cardEffect?.PlayEffect(gameObject);
                        break;
                    case (CardEffect.effect.TurnStart):
                    {
                        obj.GetComponent<CardDisplay>().card.cardEffect?.TurnStartEffect(gameObject);
                        obj.transform.Find("Indicator").GetComponent<Image>().enabled = false;
                        player.currentEffect.RemoveAt(0);
                        if (player.currentEffect.Count > 0)
                        {
                            player.currentEffect[0].transform.Find("Indicator").GetComponent<Image>().enabled = true;
                            player.currentEffect[0].transform.Find("Indicator").GetComponent<Image>().color = Color.blue;
                            player.currentEffect[0].GetComponent<CardDisplay>().card.cardEffect.TurnStart();
                        }
                        break;
                    }
                }
                if (tempSpell.GetComponent<CardDisplay>().card.cardType == Card.CardType.VillainousArt)
                {
                    player.QueueDestroy(tempSpell);
                }
                effectSelectable = false;

    }
    
    public void OnDrop(PointerEventData pointerEventData)
    {
        foreach (PlayerManager p in gameManager.players)
        {
            if (p.enemyField.Count > 0)
            {
                return;
            }
        }
        if (pointerEventData.pointerDrag.GetComponent<CardBehaviour>().canAttack)
        {
            if (pointerEventData.pointerDrag.GetComponent<CardBehaviour>().isTargeting && gameManager.turnNumber > 1)
            {
                gameManager.Combat(pointerEventData.pointerDrag, gameObject);
                pointerEventData.pointerDrag.GetComponent<CardBehaviour>().canAttack = false;
            }

        }
    }
}
