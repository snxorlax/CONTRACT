using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Mirror;
using TMPro;
using UnityEngine.VFX;

//classes for events
public class PlayerEvent : UnityEvent<PlayerManager>{}
public class CardEvent : UnityEvent<Card>{}
public class ContinuousEvent : UnityEvent{}
public class DamageEvent : UnityEvent<Card>{}
public class GraveyardEvent: UnityEvent{}
public class GameManager : NetworkBehaviour
{
    public readonly SyncList<PlayerManager> players = new SyncList<PlayerManager>();
    [SyncVar]
    public int turnNumber;
    public PlayerManager playerManager;
    public PlayerManager currentTurn;
    public NetworkManager NetworkManager;
    public GameObject mainCanvas;
    public GameObject player, enemy;
    public GameObject cardIndicator;
    public Animator cardIndicatorAnimator;
    
    // Action Queue for separating animations, events, actions, etc.
    public Queue<IEnumerator> actionQueue = new Queue<IEnumerator>();
    // bool for playing through Queue
    public bool actionQueuePlaying = false;
    // bool for current action completion
    public bool actionComplete;

    //Events
    public PlayerEvent OnTurnStart = new PlayerEvent();
    public CardEvent OnPlay = new CardEvent();
    public ContinuousEvent UnitEvent = new ContinuousEvent();
    public GraveyardEvent GraveyardUpdate = new GraveyardEvent();


    private void Start()
    {
        mainCanvas = GameObject.Find("MainCanvas");
        // player = GameObject.Find("PlayerAvatar");
        // enemy = GameObject.Find("EnemyAvatar");
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        playerManager = networkIdentity.GetComponent<PlayerManager>();
        cardIndicator = GameObject.Find("CurrentCardIndicator");
        cardIndicatorAnimator = cardIndicator.GetComponent<Animator>();
        DelayStartGame();
        players.Callback += OnPlayersUpdated;
        GraveyardUpdate.AddListener(UpdatePoison);
        //Coroutine is always on, can be triggered to loop through Action Queue
        StartCoroutine(PlayActionQueue());
        
    }
    //Maybe unnecessary
    public void ActivateActionQueue()
    {
        actionQueuePlaying = true;
        foreach (IEnumerator e in actionQueue)
        {
            Debug.Log(e);
        }
    }

    //Coroutine for looping through Action Queue
    public IEnumerator PlayActionQueue()
    {
        while (true)
        {
            while (actionQueue.Count > 0)
            {
                yield return StartCoroutine(actionQueue.Dequeue());
            }
            yield return null;
        }
    }
    public void OnPlayersUpdated(SyncList<PlayerManager>.Operation op, int index, PlayerManager oldPlayer, PlayerManager newPlayer)
    {
        foreach (PlayerManager p in players)
        {
            if (playerManager.isLocalPlayer)
            {
                if (!p.isLocalPlayer)
                {
                    playerManager.enemyManager = p;
                }
            }
        }
    }
    public void DelayStartGame()
    {
        Invoke("StartGame", 4);
    }
    public void StartGame()
    {
        OnTurnStart.AddListener(StartTurn);
        OnPlay.AddListener(PlayCard);
        playerManager.UpdatePlayerList();
        int playerNum = Random.Range(0,1);
        playerManager.StartPlayer();
        AssignFirstTurn();
    }

    //Example TurnStart Callback
    public void StartTurn(PlayerManager player)
    {
        player.StartTurn();
    }

    public void PlayCard(Card card)
    {
        card.cardEffect?.Play();
    }

    public void AssignFirstTurn()
    {
        if (players.Count >= 1)
        {
            players[0].ChangeTurn();
        }
    }
    [Command(requiresAuthority = false)]
    public void CmdUpdateTurnNumber()
    {
        turnNumber++;
    }
    public void ChangeTurnClick()
    {
        if (playerManager.isTurn && playerManager.currentEffect.Count == 0)
        {
            ChangeTurn();
        }
    }
    public void ChangeTurn()
    {
        CmdUpdateTurnNumber();
        if (isServer)
        {
            RpcChangeTurn();
        }
        else if (isClient)
        {
            CmdChangeTurn();
        }
    }
    [Command(requiresAuthority = false)]
    public void CmdChangeTurn()
    {
        RpcChangeTurn();
    }
    [ClientRpc]
    public void RpcChangeTurn()
    {
        // Debug.Log(playerManager.GetComponent<NetworkIdentity>().isLocalPlayer);
        // Debug.Log(playerManager.isTurn);
        if (playerManager.GetComponent<NetworkIdentity>().isLocalPlayer && !playerManager.isTurn)
        {
            OnTurnStart?.Invoke(playerManager);
        }
        playerManager.ChangeTurn();
    }
    public void EndGame()
    {
        Debug.Log("Game Over");
    }
    public void PlayVFX(GameObject target, string effect)
    {
        CmdPlayVFX(target, effect);
    }
    [Command(requiresAuthority = false)]
    public void CmdPlayVFX(GameObject target, string effect)
    {
        RpcPlayVFX(target, effect);
    }
    [ClientRpc]
    public void RpcPlayVFX(GameObject target, string effect)
    {
        target.transform.Find("VFX").Find(effect).GetComponent<ParticleSystem>().Play(true);
    }
    public void DeactivateShroud(GameObject card)
    {
        CmdDeactivateShroud(card);
    }
    [Command(requiresAuthority=false)]
    public void CmdDeactivateShroud(GameObject card)
    {
        RpcDeactivateShroud(card);
    }
    [ClientRpc]
    public void RpcDeactivateShroud(GameObject card)
    {
        card.transform.Find("VFX").Find("Shroud").GetComponent<VisualEffect>().enabled = false;
        card.transform.GetChild(2).gameObject.SetActive(false);
        if (!hasAuthority)
        {
            card.transform.rotation = Quaternion.identity;
        }
    }
    public void AnimateStats(GameObject target, string val)
    {
        if (target.transform.Find("AnimatedStats"))
        {
            target.transform.Find("AnimatedStats").GetComponent<TextMeshProUGUI>().text = val;
            target.transform.Find("AnimatedStats").GetComponent<AnimateText>().StartAnimation();
        }
        else if (target.transform.Find("AnimationOffset"))
        {
            target.transform.Find("AnimationOffset").Find("AnimatedStats").GetComponent<TextMeshProUGUI>().text = val;
            target.transform.Find("AnimationOffset").Find("AnimatedStats").GetComponent<AnimateText>().StartAnimation();
        }

    }
    public void Damage(GameObject target, int amount)
    {
        CmdDamage(target, amount);
    }
    [Command(requiresAuthority = false)]
    public void CmdDamage(GameObject target, int amount)
    {
        RpcDamage(target, amount);
    }
    [ClientRpc]
    public void RpcDamage(GameObject target, int amount)
    {
        if (target.GetComponent<CardDisplay>())
        {
            target.GetComponent<CardDisplay>().card.health -= amount;
            if (amount > 0 )
            {
                AnimateStats(target, "-" + amount);
            }
            else if (amount < 0)
            {
                AnimateStats(target, "+" + Mathf.Abs(amount));
            }
            if (target.GetComponent<CardDisplay>().card.health <= 0)
            {
                playerManager.QueueDestroy(target);
            }
            if (target.GetComponent<CardDisplay>().card.shroud && target.GetComponent<NetworkIdentity>().hasAuthority)
            {
                target.GetComponent<CardDisplay>().card.cardEffect.Shroud();
            }
            target.GetComponent<CardDisplay>().SetCardProperties();
        }
        else if (target.GetComponent<PlayerDisplay>())
        {
            if (!target.GetComponent<NetworkIdentity>().hasAuthority)
            {
                target.GetComponent<PlayerDisplay>().playerInfo.lifeTotal -= amount;
                AnimateStats(target, "-" + amount);
                target.GetComponent<PlayerDisplay>().SetPlayerProperties();
            }
            else if (target.GetComponent<NetworkIdentity>().hasAuthority)
            {
                player.GetComponent<PlayerDisplay>().playerInfo.lifeTotal -= amount;
                AnimateStats(player, "-" + amount);
                player.GetComponent<PlayerDisplay>().SetPlayerProperties();
            }
        }
    }
    public void ChangeStats(GameObject target, int attack, int health)
    {
        CmdChangeStats(target, attack, health);
    }
    [Command(requiresAuthority = false)]
    public void CmdChangeStats(GameObject target, int attack, int health)
    {
        RpcChangeStats(target, attack, health);
    }
    [ClientRpc]
    public void RpcChangeStats(GameObject target, int attack, int health)
    {
        string attackMessage = "";
        string healthMessage = "";
        target.GetComponent<CardDisplay>().card.attack += attack;
        if (target.GetComponent<CardDisplay>().card.attack < 0)
        {
            target.GetComponent<CardDisplay>().card.attack = 0;
        }
        target.GetComponent<CardDisplay>().card.health += health;
        if (target.GetComponent<CardDisplay>().card.health < 0)
        {
            playerManager.QueueDestroy(target);
        }
        if (attack > 0)
        {
            attackMessage = "+" + attack;
        }
        else if (attack < 0)
        {
            attackMessage = attack.ToString();
        }
        if (health > 0)
        {
            healthMessage = "+" + health;
        }
        else if (health < 0)
        {
            healthMessage = health.ToString();
        }
        if (attack == 0 && health > 0)
        {
            attackMessage = "+" + attack;
        }
        else if (attack == 0 && health < 0)
        {
            attackMessage = "-" + attack;
        }
        if (health == 0 && attack > 0)
        {
            healthMessage = "+" + health;
        }
        else if (health == 0 && attack < 0)
        {
            healthMessage = "-" + health;
        }
        AnimateStats(target, attackMessage + "/" + healthMessage);
        
        if (target.GetComponent<CardDisplay>().card.health <= 0)
        {
            playerManager.QueueDestroy(target);
        }
        target.GetComponent<CardDisplay>().SetCardProperties();
    }
    public void ResetStats(GameObject card)
    {
        CmdResetStats(card);
    }
    [Command(requiresAuthority = false)]
    public void CmdResetStats(GameObject card)
    {
        RpcResetStats(card);
    }
    [ClientRpc]
    public void RpcResetStats(GameObject card)
    {
        Card cardInfo;
        cardInfo = card.GetComponent<CardDisplay>().card;
        card.GetComponent<CardDisplay>().card.attack = cardInfo.originalAttack;
        card.GetComponent<CardDisplay>().card.health = cardInfo.originalHealth;
        card.transform.Find("Front").GetChild(3).gameObject.SetActive(true);
    }
    public void Combat(GameObject attacker, GameObject defender)
    {
        CmdCombat(attacker, defender);
    }
    [Command(requiresAuthority = false)]
    public void CmdCombat(GameObject attacker, GameObject defender)
    {
        Damage(defender, attacker.GetComponent<CardDisplay>().card.attack);
        // RpcCombat(attacker, defender);
        if (defender.GetComponent<CardDisplay>())
        {
            Damage(attacker, defender.GetComponent<CardDisplay>().card.attack);
        }
    }

    public void DisableZone(GameObject zone)
    {
        foreach (CardBehaviour behaviour in zone.GetComponentsInChildren<CardBehaviour>())
        {
            behaviour.interactable = false;
        }
    }
    public void EnableZone(GameObject zone)
    {
        foreach (CardBehaviour behaviour in zone.GetComponentsInChildren<CardBehaviour>())
        {
            behaviour.interactable = true;
        }
    }
    public void AnimateIndicator(Card card)
    {
        cardIndicator.GetComponent<CardDisplay>().card = card;
        cardIndicator.GetComponent<CardDisplay>().SetCardProperties();
        cardIndicator.transform.GetChild(1).gameObject.SetActive(true);
        cardIndicatorAnimator.Play("Base Layer.cardeffectindicator", -1, 0);
    }
    public int CaluculatePoison()
    {
        int poison = 0;
        List<Card.CardType> cardTypes = new List<Card.CardType>();

        foreach (PlayerManager p in players)
        {
            foreach (Transform t in p.enemyDiscardArea.transform)
            {
                Card.CardType currentType = t.GetComponent<CardDisplay>().card.cardType;
                if (!cardTypes.Contains(currentType))
                {
                    cardTypes.Add(currentType);
                }
            }
        }
        poison = cardTypes.Count;
        return poison;
    }
    public void UpdatePoison(){

    }
    [Command]
    public void CmdUpdatePoison(){
        RpcUpdatePoison();
    }
    [ClientRpc]
    public void RpcUpdatePoison()
    {
        foreach(PlayerManager p in players)
        {
            foreach (GameObject g in p.playerField)
            {
                int currentAttack = g.GetComponent<CardDisplay>().card.attack;
                int originalHealth = g.GetComponent<CardDisplay>().card.originalAttack;
                int currentPoison = CaluculatePoison();

            }
        }
    }
}
