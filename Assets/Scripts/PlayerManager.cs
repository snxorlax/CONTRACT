using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Mirror;
using UnityEngine.VFX;
using TMPro;

public class PlayerManager : NetworkBehaviour
{
    //GameManager
    public GameObject GameManager;
    public GameManager gameManager;
    //Action Queue
    public Queue<IEnumerator> actionQueue;
    //Action Queue Bool
    public bool actionComplete = false;
    //Work in Progress Lists
    public List<GameObject> playerHand = new List<GameObject>();
    public List<GameObject> enemyHand = new List<GameObject>();
    public List<Card> playerDeck = new List<Card>();
    public List<Card> enemyDeck = new List<Card>();
    public List<GameObject> playerField = new List<GameObject>();
    public List<GameObject> playerUtility = new List<GameObject>();
    public List<GameObject> playerDiscard = new List<GameObject>();
    public List<GameObject> enemyDiscard = new List<GameObject>();
    public List<GameObject> enemyField = new List<GameObject>();
    public List<GameObject> enemyUtility = new List<GameObject>();
    public readonly SyncList<GameObject> selectedUnits = new SyncList<GameObject>();
    public readonly SyncList<GameObject> selectedRelics = new SyncList<GameObject>();
    public List<GameObject> currentContract = new List<GameObject>();
    public List<GameObject> currentEffect = new List<GameObject>();
    public PlayerManager enemyManager;

    public Deck deck;
    public TextMeshProUGUI playerDeckCount, enemyDeckCount;
    public GameObject deckIndicator, enemyDeckIndicator;

    //Areas where cards can be spawned, etc.
    public GameObject Player, Enemy, playerHandArea, playerDiscardArea, enemyHandArea, playerFieldArea, playerUtilityArea, enemyFieldArea, enemyUtilityArea, enemyDiscardArea;
    public GameObject card, playerAvatarZone, enemyAvatarZone, playerFieldIndicator, playerUtilityIndicator, gameText;
    public PlayerInfo playerInfo;

    [SyncVar]
    public bool isTurn = false;
    public bool isSelectingUnit = false;
    public bool isSelectingRelic = false;
    public bool hasSummon;
    public bool openingDraw = true;

    [SyncVar(hook = nameof(CheckContinuous))]
    public int playerUnitCount;
    //Card effect counters
    public int blade = 0;
    private void Awake()
    {
        GameManager = GameObject.Find("GameManager");
        gameManager = GameManager.GetComponent<GameManager>();
        actionQueue = gameManager.actionQueue;
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        playerHandArea = GameObject.Find("PlayerHandArea");
        enemyHandArea = GameObject.Find("EnemyHandArea");
        playerFieldArea = GameObject.Find("PlayerField");
        playerFieldIndicator = GameObject.Find("PlayerFieldIndicator");
        playerUtilityArea = GameObject.Find("PlayerUtility");
        enemyUtilityArea = GameObject.Find("EnemyUtility");
        playerUtilityIndicator = GameObject.Find("PlayerUtilityIndicator");
        enemyFieldArea = GameObject.Find("EnemyField");
        playerDiscardArea = GameObject.Find("PlayerDiscard");
        enemyDiscardArea = GameObject.Find("EnemyDiscard");
        
        playerAvatarZone = GameObject.Find("PlayerAvatarZone");
        enemyAvatarZone = GameObject.Find("EnemyAvatarZone");

        deckIndicator = GameObject.Find("PlayerDeck");
        playerDeckCount = deckIndicator.transform.Find("DeckInfo").Find("DeckCount").GetComponent<TextMeshProUGUI>();

        enemyDeckIndicator = GameObject.Find("EnemyDeck");
        enemyDeckCount = enemyDeckIndicator.transform.Find("DeckInfo").Find("DeckCount").GetComponent<TextMeshProUGUI>();



        Enemy = GameObject.Find("EnemyAvatar");

        gameText = GameObject.Find("GameText");

        


        hasSummon = true;

    }
    public void CheckContinuous(int oldVal, int newVal)
    {
        Debug.Log(newVal);
        gameManager.UnitEvent?.Invoke();
    }
    public void UpdatePlayerUnitCount(int val)
    {
        CmdUpdatePlayerUnitCount(val);
    }
    [Command(requiresAuthority = false)]
    public void CmdUpdatePlayerUnitCount(int val)
    {
        playerUnitCount += val;
        Debug.Log(playerUnitCount);
    }

    public void StartPlayer()
    {
        CmdStartPlayer();
    }
    [Command(requiresAuthority = false)]
    public void CmdStartPlayer()
    {
        GameObject newPlayer = Instantiate(Player, Vector2.zero, Quaternion.identity, transform);
        newPlayer.GetComponent<PlayerDisplay>().playerInfo = Instantiate(playerInfo);
        newPlayer.GetComponent<PlayerDisplay>().SetPlayerProperties();
        NetworkServer.Spawn(newPlayer, connectionToClient);
        RpcStartPlayer(newPlayer);
    }
    [ClientRpc]
    public void RpcStartPlayer(GameObject player)
    {
        playerDeck.AddRange(deck.mainDeck);
        if (hasAuthority)
        {
            player.transform.SetParent(playerAvatarZone.transform, false);
            player.GetComponent<PlayerDisplay>().playerInfo = Instantiate(playerInfo);
            player.GetComponent<PlayerDisplay>().SetPlayerProperties();
            gameManager.player = player;
        }
        if (!hasAuthority)
        {
            player.transform.SetParent(enemyAvatarZone.transform, false);
            player.GetComponent<PlayerDisplay>().playerInfo = Instantiate(playerInfo);
            player.GetComponent<PlayerDisplay>().SetPlayerProperties();
            gameManager.enemy = player;
        }
        if (isLocalPlayer)
        {
            // QueueDraw(5);
        }
        if (isTurn)
        {
            Invoke("StartTurn", 3);
        }
    }
    public void UpdateSummonAndAttacks()
    {
        CmdUpdateSummonAndAttacks();
    }
    [Command(requiresAuthority = false)]
    public void CmdUpdateSummonAndAttacks()
    {
        RpcUpdateSummonAndAttacks();
    }
    [ClientRpc]
    public void RpcUpdateSummonAndAttacks()
    {
        hasSummon = true;
        foreach(GameObject card in playerField)
        {
            card.GetComponent<CardBehaviour>().canAttack = true;
            card.GetComponent<CardBehaviour>().canAmbush = true;
        }
    }
    public void StartTurn()
    {
        Debug.Log("StartTurn");
        ActivateDrawAnimation();
        //reset blade counter
        blade = 0;
        foreach (GameObject g in playerField)
        {
            if (!g.GetComponent<CardBehaviour>().card.shroud)
            {
                g.GetComponent<CardBehaviour>().card.cardEffect?.TurnStart();
                if (g.GetComponent<CardBehaviour>().card.cardEffect.currentEffect == CardEffect.effect.TurnStart)
                {
                    currentEffect.Add(g);
                    currentEffect[0].transform.Find("Indicator").GetComponent<Image>().enabled = true;
                    currentEffect[0].transform.Find("Indicator").GetComponent<Image>().color = Color.blue;
                }
            }
            
        }
    }
    public void ActivateDrawAnimation(){
        CmdActivateDrawAnimation();
    }
    [Command(requiresAuthority = false)]
    public void CmdActivateDrawAnimation(){
        RpcActivateDrawAnimation();
    }
    [ClientRpc]
    public void RpcActivateDrawAnimation()
    {
        foreach (PlayerManager p in gameManager.players)
        {
            p.openingDraw = false;
        }
    }
    public void ChangeTurn()
    {
        CmdChangeTurn();
    }
    [Command]
    public void CmdChangeTurn()
    {
        if (isTurn)
        {
            isTurn = false;
        }
        else if (!isTurn)
        {
            isTurn = true;
            if (gameManager.turnNumber > 0)
            {
                QueueDraw(2);
                gameManager.ActivateActionQueue();
            }
            UpdateSummonAndAttacks();
        }
    }
    public void UpdatePlayerList()
    {
        CmdUpdatePlayerList();
    }
    [Command(requiresAuthority = false)]
    public void CmdUpdatePlayerList()
    {
        gameManager.players.Add(GetComponent<PlayerManager>());

    }
    //Adds Draw to Action Queue
    public void QueueDraw(int num)
    {
        for (int i = 0; i < num; i++)
        {
            actionQueue.Enqueue(DrawCard());
        }
    }
    //Coroutine for drawing. In order to start, it will be added to the Action Queue
    public IEnumerator DrawCard()
    {
        //Reset bools to false to begin coroutine
        actionComplete = false;
        gameManager.actionComplete = false;
        //Activate Draw Effect
        CmdDrawCard();
        //Wait for animation to finish and set actionComplete to true
        while (!actionComplete)
        {
            actionComplete = gameManager.actionComplete;
            yield return null;
        }
        Debug.Log(actionQueue.Count);
    }
    [Command(requiresAuthority = false)]
    public void CmdDrawCard()
    {
        //generates a random number in the range of deck count
        int ran = Random.Range(0, playerDeck.Count);
        //Instantiates whichever card resides at that deck number (will later be removed)
        Card cardInstance = Instantiate(playerDeck[ran]);
        //Sets that as the scriptable object to determine the prefab's properties
        card.GetComponent<CardDisplay>().card = cardInstance;
        //Instantiates a new GameObject with the properties of that card
        GameObject newCard = Instantiate(card, Vector2.zero, Quaternion.identity, transform);
        //Spawns that card on the server and gives the localClient authority
        NetworkServer.Spawn(newCard, connectionToClient);
        //Calls a remote procedure call to determine the card's parent object, and which lists it will be added to
        FormatCards(newCard, cardInstance.cardNo, "Draw");
    }
    public void UpdateDeckCount()
    {
        CmdUpdateDeckCount();
    }
    [Command(requiresAuthority=false)]
    public void CmdUpdateDeckCount()
    {
        RpcUpdateDeckCount();
    }
    [ClientRpc]
    public void RpcUpdateDeckCount()
    {
        if (isLocalPlayer)
        {
            playerDeckCount.text = playerDeck.Count.ToString();
        }
        foreach (PlayerManager p in gameManager.players)
        {
            if (p.enemyManager)
            {
                p.enemyDeckCount.text = p.enemyManager.playerDeck.Count.ToString();
            }
        }
    }
    //Adds PlayCard to Action Queue
    public void QueuePlay()
    {

    }
    public void PlayCard(GameObject card, bool shroud)
    {
        CmdPlayCard(card, shroud);
    }
    [Command]
    public void CmdPlayCard(GameObject card, bool shroud)
    {
        if (shroud)
        {
            FormatCards(card, 1, "Play");
        }
        else if (!shroud)
        {
            FormatCards(card, 0, "Play");
        }

    }
    public void DestroyCard(GameObject card)
    {
        CmdDestroyCard(card);
    }
    [Command(requiresAuthority = false)]
    public void CmdDestroyCard(GameObject card)
    {
        FormatCards(card, 0, "Destroy");
    }
    public void RestoreCard(GameObject card)
    {
        CmdRestoreCard(card);
    }
    [Command(requiresAuthority = false)]
    public void CmdRestoreCard(GameObject card)
    {
        FormatCards(card, 0, "Restore");
    }
    public void ExileCard(GameObject card)
    {
        CmdExileCard(card);
    }
    [Command(requiresAuthority = false)]
    public void CmdExileCard(GameObject card)
    {
        FormatCards(card, 0, "Exile");
    }
    public void UpdateSelectedUnits(GameObject obj, bool value)
    {
        CmdUpdateSelectedUnits(obj, value);
    }

    [Command (requiresAuthority = false)]
    public void CmdUpdateSelectedUnits(GameObject obj, bool value)
    {
        if (value)
        {
            selectedUnits.Add(obj);
        }
        else if (!value)
        {
            selectedUnits.Clear();
        }
    }
    public void UpdateSelectedRelics(GameObject obj, bool value)
    {
        CmdUpdateSelectedRelics(obj, value);
    }
    [Command (requiresAuthority = false)]
    public void CmdUpdateSelectedRelics(GameObject obj, bool value)
    {
        if (value)
        {
            selectedRelics.Add(obj);
        }
        else if (!value)
        {
            selectedRelics.Clear();
        }
    }
    public void CreateCard(Card card)
    {
        CmdCreateCard(card);
    }
    [Command(requiresAuthority = false)]
    public void CmdCreateCard(Card createdCard)
    {
        Card cardInstance = Instantiate(createdCard);
        card.GetComponent<CardDisplay>().card = cardInstance;
        GameObject newCard = Instantiate(card, Vector2.zero, Quaternion.identity, transform);
        NetworkServer.Spawn(newCard, connectionToClient);
        FormatCards(newCard, cardInstance.cardNo, "Create");
    }
    public void ReturnCard(GameObject card)
    {
        CmdReturnCard(card);
    }
    [Command(requiresAuthority=false)]
    public void CmdReturnCard(GameObject card)
    {
        FormatCards(card, 0, "Return");
    }
    [ClientRpc]
    private void FormatCards(GameObject card, int num, string action)
    {
        if (action == "Draw")
        {
            card.GetComponent<CardDisplay>().card = Instantiate(card.GetComponent<CardDisplay>().cardCatalogue.CardList[num]);
            if (card.GetComponent<CardDisplay>().card.cardEffect)
            {
                card.GetComponent<CardDisplay>().card.cardEffect = Instantiate(card.GetComponent<CardDisplay>().card.cardEffect);
            }
        }
        if (action == "Create")
        {
            card.GetComponent<CardDisplay>().card = Instantiate(card.GetComponent<CardDisplay>().cardCatalogue.CardList[num]);
        }
        card.GetComponent<CardDisplay>().card.cardEffect.self = card;
        card.GetComponent<CardDisplay>().card.cardEffect.CardSetup();
        card.GetComponent<CardDisplay>().SetCardProperties();
        if (hasAuthority)
        {
            if (action == "Draw")
            {
                card.GetComponent<CardDisplay>().card.currentZone = "Hand";
                card.transform.SetParent(playerHandArea.transform, false);
                playerDeck.Remove(card.GetComponent<CardDisplay>().cardCatalogue.CardList[num]);
                UpdateDeckCount();
                playerHand.Add(card);
                card.GetComponent<AnimateCard>().DrawPlayerCard();
                GetComponent<Display>().DisplayHorizontal(playerHand, Display.handOffset);
            }
            if (action == "Play")
            {
                card.GetComponent<CardBehaviour>().SetCard();
                if (num == 0)
                {
                    gameManager.OnPlay?.Invoke(card.GetComponent<CardDisplay>().card);
                }
                switch (card.GetComponent<CardDisplay>().card.cardType)
                {
                    case Card.CardType.Henchman:
                        card.transform.SetParent(playerFieldArea.transform, false);
                        playerField.Add(card);
                        UpdatePlayerUnitCount(1);
                        this.GetComponent<Display>().DisplayHorizontal(playerField, Display.fieldOffset);
                        hasSummon = false;
                        break;
                    case Card.CardType.VillainousArt:
                        card.transform.SetParent(playerUtilityArea.transform, false);
                        playerUtility.Add(card);
                        this.GetComponent<Display>().DisplayHorizontal(playerUtility, Display.fieldOffset);
                        break;
                    case Card.CardType.Relic:
                        card.transform.SetParent(playerUtilityArea.transform, false);
                        playerUtility.Add(card);
                        this.GetComponent<Display>().DisplayHorizontal(playerUtility, Display.fieldOffset);
                        break;
                    case Card.CardType.Villain:
                        Debug.Log("Play Villain");
                        card.transform.SetParent(playerFieldArea.transform, false);
                        playerField.Add(card);
                        UpdatePlayerUnitCount(1);
                        this.GetComponent<Display>().DisplayHorizontal(playerField, Display.fieldOffset);
                        break;
                }
                card.GetComponent<CardDisplay>().card.currentZone = "Field";
                if (num == 1)
                {
                    card.transform.Find("Back").gameObject.SetActive(true);
                    card.transform.Find("VFX").Find("Shroud").GetComponent<VisualEffect>().enabled = true;
                    card.GetComponent<CardDisplay>().card.shroud = true;
                    card.GetComponent<CardDisplay>().SetCardProperties();
                    card.GetComponent<CardBehaviour>().SetCard();

                }
                playerHand.Remove(card);
                this.GetComponent<Display>().DisplayHorizontal(playerHand, Display.handOffset);
                card.GetComponent<AnimateCard>().StartPlayerPlay();
            }
            if (action == "Destroy")
            {
                if (card.GetComponent<NetworkIdentity>().hasAuthority)
                {
                    card.GetComponent<CardBehaviour>().card.currentZone = "Discard";
                    card.transform.SetParent(playerDiscardArea.transform, false);
                    playerDiscard.Add(card);
                }
                if (playerField.Contains(card))
                {
                    playerField.Remove(card);
                    card.transform.Find("Back").gameObject.SetActive(false);
                    card.transform.Find("VFX").Find("Shroud").GetComponent<VisualEffect>().enabled = false;
                    card.transform.Find("HoverImage").gameObject.SetActive(false);
                    gameManager.ResetStats(card);
                    UpdatePlayerUnitCount(-1);
                }
                else if (playerUtility.Contains(card))
                {
                    playerUtility.Remove(card);
                }
                foreach (PlayerManager p in gameManager.players)
                {
                    if (p.enemyField.Contains(card))
                    {
                        p.enemyField.Remove(card);
                        this.GetComponent<Display>().DisplayHorizontal(p.enemyField, Display.fieldOffset);
                    }
                    else if (p.enemyUtility.Contains(card))
                    {
                        Debug.Log("Test");
                        p.enemyUtility.Remove(card);
                        this.GetComponent<Display>().DisplayHorizontal(p.enemyUtility, Display.fieldOffset);
                    }
                }
                this.GetComponent<Display>().DisplayVertical(playerDiscard, Display.discardOffset);
                this.GetComponent<Display>().DisplayHorizontal(playerField, Display.fieldOffset);
                this.GetComponent<Display>().DisplayHorizontal(playerUtility, Display.fieldOffset);
                // StopAllCoroutines();
            }
            if (action == "Restore")
            {
                card.GetComponent<CardDisplay>().card.currentZone = "Field";
                card.GetComponent<CardDisplay>().card.health = card.GetComponent<CardDisplay>().card.originalHealth;
                card.GetComponent<CardDisplay>().SetCardProperties();
                if (card.GetComponent<CardDisplay>().card.cardType == Card.CardType.Henchman)
                {
                    card.transform.SetParent(playerFieldArea.transform, false);
                    playerField.Add(card);
                    UpdatePlayerUnitCount(1);
                }
                else if (card.GetComponent<CardDisplay>().card.cardType == Card.CardType.Relic)
                {
                    card.transform.SetParent(playerUtilityArea.transform, false);
                    playerUtility.Add(card);
                }
                playerDiscard.Remove(card);
                this.GetComponent<Display>().DisplayHorizontal(playerDiscard, Display.fieldOffset);
                this.GetComponent<Display>().DisplayHorizontal(playerField, Display.fieldOffset);
                this.GetComponent<Display>().DisplayHorizontal(playerUtility, Display.fieldOffset);
            }
            else if (action == "Exile")
            {
                playerDiscard.Remove(card);
                Destroy(card);
            }
            else if (action == "Create")
            {
                if (card.GetComponent<CardDisplay>().card.cardEffect)
                {
                    card.GetComponent<CardDisplay>().card.cardEffect.self = card;
                }
                card.GetComponent<CardDisplay>().card.cardEffect.Create();
                switch (card.GetComponent<CardDisplay>().card.cardType)
                {
                    case Card.CardType.Henchman:
                        card.GetComponent<CardDisplay>().card.currentZone = "Field";
                        card.transform.SetParent(playerFieldArea.transform, false);
                        playerField.Add(card);
                        UpdatePlayerUnitCount(1);
                        this.GetComponent<Display>().DisplayHorizontal(playerField, Display.fieldOffset);
                        break;
                    case Card.CardType.VillainousArt:
                        card.transform.SetParent(playerHandArea.transform, false);
                        playerHand.Add(card);
                        this.GetComponent<Display>().DisplayHorizontal(playerHand, Display.fieldOffset);
                        break;
                    case Card.CardType.Relic:
                        card.GetComponent<CardDisplay>().card.currentZone = "Field";
                        card.transform.SetParent(playerUtilityArea.transform, false);
                        playerUtility.Add(card);
                        this.GetComponent<Display>().DisplayHorizontal(playerUtility, Display.fieldOffset);
                        break;
                }
                //Placeholder Animation
                card.GetComponent<AnimateCard>().StartPlayerPlay();
            }
            else if (action == "Return")
            {
                if (card.transform.parent == playerFieldArea.transform)
                {
                    playerField.Remove(card);
                    GetComponent<Display>().DisplayHorizontal(playerField, Display.fieldOffset);
                }
                else if (card.transform.parent == playerUtilityArea.transform)
                {
                    playerUtility.Remove(card);
                    GetComponent<Display>().DisplayHorizontal(playerUtility, Display.fieldOffset);
                }
                card.transform.SetParent(playerHandArea.transform);
                gameManager.ResetStats(card);
                card.GetComponent<CardDisplay>().card.currentZone = "Hand";
                card.transform.Find("HoverImage").gameObject.SetActive(false);
                gameManager.ResetStats(card);
                playerHand.Add(card);
                GetComponent<Display>().DisplayHorizontal(playerHand, Display.handOffset);
            }
        }
        else if (!hasAuthority)
        {
            // Debug.Log(action);
            if (action == "Draw")
            {
                card.transform.SetParent(enemyHandArea.transform, false);
                card.transform.rotation = Quaternion.Euler(0, 0, -180);
                card.transform.Find("Back").gameObject.SetActive(true);
                playerDeck.Remove(card.GetComponent<CardDisplay>().cardCatalogue.CardList[num]);
                UpdateDeckCount();
                enemyHand.Add(card);
                card.GetComponent<AnimateCard>().DrawEnemyCard();
                this.GetComponent<Display>().DisplayHorizontal(enemyHand, Display.enemyHandOffset);
            }
            else if (action == "Play")
            {
                GameObject.Find("PlayZoneIndicator").GetComponent<Image>().enabled = false;
                card.GetComponent<CardDisplay>().card.currentZone = "Field";
                if (num != 1)
                {
                    card.GetComponent<AnimateCard>().PlayEnemyCard();
                }
                switch (card.GetComponent<CardDisplay>().card.cardType)
                {
                    case Card.CardType.Henchman:
                        card.transform.SetParent(enemyFieldArea.transform, false);
                        enemyField.Add(card);
                        this.GetComponent<Display>().DisplayHorizontal(enemyField, Display.fieldOffset);
                        break;
                    case Card.CardType.VillainousArt:
                        card.transform.SetParent(enemyUtilityArea.transform, false);
                        enemyUtility.Add(card);
                        this.GetComponent<Display>().DisplayHorizontal(enemyUtility, Display.fieldOffset);
                        break;
                    case Card.CardType.Relic:
                        card.transform.SetParent(enemyUtilityArea.transform, false);
                        enemyUtility.Add(card);
                        this.GetComponent<Display>().DisplayHorizontal(enemyUtility, Display.fieldOffset);
                        break;
                    case Card.CardType.Villain:
                        card.transform.SetParent(enemyFieldArea.transform, false);
                        enemyField.Add(card);
                        this.GetComponent<Display>().DisplayHorizontal(enemyField, Display.fieldOffset);
                        break;
                }
                if (num == 1)
                {
                    card.GetComponent<CardDisplay>().card.shroud = true;
                    card.transform.Find("Back").gameObject.SetActive(true);
                    card.transform.Find("VFX").Find("Shroud").GetComponent<VisualEffect>().enabled = true;
                }
                enemyHand.Remove(card);
                this.GetComponent<Display>().DisplayHorizontal(enemyHand, Display.handOffset);
            }
            else if (action == "Destroy")
            {
                Debug.Log("!Destroy");
                if (!card.GetComponent<NetworkIdentity>().hasAuthority)
                {
                    card.GetComponent<CardBehaviour>().card.currentZone = "Discard";
                    card.transform.SetParent(enemyDiscardArea.transform, false);
                    enemyDiscard.Add(card);
                    Debug.Log("!adding");
                }
                foreach (PlayerManager p in gameManager.players)
                {
                    if (p.enemyField.Contains(card))
                    {
                        card.GetComponent<CardBehaviour>().card.currentZone = "Discard";
                        card.transform.SetParent(enemyDiscardArea.transform, false);
                        p.enemyField.Remove(card);
                        this.GetComponent<Display>().DisplayHorizontal(p.enemyField, Display.fieldOffset);
                        this.GetComponent<Display>().DisplayHorizontal(p.enemyDiscard, Display.fieldOffset);
                        Debug.Log("!destroyingfield");
                        // this.GetComponent<Display>().DisplayHorizontal(enemyField, Display.fieldOffset);
                        card.transform.Find("Back").gameObject.SetActive(false);
                        card.transform.Find("VFX").Find("Shroud").GetComponent<VisualEffect>().enabled = false;
                        card.transform.Find("HoverImage").gameObject.SetActive(false);
                        gameManager.ResetStats(card);
                    }
                    else if (p.enemyUtility.Contains(card))
                    {
                        card.GetComponent<CardBehaviour>().card.currentZone = "Discard";
                        card.transform.SetParent(enemyDiscardArea.transform, false);
                        p.enemyUtility.Remove(card);
                        this.GetComponent<Display>().DisplayHorizontal(p.enemyUtility, Display.fieldOffset);
                        this.GetComponent<Display>().DisplayHorizontal(p.enemyDiscard, Display.fieldOffset);
                        // this.GetComponent<Display>().DisplayHorizontal(enemyUtility, Display.fieldOffset);
                    }
                }
                this.GetComponent<Display>().DisplayHorizontal(enemyDiscard, Display.fieldOffset);
            }
            else if (action == "Restore")
            {
                card.GetComponent<CardDisplay>().card.currentZone = "Field";
                foreach (PlayerManager p in gameManager.players)
                {
                    if (card.GetComponent<CardDisplay>().card.cardType == Card.CardType.Henchman)
                    {
                        card.transform.SetParent(enemyFieldArea.transform, false);
                        card.GetComponent<CardDisplay>().card.health = card.GetComponent<CardDisplay>().card.originalHealth;
                        card.GetComponent<CardDisplay>().SetCardProperties();
                        p.enemyField.Add(card);
                    }
                    else if (card.GetComponent<CardDisplay>().card.cardType == Card.CardType.Relic)
                    {
                        card.transform.SetParent(enemyUtilityArea.transform, false);
                        card.GetComponent<CardDisplay>().card.health = card.GetComponent<CardDisplay>().card.originalHealth;
                        card.GetComponent<CardDisplay>().SetCardProperties();
                        p.enemyUtility.Add(card);
                    }

                }
                enemyDiscard.Remove(card);
                this.GetComponent<Display>().DisplayHorizontal(enemyDiscard, Display.fieldOffset);
                this.GetComponent<Display>().DisplayHorizontal(enemyField, Display.fieldOffset);
                this.GetComponent<Display>().DisplayHorizontal(enemyUtility, Display.handOffset);
            }
            else if (action == "Exile")
            {
                foreach (PlayerManager p in gameManager.players)
                {
                    p.enemyDiscard.Remove(card);
                }
                Destroy(card);
            }
            else if (action == "Create")
            {
                card.GetComponent<CardDisplay>().card.cardEffect.Create();
                switch (card.GetComponent<CardDisplay>().card.cardType)
                {
                    case Card.CardType.Henchman:
                        card.transform.SetParent(enemyFieldArea.transform, false);
                        enemyField.Add(card);
                        UpdatePlayerUnitCount(1);
                        this.GetComponent<Display>().DisplayHorizontal(enemyField, Display.fieldOffset);
                        break;
                    case Card.CardType.VillainousArt:
                        card.transform.SetParent(enemyHandArea.transform, false);
                        card.transform.Find("Back").gameObject.SetActive(true);
                        enemyHand.Add(card);
                        this.GetComponent<Display>().DisplayHorizontal(enemyHand, Display.fieldOffset);
                        break;
                    case Card.CardType.Relic:
                        card.transform.SetParent(enemyUtilityArea.transform, false);
                        enemyUtility.Add(card);
                        this.GetComponent<Display>().DisplayHorizontal(enemyUtility, Display.fieldOffset);
                        break;
                }
                // card.GetComponent<AnimateCard>().PlayEnemyCard();
            }
            else if (action == "Return")
            {
                if (card.transform.parent == enemyFieldArea.transform)
                {
                    enemyField.Remove(card);
                    GetComponent<Display>().DisplayHorizontal(enemyField, Display.fieldOffset);
                }
                else if (card.transform.parent == enemyUtilityArea.transform)
                {
                    enemyUtility.Remove(card);
                    GetComponent<Display>().DisplayHorizontal(enemyUtility, Display.fieldOffset);
                }
                card.transform.SetParent(enemyHandArea.transform);
                // card.transform.GetChild(2).gameObject.SetActive(true);
                card.GetComponent<CardDisplay>().card.currentZone = "Hand";
                gameManager.ResetStats(card);
                enemyHand.Add(card);
                GetComponent<Display>().DisplayHorizontal(enemyHand, Display.handOffset);
            }

        }
    }
}