using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu]
public class TerryScytheEffect : CardEffect
{
    public override void TurnStart()
    {
        base.TurnStart();
        currentEffect = effect.TurnStart;
        foreach (Transform t in enemyField.transform)
        {

                    t.Find("Indicator").GetComponent<Image>().enabled = true;
                    t.GetComponent<CardBehaviour>().effectSelectable = true;
        }
        gameManager.DisableZone(gameManager.playerManager.playerHandArea);
        gameManager.DisableZone(gameManager.playerManager.playerFieldArea);
        enemyAvatar.GetComponent<PlayerAvatarBehaviour>().effectSelectable = true;
    }
    public override void TurnStartEffect(GameObject target)
    {
        player.blade ++;
        gameManager.Damage(target, player.blade);
        gameManager.PlayVFX(target, "Blade");
        // target.transform.Find("VFX").Find("Blade").GetComponent<ParticleSystem>().Play(true);
        foreach (Transform t in enemyField.transform)
        {

                    t.Find("Indicator").GetComponent<Image>().enabled = false;
                    t.GetComponent<CardBehaviour>().effectSelectable = false;
        }
        enemyAvatar.GetComponent<PlayerAvatarBehaviour>().effectSelectable = false;
        gameManager.EnableZone(gameManager.playerManager.playerHandArea);
        gameManager.EnableZone(gameManager.playerManager.playerFieldArea);
        base.TurnStartEffect(target);
    }
}
