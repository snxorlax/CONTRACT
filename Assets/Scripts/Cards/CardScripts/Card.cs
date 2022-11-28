using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
[CreateAssetMenu]
public class Card : ScriptableObject
{
    public int cardNo;
    public enum CardType 
    {
        Henchman, Villain, Relic, VillainousArt, Lair, Calamity
    }
    public CardType cardType;
    public Color artColor;
    public int attack, originalAttack, health, originalHealth, bounty;
    public string currentZone;
    public bool shroud;

    public string cardName;
    public string cardText;

    public CardEffect cardEffect;
    public List<string> activatedEffectText;
    public string deathWalkText;
    public bool deathWalk;
    public List<Card> creations;

}
