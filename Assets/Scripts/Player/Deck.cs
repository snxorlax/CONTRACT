using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]
public class Deck : ScriptableObject
{
    public List<Card> mainDeck = new List<Card>();
    public List<Card> calamityDeck = new List<Card>();
}
