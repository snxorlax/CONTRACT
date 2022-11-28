using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Mirror;

public class CardBehaviour : NetworkBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IDropHandler
{
    //Card used by this GameObject
    public Card card;

    //GameObjects associated with localPlayer
    public GameObject player, handZone, playerField, enemyField, playerAvatar, enemyAvatar, playerUtility, playerDiscard;
    //Indicator associated with this object
    public GameObject indicator, cardEffect;
    //GameManager associated with this card's localPlayer
    public GameManager gameManager;
    //PlayerManager associated with this card's localPlayer
    public PlayerManager playerManager;
    public Display playerDisplay;
    public RectTransform rectTransform;
    public Canvas canvas;
    public bool isOverPlayZone, isOverUtility, isTargeting;
    public Vector3 originalScale, originalHandPos, originalHandScale;
    public bool canAttack;
    public bool canAmbush = false;
    //Used to determine if this card is selectable for Contracts
    public bool contractSelectable;
    public bool effectSelectable;
    public bool hover, isDragging;
    public bool interactable = true;



    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        canvas = GameObject.Find("MainCanvas").GetComponent<Canvas>();
        handZone = GameObject.Find("PlayerHandArea").gameObject;
        enemyField = GameObject.Find("EnemyField").gameObject;
        playerAvatar = gameManager.player;
        enemyAvatar = gameManager.enemy;
        playerField = GameObject.Find("PlayerField").gameObject;
        playerUtility = GameObject.Find("PlayerUtility").gameObject;
        playerDiscard = GameObject.Find("PlayerDiscard").gameObject;
        indicator = transform.Find("Indicator").gameObject;
        player = NetworkClient.connection.identity.gameObject;
        playerManager = player.GetComponent<PlayerManager>();
        playerDisplay = player.GetComponent<Display>();
        originalScale = transform.localScale;

        card = GetComponent<CardDisplay>().card;
        cardEffect = transform.Find("CardEffects").gameObject;

        originalHandPos = handZone.transform.position;
        originalHandScale = handZone.transform.localScale;
        
        canAttack = true;

        playerManager.selectedUnits.Callback += OnUnitListUpdated;
        playerManager.selectedRelics.Callback += OnRelicListUpdated;
    }
    public void OnUnitListUpdated(SyncList<GameObject>.Operation op, int index, GameObject oldUnit, GameObject newUnit)
    {
        if (playerManager.currentContract.Count > 0)
        {
            if (playerManager.selectedUnits.Count == playerManager.currentContract[0].GetComponent<CardBehaviour>().card.cardEffect.contractInfo[0].amount && contractSelectable)
            {
                // playerManager.PlayCard(playerManager.currentContract[0], false);
                playerManager.currentContract[0].GetComponent<CardDisplay>().card.cardEffect.ContractEffect();
                contractSelectable = false;
                // DestroyUnits();
                playerManager.currentContract.Clear();
                playerManager.isSelectingUnit = false;
            }
        }
    }
    public void OnRelicListUpdated(SyncList<GameObject>.Operation op, int index, GameObject oldRelic, GameObject newRelic)
    {
        if (playerManager.currentContract.Count > 0)
        {
            if (playerManager.selectedRelics.Count == playerManager.currentContract[0].GetComponent<CardBehaviour>().card.cardEffect.contractInfo[0].amount && contractSelectable)
            {
                playerManager.PlayCard(playerManager.currentContract[0], false);
                contractSelectable = false;
                DestroyRelics();
                playerManager.currentContract.Clear();
                playerManager.isSelectingRelic = false;
            }
        }
    }
    private void Update()
    {
        //select unit for contract
        if (Input.GetMouseButtonDown(0) && contractSelectable && card.cardType == Card.CardType.Henchman && hover)
        {
            playerManager.UpdateSelectedUnits(gameObject, true);
            indicator.GetComponent<Image>().enabled = false;
        }
        //select relic for contract
        else if (Input.GetMouseButtonDown(0) && contractSelectable && card.cardType == Card.CardType.Relic && hover)
        {
            playerManager.UpdateSelectedRelics(gameObject, true);
            indicator.GetComponent<Image>().enabled = false;

        }
        //RightClick to cancel selection
        if (Input.GetMouseButtonDown(1) && player.GetComponent<PlayerManager>().isSelectingRelic)
        {
                playerManager.UpdateSelectedRelics(this.gameObject, false);
                foreach (Transform t in playerUtility.transform)
                {
                    t.Find("Indicator").GetComponent<Image>().enabled = false;
                }
                player.GetComponent<PlayerManager>().currentContract.Clear();
                player.GetComponent<PlayerManager>().isSelectingRelic = false;
        }
        else if (Input.GetMouseButtonDown(1) && player.GetComponent<PlayerManager>().isSelectingUnit)
        {
                playerManager.UpdateSelectedUnits(this.gameObject, false);
                foreach (Transform t in playerField.transform)
                {
                    t.Find("Indicator").GetComponent<Image>().enabled = false;
                }
                player.GetComponent<PlayerManager>().currentContract.Clear();
                player.GetComponent<PlayerManager>().isSelectingUnit = false;
        }
        //targeted by effect
        if (Input.GetMouseButtonDown(0) && effectSelectable && hover)
        {
            indicator.GetComponent<Image>().enabled = false;
            if (playerManager.currentEffect.Count > 0)
            {
                ActivateEffect(playerManager.currentEffect[0]);
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
                obj.GetComponent<CardDisplay>().card.cardEffect?.TurnStartEffect(gameObject);
                obj.transform.Find("Indicator").GetComponent<Image>().enabled = false;
                playerManager.currentEffect.RemoveAt(0);
                Debug.Log(playerManager.currentEffect.Count);
                if (playerManager.currentEffect.Count > 0)
                {
                    playerManager.currentEffect[0].transform.Find("Indicator").GetComponent<Image>().enabled = true;
                    playerManager.currentEffect[0].transform.Find("Indicator").GetComponent<Image>().color = Color.blue;
                    playerManager.currentEffect[0].GetComponent<CardDisplay>().card.cardEffect.TurnStart();
                }
                break;
            case (CardEffect.effect.ActivatedEffect_1):
                obj.GetComponent<CardDisplay>().card.cardEffect.Effect1(gameObject);
                obj.transform.Find("Indicator").GetComponent<Image>().enabled = false;
                break;
            case (CardEffect.effect.Deathwalk):
                obj.GetComponent<CardDisplay>().card.cardEffect.DeathwalkEffect(gameObject);
                break;

        }
        if (tempSpell.GetComponent<CardDisplay>().card.cardType == Card.CardType.VillainousArt)
        {
            playerManager.DestroyCard(tempSpell);
        }
        effectSelectable = false;

    }
    public void SetCard()
    {
        card = GetComponent<CardDisplay>().card;
    }
    public void OnDrag(PointerEventData pointerEventData)
    {
        //Only allow drag if interactable
        if (interactable)
        {
            //Check if it is your turn, the card belongs to you, and the card is in your hand.
            if (playerManager.isTurn && hasAuthority && card.currentZone == "Hand")
            {
                if (card.cardType == Card.CardType.Villain)
                {
                    playerManager.playerFieldIndicator.GetComponent<Image>().enabled = true;
                }
                //Check if the card is a henchman and player has not yet summoned.
                if (card.cardType == Card.CardType.Henchman && playerManager.hasSummon)
                {
                    playerManager.playerFieldIndicator.GetComponent<Image>().enabled = true;
                }
                //If card is relic or VA, then it will highlight the utility portion
                else if (card.cardType == Card.CardType.VillainousArt || card.cardType == Card.CardType.Relic)
                {
                    playerManager.playerUtilityIndicator.GetComponent<Image>().enabled = true;
                }
                //Move the dragged card along with mouse according to canvas scale
                rectTransform.anchoredPosition += pointerEventData.delta / canvas.scaleFactor;
            }
            //check for all the attacking prerequisites: your turn, your card, the card is on the field, and it is the second turn of the game, and correct mouse button is used
            else if (!card.shroud && playerManager.isTurn && hasAuthority && card.currentZone == "Field" && canAttack && gameManager.turnNumber > 1 && pointerEventData.button == PointerEventData.InputButton.Left)
            {
                isTargeting = true;
                //Highlight available targets for attacking
                if (enemyField.transform.childCount > 0)
                {
                    foreach (Transform child in enemyField.transform)
                    {
                        child.Find("Indicator").gameObject.GetComponent<Image>().enabled = true;
                        child.Find("Indicator").gameObject.GetComponent<Image>().color = Color.red;
                    }
                }
                else if (enemyField.transform.childCount == 0)
                {
                    // enemyAvatar.transform.Find("Indicator").gameObject.GetComponent<Image>().enabled = true;
                }

            }
            else if (card.shroud && playerManager.isTurn && hasAuthority && card.currentZone == "Field" && canAmbush)
            {
                cardEffect.transform.GetChild(4).gameObject.SetActive(true);
            }
            handZone.transform.position = originalHandPos;
            handZone.transform.localScale = originalHandScale;
            
        }
    }
    public void OnBeginDrag(PointerEventData pointerEventData)
    {
        isDragging = true;
        if (card.currentZone == "Field" && playerManager.isTurn)
        {
            //Deactivate the larger image when dragging for clarity
            transform.Find("HoverImage").gameObject.SetActive(false);
            //Resets the parent transform because there is no longer a need to have larger image be in front of field zones
            transform.parent.SetSiblingIndex(8);
            if (pointerEventData.button == PointerEventData.InputButton.Right && card.activatedEffectText.Count > 0)
            {
                cardEffect.transform.GetChild(0).gameObject.SetActive(true);
            }

        }
        if (card.currentZone == "Discard" && playerManager.isTurn)
        {
            if (pointerEventData.button == PointerEventData.InputButton.Right)
            {
                cardEffect.transform.GetChild(3).gameObject.SetActive(true);
            }

        }
    }
    public void OnEndDrag(PointerEventData pointerEventData)
    {
        if (interactable)
        {
            isTargeting = false;
            if (isOverPlayZone && playerManager.isTurn && hasAuthority && card.currentZone == "Hand")
            {
                if (card.cardType == Card.CardType.Villain)
                {
                    card.cardEffect.Contract();
                }
                    if (card.cardType == Card.CardType.Henchman && !playerManager.hasSummon)
                    {
                        Debug.Log("Can only recruit one unit per turn");
                    }
                    else if (card.cardType == Card.CardType.Henchman && playerManager.hasSummon)
                    {
                        if (pointerEventData.button == PointerEventData.InputButton.Right)
                        {
                            playerManager.PlayCard(this.gameObject, true);
                        }
                        else
                        {
                            if (card.cardEffect)
                            {
                                playerManager.currentEffect.Add(gameObject);
                            }
                            playerManager.PlayCard(this.gameObject, false);
                        }
                    }

            }
            else if (isOverUtility && playerManager.isTurn && hasAuthority && card.currentZone == "Hand")
            {
                if (card.cardType == Card.CardType.Relic || card.cardType == Card.CardType.VillainousArt)
                {
                    if (card.cardEffect)
                    {
                        playerManager.currentEffect.Add(gameObject);
                    }
                    playerManager.PlayCard(gameObject, false);
                }
            }
            playerDisplay.DisplayHorizontal(playerManager.playerHand, Display.handOffset);
            transform.localScale = originalScale;
            playerManager.playerFieldIndicator.GetComponent<Image>().enabled = false;
            playerManager.playerUtilityIndicator.GetComponent<Image>().enabled = false;
            if (playerManager.currentEffect.Count == 0)
            {
                foreach (Transform child in enemyField.transform)
                {
                    child.Find("Indicator").gameObject.GetComponent<Image>().enabled = false;
                }

            }
            cardEffect.transform.GetChild(0).gameObject.SetActive(false);
            cardEffect.transform.GetChild(1).gameObject.SetActive(false);
            cardEffect.transform.GetChild(2).gameObject.SetActive(false);
            cardEffect.transform.GetChild(3).gameObject.SetActive(false);
            cardEffect.transform.GetChild(4).gameObject.SetActive(false);
            isDragging = false;
            // enemyAvatar.transform.Find("Indicator").gameObject.GetComponent<Image>().enabled = false;

        }
    }
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("PlayerField"))
        {
            isOverPlayZone = true;
        }
        if (other.gameObject.CompareTag("PlayerUtility"))
        {
            isOverUtility = true;
        }

    }
    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("PlayerField"))
        {
            isOverPlayZone = false;
        }
        if (other.gameObject.CompareTag("PlayerUtility"))
        {
            isOverUtility = false;
        }
    }
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        if (interactable)
        {
            hover = true;
            if (card.currentZone == "Field" && transform.Find("HoverImage") && isDragging == false)
            {
                if (!(gameObject.transform.parent == enemyField.transform && card.shroud))
                {
                    transform.Find("HoverImage").GetComponent<CardDisplay>().card = card;
                    transform.Find("HoverImage").GetComponent<CardDisplay>().SetCardProperties();
                    transform.Find("HoverImage").gameObject.SetActive(true);
                    transform.parent.SetSiblingIndex(11);
                }
            }
            if (transform.parent == handZone.transform)
            {
                transform.localScale *= 1.4f;
                handZone.transform.position = new Vector3(handZone.transform.position.x, handZone.transform.position.y + .4f);
                handZone.transform.localScale *= 1.4f;
                transform.SetAsLastSibling();
            }
            else if (card.currentZone == "Discard" && !isDragging)
            {
                transform.localScale *= 2f;
                transform.SetAsLastSibling();
            }
            else if (transform.parent == playerField.transform && playerManager.isSelectingUnit)
            {
                contractSelectable = true;
            }
            else if (transform.parent == playerUtility.transform && playerManager.isSelectingRelic)
            {
                contractSelectable = true;
            }

        }
    }
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        if (card.currentZone == "Field" && transform.Find("HoverImage"))
        {
            transform.Find("HoverImage").gameObject.SetActive(false);
            transform.parent.SetSiblingIndex(11);
        }

        if (card.currentZone == "Hand")
        {
            transform.localScale = originalScale;
            handZone.transform.position = originalHandPos;
            handZone.transform.localScale = originalHandScale;
        }
        if (card.currentZone == "Discard")
        {
            if (!isDragging)
            {
                transform.localScale = originalScale;
            }
        }
        hover = false;
    }
    public void OnDrop(PointerEventData pointerEventData)
    {
        if (pointerEventData.pointerDrag.GetComponent<CardBehaviour>().canAttack)
        {
            if (pointerEventData.pointerDrag.GetComponent<CardBehaviour>().isTargeting && transform.parent == enemyField.transform)
            {
                if (gameManager.turnNumber > 1)
                {
                    StartCoroutine(AttackAnimation(pointerEventData.pointerDrag));
                    pointerEventData.pointerDrag.GetComponent<CardBehaviour>().canAttack = false;
                }
                else if (gameManager.turnNumber <= 1)
                {
                    Debug.Log("Cannot attack until Turn 2");
                }
            }
        }
    }

    public void Bleed(int number)
    {
        CmdBleed(number);
    }
    [Command(requiresAuthority = false)]
    public void CmdBleed(int number)
    {
        RpcBleed(number);
    }
    [ClientRpc]
    public void RpcBleed(int number)
    {
        if (hasAuthority)
        {
            playerAvatar.GetComponent<PlayerDisplay>().playerInfo.lifeTotal -= number;
            playerAvatar.GetComponent<PlayerDisplay>().SetPlayerProperties();
        }
        if (!hasAuthority)
        {
            enemyAvatar.GetComponent<PlayerDisplay>().playerInfo.lifeTotal -= number;
            enemyAvatar.GetComponent<PlayerDisplay>().SetPlayerProperties();
        }
    }

    public void Betray(int amount)
    {
        playerManager.isSelectingUnit = true;
        foreach (Transform t in playerField.transform)
        {
            t.Find("Indicator").GetComponent<Image>().enabled = true;
            t.Find("Indicator").GetComponent<Image>().color = Color.red;
            playerManager.currentContract.Add(gameObject);
        }
    }
    public void Greed(int amount)
    {
        playerManager.isSelectingRelic = true;
        foreach (Transform t in playerUtility.transform)
        {
            t.Find("Indicator").GetComponent<Image>().enabled = true;
            t.Find("Indicator").GetComponent<Image>().color = Color.red;
            playerManager.currentContract.Add(gameObject);
        }
    }
    public void DestroyUnits()
    {
        CmdDestroyUnits();
    }
    [Command(requiresAuthority = false)]
    public void CmdDestroyUnits()
    {
        RpcDestroyUnits();
    }
    
    [ClientRpc]
    public void RpcDestroyUnits()
    {
        foreach (PlayerManager p in gameManager.players)
        {
            foreach (GameObject obj in p.selectedUnits)
            {
                p.DestroyCard(obj);
            }
        }
        playerManager.UpdateSelectedUnits(gameObject, false);

    }
    public void DestroyRelics()
    {
        CmdDestroyRelics();
    }
    [Command(requiresAuthority = false)]
    public void CmdDestroyRelics()
    {
        RpcDestroyRelics();
    }
    
    [ClientRpc]
    public void RpcDestroyRelics()
    {
        foreach (PlayerManager p in gameManager.players)
        {
            foreach (GameObject obj in p.selectedRelics)
            {
                // Debug.Log("Test");
                p.DestroyCard(obj);
            }
        }
        playerManager.UpdateSelectedRelics(gameObject, false);

    }

    public IEnumerator AttackAnimation(GameObject attacker)
    {
        attacker.transform.parent.SetSiblingIndex(11);
        Vector2 originalPos = attacker.transform.position;
        for (int i = 0; i < 10; i++)
        {
            attacker.transform.position = Vector2.Lerp(attacker.transform.position, transform.position, .02f);

            yield return new WaitForSeconds(.01f);
        }
        StopAllCoroutines();
        gameManager.Combat(attacker, gameObject);
        StartCoroutine(ResetAttacker(attacker, originalPos));
    }
    public IEnumerator ResetAttacker(GameObject attacker, Vector2 originalPos)
    {
        while ((Vector2)attacker.transform.position != originalPos)
        {
            attacker.transform.position = Vector2.Lerp(attacker.transform.position, originalPos, .015f);

            yield return new WaitForSeconds(.01f);

        }
        attacker.transform.parent.SetSiblingIndex(8);
    }

}
