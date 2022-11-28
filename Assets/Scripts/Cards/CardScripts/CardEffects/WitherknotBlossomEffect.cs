using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[CreateAssetMenu]
public class WitherknotBlossomEffect : CardEffect
{
    public GameObject playerDiscard;
    public override void Play()
    {
        base.Play();
        playerDiscard = GameObject.Find("PlayerDiscard");
        playerDiscard.transform.SetAsLastSibling();
        //Should update to check for valid targets first, and return if none is available;
        foreach (Transform t in playerDiscard.transform)
        {
                if (t.Find("Indicator"))
                {
                    t.Find("Indicator").GetComponent<Image>().enabled = true;
                    t.GetComponent<CardBehaviour>().effectSelectable = true;
                }
        }
    }
    public override void PlayEffect(GameObject target)
    {
        player.RestoreCard(target);
        playerDiscard.transform.SetAsFirstSibling();
        base.PlayEffect(target);
    }
}
