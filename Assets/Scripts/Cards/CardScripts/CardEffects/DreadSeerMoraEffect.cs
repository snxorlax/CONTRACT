using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CardEffects/DreadSeerMoraEffect")]
public class DreadSeerMoraEffect : CardEffect
{
    public override void Shroud()
    {
        player.CreateCard(self.GetComponent<CardDisplay>().card.creations[0]);
        base.Shroud();
    }
    public override void Play()
    {
        base.Play();
        player.currentEffect.Clear();
    }
}
