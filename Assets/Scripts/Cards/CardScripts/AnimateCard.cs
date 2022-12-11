using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class AnimateCard : NetworkBehaviour
{
    public Transform cardEffectIndicator, front, back, mainBoard;
    public Card card;
    //Prefab for destroying and creating
    public GameObject cardDissolve;
    //Material for instantiating different dissolving art
    public Material artDissolve;
    public PlayerManager playerManager;
    public GameManager gameManager;
    
    public Animator cardAnimator, cardIndicatorAnimator;
    public AnimationClip drawClipPlayer, drawClipEnemy, playClipEnemy;
    public Canvas canvas;

    private void Awake()
    {
        card = GetComponent<CardDisplay>().card;
        cardAnimator = GetComponent<Animator>();
        front = transform.Find("Front");
        back = transform.Find("Back");
        cardEffectIndicator = GameObject.Find("CurrentCardIndicator").transform;
        
        cardIndicatorAnimator = cardEffectIndicator.GetComponent<Animator>();
        mainBoard = GameObject.Find("MainBoard").transform;
        canvas = GameObject.Find("MainCanvas").GetComponent<Canvas>();
        
    }
    private void OnEnable()
    {
        playerManager = NetworkClient.connection.identity.GetComponent<PlayerManager>();
        gameManager = playerManager.gameManager;
    }


    //Player Draw in 3 Parts
    public void DrawPlayerCard()
    {
        front.gameObject.SetActive(false);
        cardEffectIndicator.GetComponent<CardDisplay>().card = card;
        cardEffectIndicator.GetComponent<CardDisplay>().SetCardProperties();
        cardIndicatorAnimator.Play("Base Layer.DrawCard_Player", -1, 0);
        Invoke("CompletePlayerDraw", drawClipPlayer.length);
    }
    public void CompletePlayerDraw()
    {
        front.gameObject.SetActive(true);
        front.position = cardEffectIndicator.position;
        front.localScale = cardEffectIndicator.localScale;
        StartCoroutine(CompletePlayerDrawAnimation());
    }
    public IEnumerator CompletePlayerDrawAnimation(){
        while (Vector2.Distance(front.localPosition, Vector2.zero) > .01f)
        {
            front.localScale = Vector3.Lerp(front.localScale, new Vector3(1,1,1), .3f);
            front.localPosition = Vector2.Lerp(front.localPosition, Vector2.zero, .1f);
            yield return null;
        }
        CompleteAction();
    }

    //Enemy Draw in 3 Parts
    public void DrawEnemyCard()
    {
        front.gameObject.SetActive(false);
        back.gameObject.SetActive(false);
        cardIndicatorAnimator.Play("Base Layer.DrawCard_Enemy", -1, 0);
        Invoke("CompleteEnemyDraw", drawClipEnemy.length);
    }
    public void CompleteEnemyDraw()
    {
        back.gameObject.SetActive(true);
        back.position = cardEffectIndicator.position;
        back.localScale = cardEffectIndicator.localScale;
        StartCoroutine(CompleteEnemyDrawAnimation());
    }
    public IEnumerator CompleteEnemyDrawAnimation(){
        while (Vector2.Distance(back.localPosition, Vector2.zero) > .01f)
        {
            back.localScale = Vector3.Lerp(back.localScale, new Vector3(1,1,1), .3f);
            back.localPosition = Vector2.Lerp(back.localPosition, Vector2.zero, .1f);
            yield return null;
        }
        front.gameObject.SetActive(true);
        CompleteAction();
    }
    //Disable animator once it's not useful
    public void DisableAnimator(){
        cardAnimator.enabled = false;
    }

    //Animate Player PlayCard in 4 Parts
    public void StartPlayerPlay(){
        StartCoroutine(AnimateStartPlayerPlay());
    }
    public IEnumerator AnimateStartPlayerPlay(){
        while (Vector2.Distance(front.position, mainBoard.position)> .01f)
        {
            front.localScale = Vector3.Lerp(front.localScale, new Vector3(2.3f,2.3f,2.3f), .3f);
            front.position = Vector2.Lerp(front.position, mainBoard.position, .1f);
            yield return null;
        }
        Invoke("CompletePlayerPlay", .8f);
    }
    public void CompletePlayerPlay()
    {
        transform.Find("Front").Find("Text").gameObject.SetActive(false);
        GameObject.Find("PlayZoneIndicator").GetComponent<Image>().enabled = false;
        if (!card)
        {
            card = GetComponent<CardDisplay>().card;
        }
        if (card.cardType == Card.CardType.Henchman || card.cardType == Card.CardType.Villain)
        {
            GetComponent<CardDisplay>().statBoxField.gameObject.SetActive(true);
        }
        StartCoroutine(AnimateCompletePlayerPlay());
    }
    public IEnumerator AnimateCompletePlayerPlay(){
        while (Vector2.Distance(front.localPosition, Vector2.zero) > .01f)
        {
            front.localScale = Vector3.Lerp(front.localScale, new Vector3(1,1,1), .3f);
            front.localPosition = Vector2.Lerp(front.localPosition, Vector2.zero, .1f);
            yield return null;
        }
        CompleteAction();
        // StopAllCoroutines();
    }
    //Complete Enemy Play Card in 3 Steps
    public void PlayEnemyCard()
    {
        front.gameObject.SetActive(false);
        back.gameObject.SetActive(false);
        cardEffectIndicator.GetComponent<CardDisplay>().card = GetComponent<CardDisplay>().card;
        cardEffectIndicator.GetComponent<CardDisplay>().SetCardProperties();
        cardIndicatorAnimator.Play("Base Layer.PlayCard_Enemy", -1, 0);
        Invoke("CompleteEnemyPlay", playClipEnemy.length);
    }
    public void CompleteEnemyPlay()
    {
        front.gameObject.SetActive(true);
        front.localScale = cardEffectIndicator.transform.Find("Front").localScale;
        transform.rotation = Quaternion.identity;
        front.position = cardEffectIndicator.position;
        transform.Find("Front").Find("Text").gameObject.SetActive(false);
        if (!card)
        {
            card = GetComponent<CardDisplay>().card;
        }
        if (card.cardType == Card.CardType.Henchman || card.cardType == Card.CardType.Villain)
        {
            GetComponent<CardDisplay>().statBoxField.gameObject.SetActive(true);
        }
        StartCoroutine(CompleteEnemyPlayAnimation());
    }
    public IEnumerator CompleteEnemyPlayAnimation(){
        while (Vector2.Distance(front.localPosition, Vector2.zero) > .01f)
        {
            front.localScale = Vector3.Lerp(front.localScale, new Vector3(1,1,1), .3f);
            front.localPosition = Vector2.Lerp(front.localPosition, Vector2.zero, .1f);
            yield return null;
        }
        CompleteAction();
    }

    //Destroys card, currently in 2 parts
    public void StartDestroyCard()
    {
        GameObject cardDestroyed = Instantiate(cardDissolve, transform.position, Quaternion.identity, transform.parent);
        cardDestroyed.transform.localScale = transform.localScale;
        cardDestroyed.GetComponent<CardDisplay>().card = card;
        cardDestroyed.GetComponent<CardDisplay>().SetCardProperties();
        cardDestroyed.transform.Find("Front").Find("Art").GetComponent<Image>().material = new Material(artDissolve);
        cardDestroyed.transform.Find("Front").Find("Art").GetComponent<Image>().material.SetTexture("MainTexture", GetComponent<CardDisplay>().artCatalogue.cardArt[GetComponent<CardDisplay>().card.cardNo].texture);
        StartCoroutine(AnimateDestroyCard(cardDestroyed));
    }
    public IEnumerator AnimateDestroyCard(GameObject card)
    {
        float dissolveAmount = 0;
        float clipThreshold = 0;
        Material dissolveMat = card.transform.Find("Front").Find("Art").GetComponent<Image>().material;
        Material destroyMat = cardDissolve.transform.Find("Front").Find("CardBackground").GetComponent<Image>().material;
        while (dissolveMat.GetFloat("DissolveAmount") < 3 )
        {
            dissolveMat.SetFloat("DissolveAmount", dissolveAmount);
            destroyMat.SetFloat("ClipThreshold", clipThreshold);
            dissolveAmount += .05f;
            clipThreshold += .05f;
            yield return null;
        }
        Destroy(card);
        dissolveMat.SetFloat("DissolveAmount", 0);
        destroyMat.SetFloat("ClipThreshold", 0);
        gameManager.ResetStats(gameObject);
    }
    public void StartAttack(GameObject attacker, GameObject defender)
    {
        StartCoroutine(AttackAnimation(attacker, defender));
    }

    public IEnumerator AttackAnimation(GameObject attacker, GameObject defender)
    {
        Vector2 originalPos = attacker.transform.position;
        for (int i = 0; i < 10; i++)
        {
            attacker.transform.position = Vector2.Lerp(attacker.transform.position, defender.transform.position, .025f);

            yield return null;
        }
        StartCoroutine(ResetAttacker(attacker, originalPos));
    }
    public IEnumerator ResetAttacker(GameObject attacker, Vector2 originalPos)
    {
        while (Vector2.Distance((Vector2)attacker.transform.position, originalPos) > .01f && attacker.GetComponent<CardDisplay>().card.currentZone == "Field")
        {
            attacker.transform.position = Vector2.Lerp(attacker.transform.position, originalPos, .02f);
            yield return null;
        }
        CompleteAction();
    }

    //Placeholder function to complete actions. Will probably replace with event in the future to decouple.
    public void CompleteAction()
    {
        gameManager.actionComplete = true;
    }
}
