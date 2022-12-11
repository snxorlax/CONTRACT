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
    //List of objects waiting to be destroyed;
    public List<GameObject> destroyQueue = new List<GameObject>();
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
        UpdateDeckCount();
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
                DrawCard(2);
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
    //Draw Card Function
    public void DrawCard(int num)
    {
        for (int i = 0; i < num; i++)
        {
            CmdDrawCard();
        }
    }
    [Command(requiresAuthority= false)]
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
    //Coroutine for drawing. In order to start, it will be added to the Action Queue
    public IEnumerator DrawCardPlayer(GameObject card, int cardNo)
    {
        //Reset bools to false to begin coroutine
        actionComplete = false;
        gameManager.actionComplete = false;

        //Draw Card Player
        card.GetComponent<CardDisplay>().card.currentZone = "Hand";
        card.transform.SetParent(playerHandArea.transform, false);
        playerDeck.Remove(card.GetComponent<CardDisplay>().cardCatalogue.CardList[cardNo]);
        UpdateDeckCount();
        playerHand.Add(card);
        card.GetComponent<AnimateCard>().DrawPlayerCard();
        GetComponent<Display>().DisplayHorizontal(playerHand, Display.handOffset);

        //Wait for animation to finish and set GameManager's actionComplete to true
        while (!actionComplete)
        {
            actionComplete = gameManager.actionComplete;
            yield return null;
        }
    }
    public IEnumerator DrawCardEnemy(GameObject card, int cardNo)
    {
        //Reset bools to false to begin coroutine
        actionComplete = false;
        gameManager.actionComplete = false;

        //Draw Card Enemy
        card.transform.SetParent(enemyHandArea.transform, false);
        card.transform.rotation = Quaternion.Euler(0, 0, -180);
        card.transform.Find("Back").gameObject.SetActive(true);
        playerDeck.Remove(card.GetComponent<CardDisplay>().cardCatalogue.CardList[cardNo]);
        UpdateDeckCount();
        enemyHand.Add(card);
        card.GetComponent<AnimateCard>().DrawEnemyCard();
        this.GetComponent<Display>().DisplayHorizontal(enemyHand, Display.enemyHandOffset);

        //Wait for animation to finish and set GameManager's actionComplete to true
        while (!actionComplete)
        {
            actionComplete = gameManager.actionComplete;
            yield return null;
        }
    }
    [Command(requiresAuthority = false)]
    //Called to refresh deck count number
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
        //Checks if you are local player, then associates your deck with playerDeck
        if (isLocalPlayer)
        {
            playerDeckCount.text = playerDeck.Count.ToString();
        }
        foreach (PlayerManager p in gameManager.players)
        {
            //Iterates through players and finds the enemy
            if (p.enemyManager)
            {
                // associates enemy deck with the non local player's deck count
                p.enemyDeckCount.text = p.enemyManager.playerDeck.Count.ToString();
            }
        }
    }
    //Play Card Function
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
    //Action Queue Coroutine
    public IEnumerator PlayCardPlayer(GameObject card, int shroudNum)
    {
        //Must make both bools false to ensure coroutine completes before next one is activated
        actionComplete = false;
        gameManager.actionComplete = false;
        //Play Player Card
        card.GetComponent<CardBehaviour>().SetCard();
        if (shroudNum == 0)
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
        if (shroudNum == 1)
        {
            card.transform.Find("Back").gameObject.SetActive(true);
            card.transform.Find("VFX").Find("Shroud").GetComponent<VisualEffect>().enabled = true;
            card.GetComponent<CardDisplay>().card.shroud = true;
            card.GetComponent<CardDisplay>().SetCardProperties();
            //Placeholder until shroud animation is ready
            gameManager.actionComplete = true;

        }
        playerHand.Remove(card);
        this.GetComponent<Display>().DisplayHorizontal(playerHand, Display.handOffset);
        card.GetComponent<AnimateCard>().StartPlayerPlay();
        // Waits until action is complete and the gameManager's actionComplete bool is true;
        while (!actionComplete)
        {
            actionComplete = gameManager.actionComplete;
            yield return null;
        }
    }
    // Action Queue Coroutine
    public IEnumerator PlayCardEnemy(GameObject card, int shroudNum)
    {
        //Must make both bools false to ensure coroutine completes before next one is activated
        actionComplete = false;
        gameManager.actionComplete = false;

        //Play Enemy Card
        GameObject.Find("PlayZoneIndicator").GetComponent<Image>().enabled = false;
        card.GetComponent<CardDisplay>().card.currentZone = "Field";
        if (shroudNum != 1)
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
        if (shroudNum == 1)
        {
            card.GetComponent<CardDisplay>().card.shroud = true;
            card.transform.Find("Back").gameObject.SetActive(true);
            card.transform.Find("VFX").Find("Shroud").GetComponent<VisualEffect>().enabled = true;
            //Placeholder until shroud animation is ready
            gameManager.actionComplete = true;
        }
        enemyHand.Remove(card);
        this.GetComponent<Display>().DisplayHorizontal(enemyHand, Display.handOffset);

        // Waits until action is complete and the Gamemanager's actionComplete bool is true;
        while (!actionComplete)
        {
            actionComplete = gameManager.actionComplete;
            yield return null;
        }
    }
    //Destroy Card Function
    public void DestroyCard(GameObject card)
    {
        CmdDestroyCard(card);
    }
    [Command(requiresAuthority = false)]
    public void CmdDestroyCard(GameObject card)
    {
        FormatCards(card, 0, "Destroy");
    }
    //Coroutine for Destroying. Must be addeed to Action Queue to activate. Might be better implementation to handle simultaneous destruction.
    public IEnumerator DestroyBatchPlayer()
    {
        Debug.Log(destroyQueue.Count);
        //Must make both bools false to ensure coroutine completes before next one is activated
        actionComplete = false;
        gameManager.actionComplete = false;
        for (int i = 0; i < destroyQueue.Count; i++)
        {
            GameObject card = destroyQueue[i];
            card.GetComponent<AnimateCard>().StartDestroyCard();
            card.GetComponent<CardBehaviour>().card.currentZone = "Discard";
            if (card.GetComponent<NetworkIdentity>().hasAuthority)
            {
                card.transform.SetParent(playerDiscardArea.transform, false);
                playerDiscard.Add(card);
            }
            else
            {
                card.transform.SetParent(enemyDiscardArea.transform, false);
                enemyDiscard.Add(card);
            }
            if (playerField.Contains(card))
            {
                playerField.Remove(card);
                card.transform.Find("Back").gameObject.SetActive(false);
                card.transform.Find("VFX").Find("Shroud").GetComponent<VisualEffect>().enabled = false;
                card.transform.Find("Front").Find("StatBoxField").gameObject.SetActive(false);
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
                    p.enemyUtility.Remove(card);
                    this.GetComponent<Display>().DisplayHorizontal(p.enemyUtility, Display.fieldOffset);
                }
            }
            this.GetComponent<Display>().DisplayVertical(playerDiscard, Display.discardOffset);
            this.GetComponent<Display>().DisplayHorizontal(playerField, Display.fieldOffset);
            this.GetComponent<Display>().DisplayHorizontal(playerUtility, Display.fieldOffset);

        }
        destroyQueue.Clear();
        // Waits until action is complete and the Gamemanager's actionComplete bool is true;
        while (destroyQueue.Count > 0)
        {
            yield return null;
        }
    }
    public IEnumerator DestroyBatchEnemy()
    {
        //Must make both bools false to ensure coroutine completes before next one is activated
        actionComplete = false;
        gameManager.actionComplete = false;
        for (int i = 0; i < destroyQueue.Count; i++)
        {
            GameObject card = destroyQueue[i];
            card.GetComponent<AnimateCard>().StartDestroyCard();
            if (!card.GetComponent<NetworkIdentity>().hasAuthority)
            {
                card.GetComponent<CardBehaviour>().card.currentZone = "Discard";
                card.transform.SetParent(enemyDiscardArea.transform, false);
                enemyDiscard.Add(card);
            }
            else if (card.GetComponent<NetworkIdentity>().hasAuthority)
            {
                card.transform.SetParent(playerDiscardArea.transform, false);
                playerDiscard.Add(card);
            }
            if (playerField.Contains(card))
            {
                playerField.Remove(card);
                card.transform.Find("Back").gameObject.SetActive(false);
                card.transform.Find("VFX").Find("Shroud").GetComponent<VisualEffect>().enabled = false;
                card.transform.Find("Front").Find("StatBoxField").gameObject.SetActive(false);
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
                    card.GetComponent<CardBehaviour>().card.currentZone = "Discard";
                    card.transform.SetParent(enemyDiscardArea.transform, false);
                    p.enemyField.Remove(card);
                    this.GetComponent<Display>().DisplayHorizontal(p.enemyField, Display.fieldOffset);
                    this.GetComponent<Display>().DisplayHorizontal(p.enemyDiscard, Display.fieldOffset);
                    Debug.Log("!destroyingfield");
                    // this.GetComponent<Display>().DisplayHorizontal(enemyField, Display.fieldOffset);
                    card.transform.Find("Back").gameObject.SetActive(false);
                    card.transform.Find("VFX").Find("Shroud").GetComponent<VisualEffect>().enabled = false;
                    card.transform.Find("Front").Find("StatBoxField").gameObject.SetActive(false);
                    card.transform.Find("HoverImage").gameObject.SetActive(false);
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
        destroyQueue.Clear();
        // Waits until action is complete and the Gamemanager's actionComplete bool is true;
        while (destroyQueue.Count > 0)
        {
            yield return null;
        }
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
        if (action == "Draw" || action == "Create")
        {
            card.GetComponent<CardDisplay>().card = Instantiate(card.GetComponent<CardDisplay>().cardCatalogue.CardList[num]);
            card.GetComponent<CardDisplay>().card.cardEffect = Instantiate(card.GetComponent<CardDisplay>().card.cardEffect);
        }
        card.GetComponent<CardDisplay>().card.cardEffect.self = card;
        card.GetComponent<CardDisplay>().card.cardEffect.CardSetup();
        card.GetComponent<CardDisplay>().SetCardProperties();
        if (hasAuthority)
        {
            if (action == "Draw")
            {
                actionQueue.Enqueue(DrawCardPlayer(card, num));
            }
            if (action == "Play")
            {
                actionQueue.Enqueue(PlayCardPlayer(card, num));
            }
            if (action == "Destroy")
            {
                destroyQueue.Add(card);
                if (!actionQueue.Contains(DestroyBatchPlayer()))
                {
                    actionQueue.Enqueue(DestroyBatchPlayer());
                }
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
            if (action == "Draw")
            {
                actionQueue.Enqueue(DrawCardEnemy(card, num));
            }
            else if (action == "Play")
            {
                actionQueue.Enqueue(PlayCardEnemy(card, num));
            }
            else if (action == "Destroy")
            {
                destroyQueue.Add(card);
                if (!actionQueue.Contains(DestroyBatchEnemy()))
                {
                    actionQueue.Enqueue(DestroyBatchEnemy());
                }
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
    //Calls command
    public void Combat(GameObject attacker, GameObject defender)
    {
        CmdCombat(attacker, defender);
    }
    [Command(requiresAuthority = false)]
    public void CmdCombat(GameObject attacker, GameObject defender)
    {
        RpcCombat(attacker, defender);
        gameManager.Damage(defender, attacker.GetComponent<CardDisplay>().card.attack);
        if (defender.GetComponent<CardDisplay>())
        {
            gameManager.Damage(attacker, defender.GetComponent<CardDisplay>().card.attack);
        }
    }
    [ClientRpc]
    //Enqueues actions to separate rpcs
    public void RpcCombat(GameObject attacker, GameObject defender)
    {
        if (attacker.GetComponent<NetworkIdentity>().hasAuthority)
        {
            actionQueue.Enqueue(PlayerCombat(attacker, defender));
        }
        else if (!attacker.GetComponent<NetworkIdentity>().hasAuthority)
        {
            actionQueue.Enqueue(EnemyCombat(attacker, defender));

        }
    }
    public IEnumerator PlayerCombat(GameObject attacker, GameObject defender)
    {
        actionComplete = false;
        gameManager.actionComplete = false;
        attacker.GetComponent<AnimateCard>().StartAttack(attacker, defender);
        while(!actionComplete)
        {
            actionComplete = gameManager.actionComplete;
            yield return null;
        }
    }
    public IEnumerator EnemyCombat(GameObject attacker, GameObject defender)
    {
        actionComplete = false;
        gameManager.actionComplete = false;
        attacker.GetComponent<AnimateCard>().StartAttack(attacker, defender);
        while(!actionComplete)
        {
            actionComplete = gameManager.actionComplete;
            yield return null;
        }
    }
}