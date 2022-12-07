using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "CardEffects/GoonEnforcerEffect")]

public class GoonEnforcerEffect : CardEffect
{
    bool accomplice;
    bool empowered;
    List<Card.CardType> currentTypes = new List<Card.CardType>();
    public override void Continuous()
    {
        base.Continuous();
        currentTypes.Clear();
        foreach (GameObject g in player.playerField)
        {
            currentTypes.Add(g.GetComponent<CardDisplay>().card.cardType);
            Debug.Log(g.GetComponent<CardDisplay>().card.cardType);
        }
        if (currentTypes.Contains(Card.CardType.Villain))
        {
            accomplice = true;
        }
        else{
            accomplice = false;
        }
        if (accomplice && !empowered)
        {
            gameManager.ChangeStats(self, 2, 1);
            self.GetComponent<CardDisplay>().card.originalHealth += 1;
            self.GetComponent<CardDisplay>().SetCardProperties();
            empowered = true;
        }
        else if (!accomplice && empowered)
        {
            gameManager.ChangeStats(self, -2, -1);
            self.GetComponent<CardDisplay>().card.originalHealth -= 1;
            self.GetComponent<CardDisplay>().SetCardProperties();
            empowered = false;
        }
        
    }
    public override void TurnStart()
    {
        base.TurnStart();
        if (self.GetComponent<CardDisplay>().card.health < self.GetComponent<CardDisplay>().card.originalHealth)
        {
            gameManager.Damage(self, -1);
        }
        player.currentEffect.Remove(self);
    }
    public override void Play()
    {
        base.Play();
        gameManager.UnitEvent.AddListener(Continuous);
        player.currentEffect.Clear();
    }
}
