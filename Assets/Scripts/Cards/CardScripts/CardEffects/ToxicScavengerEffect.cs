using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[CreateAssetMenu(menuName = "CardEffects/ToxicScavenger")]
public class ToxicScavengerEffect : CardEffect
{
    public override void Play(){
        base.Play();
        player.currentEffect.Clear();
    }
    public override void Shroud(){
        currentEffect = effect.Shroud;
        foreach (Transform t in enemyField.transform)
        {

            t.Find("Indicator").GetComponent<Image>().enabled = true;
            t.GetComponent<CardBehaviour>().effectSelectable = true;
        }
    }
    public override void Deathwalk()
    {
        self.GetComponent<CardBehaviour>().playerDiscard.transform.SetAsFirstSibling();
        player.currentEffect.Add(self);
        currentEffect = effect.Deathwalk;
        gameMessage = "Poison an enemy unit.";
        player.gameText.GetComponent<TextMeshProUGUI>().enabled = true;
        player.gameText.GetComponent<TextMeshProUGUI>().text = gameMessage;
        foreach (Transform t in enemyField.transform)
        {
            t.Find("Indicator").GetComponent<Image>().enabled = true;
            t.GetComponent<CardBehaviour>().effectSelectable = true;
        }
        gameManager.DisableZone(gameManager.playerManager.playerHandArea);
        gameManager.DisableZone(gameManager.playerManager.playerFieldArea);
        gameManager.DisableZone(gameManager.playerManager.playerUtilityArea);
    }
    public override void DeathwalkEffect(GameObject target)
    {
        foreach (Transform t in enemyField.transform)
        {
            t.Find("Indicator").GetComponent<Image>().enabled = false;
            t.GetComponent<CardBehaviour>().effectSelectable = false;
        }
        player.gameText.GetComponent<TextMeshProUGUI>().enabled = false;
        gameManager.ChangeStats(target, -2, 0);
        player.ExileCard(self);
        player.currentEffect.Clear();
        gameManager.EnableZone(gameManager.playerManager.playerHandArea);
        gameManager.EnableZone(gameManager.playerManager.playerFieldArea);
        gameManager.EnableZone(gameManager.playerManager.playerUtilityArea);

    }
}
