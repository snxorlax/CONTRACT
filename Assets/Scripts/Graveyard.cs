using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Mirror;

public class Graveyard : NetworkBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject playerDiscard, player;
    public PlayerManager playerManager;
    public Display display;
    public bool hover;
private void Start()
{
        player = NetworkClient.connection.identity.gameObject;
        playerManager = player.GetComponent<PlayerManager>();
        display = player.GetComponent<Display>();
    
}
    void Update()
    {
        if (hover)
        {
            if (Input.GetMouseButtonDown(0))
            {
                playerDiscard = GameObject.Find("PlayerDiscard");
                display.DisplayVertical(playerManager.playerDiscard, Display.discardOffset);
                playerDiscard.transform.SetAsLastSibling();
            }

        }
    }
    public virtual void OnPointerEnter(PointerEventData pointerEventData)
    {
        hover = true;
    }
    public virtual void OnPointerExit(PointerEventData pointerEventData)
    {
        hover = false;
    }
}
