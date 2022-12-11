using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[CreateAssetMenu(menuName ="CardEffects/DreamVialEffect")]
public class DreamVialEffect : CardEffect
{
    public override void ActivatedEffect_1()
    {
        base.ActivatedEffect_1();
        
        foreach (Transform t in playerField.transform)
        {
            t.Find("Indicator").GetComponent<Image>().enabled = true;
            t.GetComponent<CardBehaviour>().effectSelectable = true;
        }
        gameManager.DisableZone(gameManager.playerManager.playerHandArea);
        gameManager.DisableZone(gameManager.playerManager.playerUtilityArea);

    }
    public override void Effect1(GameObject target)
    {
        player.ReturnCard(target);
        player.DrawCard(1);
        player.DestroyCard(self);
        base.Effect1(target);
        foreach (Transform t in playerField.transform)
        {
            t.Find("Indicator").GetComponent<Image>().enabled = false;
            t.GetComponent<CardBehaviour>().effectSelectable = false;
        }
        gameManager.EnableZone(gameManager.playerManager.playerHandArea);
        gameManager.EnableZone(gameManager.playerManager.playerUtilityArea);
    }
}
