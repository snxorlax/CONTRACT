using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    public Card card;
    public Transform frame, statBox, statBoxPlay, text;
    public Image art, frameOuter, frameMain, frameInner, frameShadow, statFrame, statFramePlay;
    public GameObject bounty;
    public TextMeshProUGUI cardText, battleStats, battleStatsPlay, cardName, cardTypeText;
    public List<Color> henchmanColors, relicColors, vaColors, calamityColors, villainColors;
    public List<GameObject> activatedEffects;
    public GameObject deathWalk, shroud;
    public CardArtCatalogue artCatalogue;
    public CardCatalogue cardCatalogue;
    public CardBehaviour cardBehaviour;
    void OnEnable()
    {
        SetCardProperties();
    }
    public void SetCardProperties()
    {
        if (card)
        {
            // Display Art + Determine Art Color
            art = transform.Find("Front").Find("Art").gameObject.GetComponent<Image>();

            art.sprite = artCatalogue.cardArt[card.cardNo];
            art.color = card.artColor;

            //Assign frame parts to proper components
            frame = transform.Find("Front").Find("Frame");

            frameOuter = frame.Find("Outer").gameObject.GetComponent<Image>();
            frameMain = frame.Find("Main").gameObject.GetComponent<Image>();
            frameInner = frame.Find("Inner").gameObject.GetComponent<Image>();
            frameShadow = frame.Find("Shadow").gameObject.GetComponent<Image>();
            bounty = frame.Find("Bounty").gameObject;

            //Set statbox to proper object
            statBox = transform.Find("Front").Find("StatBox");
            // statBoxPlay = transform.Find("Front").Find("StatBoxField");
            statFrame = statBox.Find("Background").Find("Frame").GetComponent<Image>();
            // statFramePlay = statBoxPlay.Find("Background").Find("Frame").GetComponent<Image>();

            if (transform.Find("CardEffects"))
            {
                for (int i = 0; i < card.activatedEffectText.Count; i++)
                {
                    if (!activatedEffects.Contains(transform.Find("CardEffects").GetChild(i+1).gameObject))
                    {
                        activatedEffects.Add(transform.Find("CardEffects").GetChild(i+1).gameObject);
                    }
                    activatedEffects[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = card.activatedEffectText[i];
                    activatedEffects[i].GetComponent<CardEffectGUIBehaviour>().effectNumber = i;
                }
                if (card.deathWalk)
                {
                    deathWalk = transform.Find("CardEffects").GetChild(4).gameObject;
                    deathWalk.GetComponent<CardEffectGUIBehaviour>().effectNumber = 3;
                    deathWalk.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = card.deathWalkText;
                }
                shroud = transform.Find("CardEffects").GetChild(5).gameObject;
                shroud.GetComponent<CardEffectGUIBehaviour>().effectNumber = 4;
            }

            switch (card.cardType)
            {
                case Card.CardType.Henchman:
                    frameOuter.color = henchmanColors[0];
                    statFrame.color = henchmanColors[0];
                    // statFramePlay.color = henchmanColors[0];
                    frameMain.color = henchmanColors[1];
                    frameInner.color = henchmanColors[2];
                    frameShadow.color = henchmanColors[3];
                    SetBounty(card.bounty);
                    break;
                case Card.CardType.Relic:
                    frameOuter.color = relicColors[0];
                    frameMain.color = relicColors[1];
                    frameInner.color = relicColors[2];
                    frameShadow.color = relicColors[3];
                    break;
                case Card.CardType.VillainousArt:    
                    frameOuter.color = vaColors[0];
                    frameMain.color = vaColors[1];
                    frameInner.color = vaColors[2];
                    frameShadow.color = vaColors[3];
                    break;
                case Card.CardType.Villain:
                    frameOuter.color = villainColors[0];
                    statFrame.color = villainColors[0];
                    // statFramePlay.color = villainColors[0];
                    frameMain.color = villainColors[1];
                    frameInner.color = villainColors[2];
                    frameShadow.color = villainColors[3];
                    SetBounty(card.bounty);
                    break;
                case Card.CardType.Calamity:
                    frameOuter.color = calamityColors[0];
                    frameMain.color = calamityColors[1];
                    frameInner.color = calamityColors[2];
                    frameShadow.color = calamityColors[3];
                    SetBounty(card.bounty);
                    break;
                

            }

            if (card.cardType != Card.CardType.Henchman && card.cardType!= Card.CardType.Villain && card.cardType != Card.CardType.Calamity)
            {
               statBox.gameObject.SetActive(false);
            //    statBoxPlay.gameObject.SetActive(false);
            }
            // if (card.currentZone != "Field")
            // {
            //    statBoxPlay.gameObject.SetActive(false);
            // }
            battleStats = statBox.Find("Text").GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
            // battleStatsPlay = statBoxPlay.Find("Text").GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
            battleStats.text = card.attack.ToString() + " / " + card.health.ToString();
            battleStats.text = card.attack.ToString() + " / " + card.health.ToString();

            text = transform.Find("Front").Find("Text");
            cardText = text.Find("CardText").gameObject.GetComponent<TextMeshProUGUI>();
            cardName = text.Find("NameText").gameObject.GetComponent<TextMeshProUGUI>();
            cardTypeText = text.Find("CardTypeText").gameObject.GetComponent<TextMeshProUGUI>();

            cardText.text = card.cardText;
            cardName.text = card.cardName;
            cardTypeText.text = card.cardType.ToString();

            cardBehaviour = GetComponent<CardBehaviour>();
            cardBehaviour.SetCard();
        }
    }
    public void SetBounty(int bountyNo)
    {
        foreach (Transform t in bounty.transform)
        {
            t.gameObject.SetActive(false);
        }
        for (int i = 0; i < bountyNo; i++)
        {
            bounty.transform.GetChild(i).gameObject.SetActive(true);
        }
    }

}
