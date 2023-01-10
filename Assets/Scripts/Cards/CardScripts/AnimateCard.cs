using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class AnimateCard : NetworkBehaviour
{
    public Card card;
    // Helper object for animations
    public Transform animHelper;
    // Animator used by animHelper
    public Animator helpAnimator;
    //Transforms for changing views
    public Transform handFront, back, fieldFront;
    public Transform mainBoard;
    //Prefab for destroying and creating
    public GameObject destructionFX;
    //Material for instantiating different dissolving art
    public PlayerManager playerManager;
    public GameManager gameManager;
    
    public AnimationClip drawClipPlayer, drawClipEnemy, playClipEnemy;
    public Canvas canvas;

    private void Awake()
    {
        card = GetComponent<CardDisplay>().card;

        animHelper = GameObject.Find("AnimHelper").transform;
        
        helpAnimator = animHelper.GetComponent<Animator>();
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
        handFront.gameObject.SetActive(false);
        animHelper.GetComponent<CardDisplay>().card = card;
        animHelper.GetComponent<CardDisplay>().SetCardProperties();
        helpAnimator.Play("Base Layer.DrawCard_Player", -1, 0);
        Invoke("CompletePlayerDraw", drawClipPlayer.length);
    }
    public void CompletePlayerDraw()
    {
        handFront.gameObject.SetActive(true);
        handFront.position = animHelper.position;
        handFront.localScale = animHelper.localScale;
        StartCoroutine(CompletePlayerDrawAnimation());
    }
    public IEnumerator CompletePlayerDrawAnimation(){
        while (Vector2.Distance(handFront.localPosition, Vector2.zero) > .01f)
        {
            handFront.localScale = Vector3.Lerp(handFront.localScale, new Vector3(1,1,1), .3f);
            handFront.localPosition = Vector2.Lerp(handFront.localPosition, Vector2.zero, .1f);
            yield return null;
        }
        CompleteAction();
    }

    //Enemy Draw in 3 Parts
    public void DrawEnemyCard()
    {
        handFront.gameObject.SetActive(false);
        back.gameObject.SetActive(false);
        helpAnimator.Play("Base Layer.DrawCard_Enemy", -1, 0);
        Invoke("CompleteEnemyDraw", drawClipEnemy.length);
    }
    public void CompleteEnemyDraw()
    {
        back.gameObject.SetActive(true);
        back.position = animHelper.position;
        back.localScale = animHelper.localScale;
        StartCoroutine(CompleteEnemyDrawAnimation());
    }
    public IEnumerator CompleteEnemyDrawAnimation(){
        while (Vector2.Distance(back.localPosition, Vector2.zero) > .01f)
        {
            back.localScale = Vector3.Lerp(back.localScale, new Vector3(1,1,1), .3f);
            back.localPosition = Vector2.Lerp(back.localPosition, Vector2.zero, .1f);
            yield return null;
        }
        handFront.gameObject.SetActive(true);
        CompleteAction();
    }

    //Animate Player PlayCard in 4 Parts
    public void StartPlayerPlay(){
        StartCoroutine(AnimateStartPlayerPlay());
    }
    public IEnumerator AnimateStartPlayerPlay(){
        float scaleFactor = 1.5f / playerManager.playerFieldArea.transform.localScale.x;
        while (Vector2.Distance(handFront.position, mainBoard.position)> .01f || handFront.localScale.x - scaleFactor > .01f)
        {
            handFront.localScale = Vector3.Lerp(handFront.localScale, new Vector3(scaleFactor, scaleFactor, scaleFactor), .2f);
            handFront.position = Vector2.Lerp(handFront.position, mainBoard.position, .06f);
            yield return null;
        }
        Invoke("CompletePlayerPlay", .8f);
    }
    public void CompletePlayerPlay()
    {
        //Switches from handview to fieldview
        handFront.gameObject.SetActive(false);
        fieldFront.gameObject.SetActive(true);
        fieldFront.transform.localScale = handFront.transform.localScale;
        fieldFront.transform.localPosition = handFront.transform.localPosition;
        handFront.transform.localScale = new Vector3(1,1,1);
        GameObject.Find("PlayZoneIndicator").GetComponent<Image>().enabled = false;
        if (!card)
        {
            card = GetComponent<CardDisplay>().card;
        }
        StartCoroutine(AnimateCompletePlayerPlay());
    }
    public IEnumerator AnimateCompletePlayerPlay(){
        while (Vector2.Distance(fieldFront.localPosition, Vector2.zero) > .01f || fieldFront.localScale != new Vector3(1,1,1))
        {
            fieldFront.localScale = Vector3.Lerp(fieldFront.localScale, new Vector3(1,1,1), .3f);
            fieldFront.localPosition = Vector2.Lerp(fieldFront.localPosition, Vector2.zero, .1f);
            yield return null;
        }
        CompleteAction();
    }
    //Complete Enemy Play Card in 3 Steps
    public void PlayEnemyCard()
    {
        handFront.gameObject.SetActive(false);
        back.gameObject.SetActive(false);
        animHelper.GetComponent<CardDisplay>().card = GetComponent<CardDisplay>().card;
        animHelper.GetComponent<CardDisplay>().SetCardProperties();
        helpAnimator.Play("Base Layer.PlayCard_Enemy", -1, 0);
        Invoke("CompleteEnemyPlay", playClipEnemy.length);
    }
    public void CompleteEnemyPlay()
    {
        //switches view from handview to fieldview
        fieldFront.gameObject.SetActive(true);
        fieldFront.localScale = animHelper.transform.Find("Views").Find("HandView").Find("Front").localScale;
        transform.rotation = Quaternion.identity;
        fieldFront.position = animHelper.position;
        if (!card)
        {
            card = GetComponent<CardDisplay>().card;
        }
        StartCoroutine(CompleteEnemyPlayAnimation());
    }
    public IEnumerator CompleteEnemyPlayAnimation(){
        while (Vector2.Distance(fieldFront.localPosition, Vector2.zero) > .01f)
        {
            fieldFront.localScale = Vector3.Lerp(fieldFront.localScale, new Vector3(1,1,1), .3f);
            fieldFront.localPosition = Vector2.Lerp(fieldFront.localPosition, Vector2.zero, .1f);
            yield return null;
        }
        CompleteAction();
    }

    //Destroys card, currently in 2 parts
    public void StartDestroyCard()
    {
        destructionFX.transform.Find("TopLeft").Find("Unit").gameObject.SetActive(true);
        destructionFX.transform.Find("BottomRight").Find("Unit").gameObject.SetActive(true);
        fieldFront.gameObject.SetActive(false);
        destructionFX.GetComponent<Animator>().Play("Base Layer.CardDestroyAnimation", -1, 0);
    }
    public IEnumerator CompleteDestroyCard()
    {
        while (false)
        {
            yield return null;
        }
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
