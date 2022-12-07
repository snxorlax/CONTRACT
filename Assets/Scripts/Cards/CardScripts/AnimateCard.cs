using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class AnimateCard : NetworkBehaviour
{
    public Transform cardEffectIndicator, front, back, mainBoard;
    public Card card;
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
    public void DrawPlayerCard()
    {
        if (GetComponent<CardBehaviour>().playerManager.openingDraw)
        {
            // cardAnimator.enabled = true;
            // cardAnimator.Play("Base Layer.DrawCard_Player", -1, 0);
            // Invoke("DisableAnimator", 3);
        }
        else{
            front.gameObject.SetActive(false);
            cardEffectIndicator.GetComponent<CardDisplay>().card = GetComponent<CardDisplay>().card;
            cardEffectIndicator.GetComponent<CardDisplay>().SetCardProperties();
            cardIndicatorAnimator.Play("Base Layer.DrawCard_Player", -1, 0);
            Invoke("CompletePlayerDrawAnimation", drawClipPlayer.length);
        }
    }
    public void DrawEnemyCard()
    {
        if (!GetComponent<CardBehaviour>().playerManager.openingDraw)
        {
            front.gameObject.SetActive(false);
            back.gameObject.SetActive(false);
            cardIndicatorAnimator.Play("Base Layer.DrawCard_Enemy", -1, 0);
            Invoke("CompleteEnemyDrawAnimation", drawClipEnemy.length);

        }
    }
    public void DisableAnimator(){
        cardAnimator.enabled = false;
    }

    public void CompletePlayerDrawAnimation()
    {
        front.gameObject.SetActive(true);
        front.position = cardEffectIndicator.position;
        front.localScale = cardEffectIndicator.localScale;
        StartCoroutine(CompletePlayerDraw());
    }
    public IEnumerator CompletePlayerDraw(){
        while (Vector2.Distance(front.localPosition, Vector2.zero) > .01f)
        {
            front.localScale = Vector3.Lerp(front.localScale, new Vector3(1,1,1), .3f);
            front.localPosition = Vector2.Lerp(front.localPosition, Vector2.zero, .1f);
            yield return null;
        }
        gameManager.actionComplete = true;
    }
    public void CompleteEnemyDrawAnimation()
    {
        back.gameObject.SetActive(true);
        back.position = cardEffectIndicator.position;
        back.localScale = cardEffectIndicator.localScale;
        StartCoroutine(CompleteEnemyDraw());
    }
    public IEnumerator CompleteEnemyDraw(){
        while (Vector2.Distance(back.localPosition, Vector2.zero) > .01f)
        {
            back.localScale = Vector3.Lerp(back.localScale, new Vector3(1,1,1), .3f);
            back.localPosition = Vector2.Lerp(back.localPosition, Vector2.zero, .1f);
            yield return null;
        }
        front.gameObject.SetActive(true);
        gameManager.actionComplete = true;
    }
    public void StartPlayerPlay(){
        StartCoroutine(AnimatePlayerPlay());
    }
    public IEnumerator AnimatePlayerPlay(){
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
        StartCoroutine(CompleteAnimatePlayerPlay());
    }
    public IEnumerator CompleteAnimatePlayerPlay(){
        while (Vector2.Distance(front.localPosition, Vector2.zero) > .01f)
        {
            front.localScale = Vector3.Lerp(front.localScale, new Vector3(1,1,1), .3f);
            front.localPosition = Vector2.Lerp(front.localPosition, Vector2.zero, .1f);
            yield return null;
        }
        StopAllCoroutines();
    }
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
    }
}
