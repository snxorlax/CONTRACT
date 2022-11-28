using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateCard : MonoBehaviour
{
    public Transform cardEffectIndicator, front;
    public Animator cardAnimator, cardIndicatorAnimator;
    public AnimationClip drawClip;

    private void Awake()
    {
        cardAnimator = GetComponent<Animator>();
        front = transform.Find("Front");
        cardEffectIndicator = GameObject.Find("CurrentCardIndicator").transform;
        
        cardIndicatorAnimator = cardEffectIndicator.GetComponent<Animator>();
        
    }
    public void PlayEnemyCard()
    {
        cardAnimator.enabled = true;
        cardAnimator.Play("Base Layer.PlayCard_Enemy", -1, 0);
    }
    public void DrawPlayerCard()
    {
        if (GetComponent<CardBehaviour>().playerManager.openingDraw)
        {
            cardAnimator.enabled = true;
            cardAnimator.Play("Base Layer.DrawCard_Player", -1, 0);
            Invoke("DisableAnimator", 2);
        }
        else{
            front.gameObject.SetActive(false);
            cardEffectIndicator.GetComponent<CardDisplay>().card = GetComponent<CardDisplay>().card;
            cardEffectIndicator.GetComponent<CardDisplay>().SetCardProperties();
            cardIndicatorAnimator.Play("Base Layer.DrawCard", -1, 0);
            Invoke("CompletePlayerDrawAnimation", drawClip.length);
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
        StartCoroutine(CompleteDraw());
    }
    public IEnumerator CompleteDraw(){
        while (Vector2.Distance(front.localPosition, Vector2.zero) > .01f)
        {
            front.localScale = Vector3.Lerp(front.localScale, new Vector3(1,1,1), .3f);
            front.localPosition = Vector2.Lerp(front.localPosition, Vector2.zero, .1f);
            yield return null;
        }
    }

}
