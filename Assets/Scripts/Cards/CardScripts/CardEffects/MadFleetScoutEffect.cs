using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CardEffects/MadFleetScoutEffect")]
public class MadFleetScoutEffect : CardEffect
{
    public bool empowered;
    public override void Continuous()
    {
        if (player.field.Count > 4 && !empowered)
        {

            gameManager.ChangeStats(self, 2, 1);
            self.GetComponent<CardDisplay>().card.originalAttack += 2;
            self.GetComponent<CardDisplay>().card.originalHealth += 1;
            self.GetComponent<CardBehaviour>().SetCard();
            empowered = true;
        }
        if (player.field.Count < 5 && empowered)
        {
            gameManager.ChangeStats(self, -2, -1);
            self.GetComponent<CardDisplay>().card.originalAttack -= 2;
            self.GetComponent<CardDisplay>().card.originalHealth -= 1;
            self.GetComponent<CardBehaviour>().SetCard();
            empowered = false;
        }
    }
    public override void Play()
    {
        base.Play();
        gameManager.UnitEvent.AddListener(Continuous);
        player.currentEffect.Clear();
    }
    public override void ActivatedEffect_1()
    {
        base.ActivatedEffect_1();
    }
    public override void Deathwalk()
    {
        player.DrawCard(1);
        base.Deathwalk();
    }
}
