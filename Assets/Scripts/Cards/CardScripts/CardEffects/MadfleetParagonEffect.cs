using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "CardEffects/MadFleetParagonEffect")]
public class MadfleetParagonEffect : CardEffect
{
    public override void Contract()
    {
        base.Contract();
        foreach (Transform t in playerField.transform)
        {

            t.GetComponent<CardDisplay>().SetIndicator("Field", true);
            t.GetComponent<CardBehaviour>().contractSelectable = true;
        }
        gameManager.DisableZone(gameManager.playerManager.playerHandArea);
        gameManager.DisableZone(gameManager.playerManager.playerUtilityArea);
    }
    public override void ContractEffect()
    {
        foreach (GameObject obj in player.selectedUnits)
        {
            player.DestroyCard(obj);
        }
        player.UpdateSelectedUnits(self, false);
        foreach (Transform t in playerField.transform)
        {
            t.GetComponent<CardDisplay>().SetIndicator("Field", false);
            t.GetComponent<CardBehaviour>().contractSelectable = false;
        }
        player.currentContract.Remove(self);
        gameManager.EnableZone(gameManager.playerManager.playerHandArea);
        gameManager.EnableZone(gameManager.playerManager.playerUtilityArea);
        base.ContractEffect();
    }

    public override void Deathwalk()
    {
        player.CreateCard(self.GetComponent<CardDisplay>().card.creations[0]);
        base.Deathwalk();
    }
}
