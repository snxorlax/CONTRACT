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
    //Set to true when client is ready to start the game
    public bool startGame;
    //Action Queue
    public Queue<IEnumerator> actionQueue;
    //Action Queue Bool
    public bool actionComplete = false;
    //List of objects waiting to be destroyed;
    public List<GameObject> destroyQueue = new List<GameObject>();
    //Work in Progress Lists
    public List<Card> playerDeck = new List<Card>();
    public List<Card> enemyDeck = new List<Card>();
    public List<GameObject> hand = new List<GameObject>();
    public List<GameObject> field = new List<GameObject>();
    public List<GameObject> utility = new List<GameObject>();
    public List<GameObject> discard = new List<GameObject>();
    public readonly SyncList<GameObject> selectedUnits = new SyncList<GameObject>();
    public readonly SyncList<GameObject> selectedRelics = new SyncList<GameObject>();
    public List<GameObject> currentContract = new List<GameObject>();
    public List<GameObject> currentEffect = new List<GameObject>();
    public PlayerManager enemy;

    public Deck deck;
    public TextMeshProUGUI playerDeckCount, enemyDeckCount;
    public GameObject deckIndicator, enemyDeckIndicator;

    //Areas where cards can be spawned, etc.
    public GameObject playerAvatar, playerHandArea, playerDiscardArea, enemyHandArea, playerFieldArea, playerUtilityArea, enemyFieldArea, enemyUtilityArea, enemyDiscardArea;
    public GameObject card, playerAvatarZone, enemyAvatarZone, gameText;
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
        ClientReady();
        //Zones where cards are placed
        playerHandArea = GameObject.Find("PlayerHandArea");
        enemyHandArea = GameObject.Find("EnemyHandArea");
        playerFieldArea = GameObject.Find("PlayerField");
        playerUtilityArea = GameObject.Find("PlayerUtility");
        enemyUtilityArea = GameObject.Find("EnemyUtility");
        enemyFieldArea = GameObject.Find("EnemyField");
        playerDiscardArea = GameObject.Find("PlayerDiscard");
        enemyDiscardArea = GameObject.Find("EnemyDiscard");
        
        playerAvatarZone = GameObject.Find("PlayerAvatarZone");
        enemyAvatarZone = GameObject.Find("EnemyAvatarZone");

        deckIndicator = GameObject.Find("PlayerDeck");
        playerDeckCount = deckIndicator.transform.Find("DeckInfo").Find("DeckCount").GetComponent<TextMeshProUGUI>();

        enemyDeckIndicator = GameObject.Find("EnemyDeck");
        enemyDeckCount = enemyDeckIndicator.transform.Find("DeckInfo").Find("DeckCount").GetComponent<TextMeshProUGUI>();

        gameText = GameObject.Find("GameText");
        hasSummon = true;

    }
    //Called OnStartClient to check if client has connected (client will always connect second)
    public void ClientReady()
    {
        if (!isServer)
        {
            CmdClientReady();
        }
    }
    [Command (requiresAuthority = false)]
    public void CmdClientReady()
    {
        RpcClientReady();
    }
    [ClientRpc]
    //If client has connected, startGame will become true on all clients, and then gameManagers
    public void RpcClientReady()
    {
        startGame = true;
    }
    //Syncvar Hook triggers when certain values are updated -- Unit Count, etc.
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
        GameObject newPlayer = Instantiate(playerAvatar, Vector2.zero, Quaternion.identity, transform);
        newPlayer.GetComponent<PlayerDisplay>().playerInfo = Instantiate(playerInfo);
        newPlayer.GetComponent<PlayerDisplay>().SetPlayerProperties();
        NetworkServer.Spawn(newPlayer, connectionToClient);
        RpcStartPlayer(newPlayer);
    }
    [ClientRpc]
    public void RpcStartPlayer(GameObject playerAvatar)
    {
        playerDeck.AddRange(deck.mainDeck);
        UpdateDeckCount();
        if (hasAuthority)
        {
            playerAvatar.transform.SetParent(playerAvatarZone.transform, false);
            playerAvatar.GetComponent<PlayerDisplay>().playerInfo = Instantiate(playerInfo);
            playerAvatar.GetComponent<PlayerDisplay>().SetPlayerProperties();
            gameManager.playerAvatar = playerAvatar;
        }
        if (!hasAuthority)
        {
            playerAvatar.transform.SetParent(enemyAvatarZone.transform, false);
            playerAvatar.GetComponent<PlayerDisplay>().playerInfo = Instantiate(playerInfo);
            playerAvatar.GetComponent<PlayerDisplay>().SetPlayerProperties();
            gameManager.enemyAvatar = playerAvatar;
        }
        if (isLocalPlayer)
        {
            // QueueDraw(5);
        }
        if (isTurn && isLocalPlayer)
        {
            StartTurn();
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
        foreach(GameObject card in field)
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
        foreach (GameObject g in field)
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
    //Will be called to assign a random player the first turn
    public void SetTurn()
    {
        CmdSetTurn();
    }
    [Command(requiresAuthority = false)]
    public void CmdSetTurn()
    {
        isTurn = true;
    }
    //Called to change turns
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
            }
            UpdateSummonAndAttacks();
        }
    }
    //Adds player to Gamemanager's list of players
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
        hand.Add(card);
        card.GetComponent<AnimateCard>().DrawPlayerCard();
        GetComponent<Display>().DisplayHorizontal(hand, Display.handOffset);

        //Wait for animation to finish and set GameManager's actionComplete to true
        while (!actionComplete)
        {
            actionComplete = gameManager.actionComplete;
            yield return null;
        }
        card.GetComponent<CardBehaviour>().interactable = true;
    }
    public IEnumerator DrawCardEnemy(GameObject card, int cardNo)
    {
        //Reset bools to false to begin coroutine
        actionComplete = false;
        gameManager.actionComplete = false;

        //Draw Card Enemy
        card.transform.SetParent(enemyHandArea.transform, false);
        card.transform.rotation = Quaternion.Euler(0, 0, -180);
        // card.transform.Find("Back").gameObject.SetActive(true);
        playerDeck.Remove(card.GetComponent<CardDisplay>().cardCatalogue.CardList[cardNo]);
        UpdateDeckCount();
        hand.Add(card);
        card.GetComponent<AnimateCard>().DrawEnemyCard();
        this.GetComponent<Display>().DisplayHorizontal(hand, Display.enemyHandOffset);

        //Wait for animation to finish and set GameManager's actionComplete to true
        while (!actionComplete)
        {
            actionComplete = gameManager.actionComplete;
            yield return null;
        }
        card.GetComponent<CardBehaviour>().interactable = true;
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
            if (p.enemy)
            {
                // associates enemy deck with the non local player's deck count
                p.enemyDeckCount.text = p.enemy.playerDeck.Count.ToString();
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
                field.Add(card);
                UpdatePlayerUnitCount(1);
                this.GetComponent<Display>().DisplayHorizontal(field, Display.fieldOffset);
                hasSummon = false;
                break;
            case Card.CardType.VillainousArt:
                card.transform.SetParent(playerUtilityArea.transform, false);
                utility.Add(card);
                this.GetComponent<Display>().DisplayHorizontal(utility, Display.fieldOffset);
                break;
            case Card.CardType.Relic:
                card.transform.SetParent(playerUtilityArea.transform, false);
                utility.Add(card);
                this.GetComponent<Display>().DisplayHorizontal(utility, Display.fieldOffset);
                break;
            case Card.CardType.Villain:
                Debug.Log("Play Villain");
                card.transform.SetParent(playerFieldArea.transform, false);
                field.Add(card);
                UpdatePlayerUnitCount(1);
                this.GetComponent<Display>().DisplayHorizontal(field, Display.fieldOffset);
                break;
        }
        card.GetComponent<CardDisplay>().card.currentZone = "Field";
        if (shroudNum == 1)
        {
            card.transform.Find("Back").gameObject.SetActive(true);
            card.transform.Find("VFX").Find("Shroud").GetComponent<VisualEffect>().enabled = true;
            card.GetComponent<CardDisplay>().card.shroud = true;
            //Placeholder until shroud animation is ready
            gameManager.actionComplete = true;

        }
        hand.Remove(card);
        this.GetComponent<Display>().DisplayHorizontal(hand, Display.handOffset);
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
                field.Add(card);
                this.GetComponent<Display>().DisplayHorizontal(field, Display.fieldOffset);
                break;
            case Card.CardType.VillainousArt:
                card.transform.SetParent(enemyUtilityArea.transform, false);
                utility.Add(card);
                this.GetComponent<Display>().DisplayHorizontal(utility, Display.fieldOffset);
                break;
            case Card.CardType.Relic:
                card.transform.SetParent(enemyUtilityArea.transform, false);
                utility.Add(card);
                this.GetComponent<Display>().DisplayHorizontal(utility, Display.fieldOffset);
                break;
            case Card.CardType.Villain:
                card.transform.SetParent(enemyFieldArea.transform, false);
                field.Add(card);
                this.GetComponent<Display>().DisplayHorizontal(field, Display.fieldOffset);
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
        hand.Remove(card);
        this.GetComponent<Display>().DisplayHorizontal(hand, Display.handOffset);

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
        Debug.Log("hasAuthority");
        //Must make both bools false to ensure coroutine completes before next one is activated
        actionComplete = false;
        gameManager.actionComplete = false;
        for (int i = 0; i < destroyQueue.Count; i++)
        {
            GameObject card = destroyQueue[i];
            Debug.Log(card.GetComponent<NetworkIdentity>().connectionToClient);
            Debug.Log(card.GetComponent<NetworkIdentity>().hasAuthority);
            card.GetComponent<AnimateCard>().StartDestroyCard();
            card.GetComponent<CardBehaviour>().card.currentZone = "Discard";
            if (card.GetComponent<NetworkIdentity>().hasAuthority)
            {
                if (!discard.Contains(card))
                {
                    discard.Add(card);
                }
            }
            else
            {
                if (!enemy.discard.Contains(card))
                {
                    enemy.discard.Add(card);
                }
            }
            if (field.Contains(card))
            {
                field.Remove(card);
                card.transform.Find("VFX").Find("Shroud").GetComponent<VisualEffect>().enabled = false;
                UpdatePlayerUnitCount(-1);
            }
            else if (utility.Contains(card))
            {
                utility.Remove(card);
            }
            if (enemy.field.Contains(card))
            {
                enemy.field.Remove(card);
                GetComponent<Display>().DisplayHorizontal(enemy.field, Display.fieldOffset);
            }
            else if (enemy.utility.Contains(card))
            {
                enemy.utility.Remove(card);
                this.GetComponent<Display>().DisplayHorizontal(enemy.utility, Display.fieldOffset);
            }
            // this.GetComponent<Display>().DisplayVertical(discard, Display.discardOffset);
            // this.GetComponent<Display>().DisplayHorizontal(field, Display.fieldOffset);
            // this.GetComponent<Display>().DisplayHorizontal(utility, Display.fieldOffset);

        }
        gameManager.GraveyardUpdate?.Invoke();
        // Waits until destroyQueue is empty before completing coroutine
        while (destroyQueue.Count > 0)
        {
            yield return null;
        }
    }
    public IEnumerator DestroyBatchEnemy()
    {
        Debug.Log("!hasAuthority");
        // Must make both bools false to ensure coroutine completes before next one is activated
        actionComplete = false;
        gameManager.actionComplete = false;
        for (int i = 0; i < destroyQueue.Count; i++)
        {
            GameObject card = destroyQueue[i];
            card.GetComponent<AnimateCard>().StartDestroyCard();
            Debug.Log(card.GetComponent<NetworkIdentity>().hasAuthority);
            if (card.GetComponent<NetworkIdentity>().hasAuthority)
            {
                if (!enemy.discard.Contains(card))
                {
                    enemy.discard.Add(card);
                }
            }
            else
            {
                if (!discard.Contains(card))
                {
                    discard.Add(card);
                }
            }
            if (field.Contains(card))
            {
                field.Remove(card);
                // card.transform.Find("Back").gameObject.SetActive(false);
                card.transform.Find("VFX").Find("Shroud").GetComponent<VisualEffect>().enabled = false;
                gameManager.ResetStats(card);
                UpdatePlayerUnitCount(-1);
            }
            else if (utility.Contains(card))
            {
                utility.Remove(card);
            }
            if (enemy.field.Contains(card))
            {
                card.GetComponent<CardBehaviour>().card.currentZone = "Discard";
                enemy.field.Remove(card);
                this.GetComponent<Display>().DisplayHorizontal(enemy.field, Display.fieldOffset);
                this.GetComponent<Display>().DisplayHorizontal(enemy.discard, Display.fieldOffset);
                // this.GetComponent<Display>().DisplayHorizontal(enemyField, Display.fieldOffset);
                // card.transform.Find("Back").gameObject.SetActive(false);
                card.transform.Find("VFX").Find("Shroud").GetComponent<VisualEffect>().enabled = false;
            }
                else if (enemy.utility.Contains(card))
                {
                    card.GetComponent<CardBehaviour>().card.currentZone = "Discard";
                    card.transform.SetParent(enemyDiscardArea.transform, false);
                    enemy.utility.Remove(card);
                    this.GetComponent<Display>().DisplayHorizontal(enemy.utility, Display.fieldOffset);
                    this.GetComponent<Display>().DisplayHorizontal(enemy.discard, Display.fieldOffset);
                    // this.GetComponent<Display>().DisplayHorizontal(enemyUtility, Display.fieldOffset);
                }
            this.GetComponent<Display>().DisplayHorizontal(enemy.discard, Display.fieldOffset);

        }
        gameManager.GraveyardUpdate?.Invoke();
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
                // card.GetComponent<CardDisplay>().SetCardProperties();
                if (card.GetComponent<CardDisplay>().card.cardType == Card.CardType.Henchman)
                {
                    card.transform.SetParent(playerFieldArea.transform, false);
                    field.Add(card);
                    UpdatePlayerUnitCount(1);
                }
                else if (card.GetComponent<CardDisplay>().card.cardType == Card.CardType.Relic)
                {
                    card.transform.SetParent(playerUtilityArea.transform, false);
                    utility.Add(card);
                }
                discard.Remove(card);
                this.GetComponent<Display>().DisplayHorizontal(discard, Display.fieldOffset);
                this.GetComponent<Display>().DisplayHorizontal(field, Display.fieldOffset);
                this.GetComponent<Display>().DisplayHorizontal(utility, Display.fieldOffset);
            }
            else if (action == "Exile")
            {
                discard.Remove(card);
                Destroy(card);
                gameManager.GraveyardUpdate?.Invoke();
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
                        field.Add(card);
                        UpdatePlayerUnitCount(1);
                        this.GetComponent<Display>().DisplayHorizontal(field, Display.fieldOffset);
                        break;
                    case Card.CardType.VillainousArt:
                        card.transform.SetParent(playerHandArea.transform, false);
                        hand.Add(card);
                        this.GetComponent<Display>().DisplayHorizontal(hand, Display.fieldOffset);
                        break;
                    case Card.CardType.Relic:
                        card.GetComponent<CardDisplay>().card.currentZone = "Field";
                        card.transform.SetParent(playerUtilityArea.transform, false);
                        utility.Add(card);
                        this.GetComponent<Display>().DisplayHorizontal(utility, Display.fieldOffset);
                        break;
                }
                //Placeholder Animation
                card.GetComponent<AnimateCard>().StartPlayerPlay();
                card.GetComponent<CardBehaviour>().interactable = true;
            }
            else if (action == "Return")
            {
                if (card.transform.parent == playerFieldArea.transform)
                {
                    field.Remove(card);
                    GetComponent<Display>().DisplayHorizontal(field, Display.fieldOffset);
                }
                else if (card.transform.parent == playerUtilityArea.transform)
                {
                    utility.Remove(card);
                    GetComponent<Display>().DisplayHorizontal(utility, Display.fieldOffset);
                }
                card.transform.SetParent(playerHandArea.transform);
                gameManager.ResetStats(card);
                card.GetComponent<CardDisplay>().card.currentZone = "Hand";
                card.transform.Find("HoverImage").gameObject.SetActive(false);
                gameManager.ResetStats(card);
                hand.Add(card);
                GetComponent<Display>().DisplayHorizontal(hand, Display.handOffset);
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
                        card.GetComponent<CardDisplay>().SetStats();
                        enemy.field.Add(card);
                    }
                    else if (card.GetComponent<CardDisplay>().card.cardType == Card.CardType.Relic)
                    {
                        card.transform.SetParent(enemyUtilityArea.transform, false);
                        card.GetComponent<CardDisplay>().card.health = card.GetComponent<CardDisplay>().card.originalHealth;
                        card.GetComponent<CardDisplay>().SetStats();
                        enemy.utility.Add(card);
                    }

                }
                enemy.discard.Remove(card);
                this.GetComponent<Display>().DisplayHorizontal(enemy.discard, Display.fieldOffset);
                this.GetComponent<Display>().DisplayHorizontal(enemy.field, Display.fieldOffset);
                this.GetComponent<Display>().DisplayHorizontal(enemy.utility, Display.fieldOffset);
            }
            else if (action == "Exile")
            {
                foreach (PlayerManager p in gameManager.players)
                {
                    p.enemy.discard.Remove(card);
                }
                Destroy(card);
                gameManager.GraveyardUpdate?.Invoke();
            }
            else if (action == "Create")
            {
                card.GetComponent<CardDisplay>().card.cardEffect.Create();
                switch (card.GetComponent<CardDisplay>().card.cardType)
                {
                    case Card.CardType.Henchman:
                        card.transform.SetParent(enemyFieldArea.transform, false);
                        enemy.field.Add(card);
                        UpdatePlayerUnitCount(1);
                        this.GetComponent<Display>().DisplayHorizontal(enemy.field, Display.fieldOffset);
                        break;
                    case Card.CardType.VillainousArt:
                        card.transform.SetParent(enemyHandArea.transform, false);
                        card.transform.Find("Back").gameObject.SetActive(true);
                        hand.Add(card);
                        this.GetComponent<Display>().DisplayHorizontal(hand, Display.fieldOffset);
                        break;
                    case Card.CardType.Relic:
                        card.transform.SetParent(enemyUtilityArea.transform, false);
                        enemy.utility.Add(card);
                        this.GetComponent<Display>().DisplayHorizontal(enemy.utility, Display.fieldOffset);
                        break;
                }
                // card.GetComponent<AnimateCard>().PlayEnemyCard();
            }
            else if (action == "Return")
            {
                if (card.transform.parent == enemyFieldArea.transform)
                {
                    enemy.field.Remove(card);
                    GetComponent<Display>().DisplayHorizontal(enemy.field, Display.fieldOffset);
                }
                else if (card.transform.parent == enemyUtilityArea.transform)
                {
                    enemy.utility.Remove(card);
                    GetComponent<Display>().DisplayHorizontal(enemy.utility, Display.fieldOffset);
                }
                card.transform.SetParent(enemyHandArea.transform);
                // card.transform.GetChild(2).gameObject.SetActive(true);
                card.GetComponent<CardDisplay>().card.currentZone = "Hand";
                gameManager.ResetStats(card);
                hand.Add(card);
                GetComponent<Display>().DisplayHorizontal(hand, Display.handOffset);
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