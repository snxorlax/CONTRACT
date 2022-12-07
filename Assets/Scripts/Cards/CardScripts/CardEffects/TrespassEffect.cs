using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[CreateAssetMenu(menuName = "CardEffects/TrespassEffect")]
public class TrespassEffect : CardEffect
{
    public override void Play()
    {
        base.Play();
        gameMessage = "Deal " + (player.blade + 1).ToString() + " damage.";
        gameManager.Damage(gameManager.player, 2);
        foreach (Transform t in enemyField.transform)
        {

                    t.Find("Indicator").GetComponent<Image>().enabled = true;
                    t.GetComponent<CardBehaviour>().effectSelectable = true;
        }
        enemyAvatar.GetComponent<PlayerAvatarBehaviour>().effectSelectable = true;
        player.gameText.GetComponent<TextMeshProUGUI>().enabled = true;
        player.gameText.GetComponent<TextMeshProUGUI>().text = gameMessage;
        gameManager.DisableZone(gameManager.playerManager.playerHandArea);
        gameManager.DisableZone(gameManager.playerManager.playerFieldArea);
        gameManager.DisableZone(gameManager.playerManager.playerUtilityArea);
    }

    public override void PlayEffect(GameObject target)
    {
        player.blade ++;
        gameManager.Damage(target, player.blade);
        gameManager.PlayVFX(target, "Blade");
        player.QueueDraw(1);
        // target.transform.Find("VFX").Find("Blade").GetComponent<ParticleSystem>().Play(true);
        foreach (Transform t in enemyField.transform)
        {

                    t.Find("Indicator").GetComponent<Image>().enabled = false;
                    t.GetComponent<CardBehaviour>().effectSelectable = false;
        }
        enemyAvatar.GetComponent<PlayerAvatarBehaviour>().effectSelectable = false;
        gameManager.EnableZone(gameManager.playerManager.playerHandArea);
        gameManager.EnableZone(gameManager.playerManager.playerFieldArea);
        gameManager.EnableZone(gameManager.playerManager.playerUtilityArea);
        base.PlayEffect(target);
    }

}
