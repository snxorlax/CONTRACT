using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    //Scriptable Object card associated with this GameObject
    public Card card;
    //Card Behaviour associated with this GameObject. Used to sync card between components
    [Header("Scripts")]
    public CardBehaviour cardBehaviour;
    //AnimateCard script associated with this object
    public AnimateCard animateCard;
    [Header("Catalogues")]
    public CardArtCatalogue artCatalogue;
    public CardCatalogue cardCatalogue;
    [Header("Views")]
    public GameObject handViewFront;
    public GameObject handViewBack, fieldViewUnit, fieldViewRelic;
    //Used to sync card and cardArt without sending sprites and other complex datatypes over network
    [Header ("HandView Properties")]
    public Image handArt; 
    public Image handFrameOuter, handFrameMain, handFrameInner, handFrameShadow;
    public GameObject bounty;

    [Header ("HandView BattleStats")]
    public GameObject handStatBox;
    public Image handAttackFrame;
    public Image handAttackBackground_1, handAttackBackground_2, handAttackBackground_3;
    public Image handHealthFrame, handHealthBackground_1, handHealthBackground_2, handHealthBackground_3;
    public TextMeshProUGUI handAttack, handHealth;

    [Header ("CardText")]
    public TextMeshProUGUI cardName;
    public TextMeshProUGUI cardTypeText;
    public TextMeshProUGUI cardText;

    [Header ("FieldView Properties, Player, Unit")]
    public Image playerFieldArt; 
    public Image playerFieldFrameOuter, playerFieldFrameMain, playerFieldFrameInner, playerFieldFrameShadow;

    [Header ("FieldView BattleStats, Player, Unit")]
    public Image playerFieldAttackFrame;
    public Image playerFieldAttackBackground_1, playerFieldAttackBackground_2, playerFieldAttackBackground_3;
    public Image playerFieldHealthFrame, playerFieldHealthBackground_1, playerFieldHealthBackground_2, playerFieldHealthBackground_3;
    public TextMeshProUGUI playerFieldAttack, playerFieldHealth;

    [Header ("FieldView Properties, Enemy, Unit")]
    public Image enemyFieldArt; 
    public Image enemyFieldFrameOuter, enemyFieldFrameMain, enemyFieldFrameInner, enemyFieldFrameShadow;

    [Header ("FieldView BattleStats, Enemy, Unit")]
    public Image enemyFieldAttackFrame;
    public Image enemyFieldAttackBackground_1, enemyFieldAttackBackground_2, enemyFieldAttackBackground_3;
    public Image enemyFieldHealthFrame, enemyFieldHealthBackground_1, enemyFieldHealthBackground_2, enemyFieldHealthBackground_3;
    public TextMeshProUGUI enemyFieldAttack, enemyFieldHealth;

    // [Header("FieldView Properties, Relic")]

    [Header ("Lists for Assigning Colors")]
    public List<Image> handFrameImages;
    public List<Image> handAttackImages, handHealthImages;
    public List<Image> playerUnitImages, playerAttackImages, playerHealthImages;
    public List<Image> enemyUnitImages, enemyAttackImages, enemyHealthImages;

    //Preset color combinations for different card types
    [Header("CardType Color Presets")]
    public List<Color> henchmanColors;
    public List<Color> relicColors, vaColors, calamityColors, villainColors, attackColors, henchmanHealthColors, villainHealthColors;

    //GameObjects used to activate effects if applicable
    [Header("Card Effects")]
    public GameObject deathWalk, shroud;
    public List<GameObject> activatedEffects;
    
    void OnEnable()
    {
        SetCardProperties();
    }
    public void SetCardProperties()
    {
        if (card)
        {
            //Sync sprite via number in catalogue to texture of instantiated material
            handArt.material.SetTexture("MainTexture", artCatalogue.cardArt[card.cardNo].texture);
            switch (card.cardType)
            {
                case Card.CardType.Henchman:
                    for (int i = 0; i < handFrameImages.Count; i++)
                    {
                        handFrameImages[i].color = henchmanColors[i];
                    }
                    for (int i = 0; i < handAttackImages.Count; i++)
                    {
                        handAttackImages[i].color = attackColors[i];
                        handHealthImages[i].color = henchmanHealthColors[i];
                    }
                    handStatBox.SetActive(true);
                    SetBounty(card.bounty);
                    break;
                case Card.CardType.Relic:
                    for (int i = 0; i < handFrameImages.Count; i++)
                    {
                        handFrameImages[i].color = relicColors[i];
                    }
                    handStatBox.gameObject.SetActive(false);
                    break;
                case Card.CardType.VillainousArt:    
                    for (int i = 0; i < handFrameImages.Count; i++)
                    {
                        handFrameImages[i].color = vaColors[i];
                    }
                    handStatBox.gameObject.SetActive(false);
                    break;
                case Card.CardType.Villain:
                    for (int i = 0; i < handFrameImages.Count; i++)
                    {
                        handFrameImages[i].color = villainColors[i];
                    }
                    for (int i = 0; i < handAttackImages.Count; i++)
                    {
                        handAttackImages[i].color = attackColors[i];
                        handHealthImages[i].color = villainHealthColors[i];
                    }
                    SetBounty(card.bounty);
                    break;
                case Card.CardType.Calamity:
                    for (int i = 0; i < handFrameImages.Count; i++)
                    {
                        handFrameImages[i].color = calamityColors[i];
                    }
                    SetBounty(card.bounty);
                    break;
            }
            // battleStats = statBox.Find("Text").GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
            // battleStatsField = statBoxField.Find("Text").GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
            // battleStats.text = card.attack.ToString() + "  /  " + card.health.ToString();
            // battleStatsField.text = card.attack.ToString() + "  /  " + card.health.ToString();

            cardText.text = card.cardText;
            cardName.text = card.cardName;
            cardTypeText.text = card.cardType.ToString();

            cardBehaviour.SetCard();
            SetCardEffects();

            if (animateCard)
            {
                animateCard.card = card;
            }
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
    public void SetCardEffects()
    {
        if (transform.Find("CardEffects"))
        {
            for (int i = 0; i < card.activatedEffectText.Count; i++)
            {
                if (!activatedEffects.Contains(transform.Find("CardEffects").GetChild(i + 1).gameObject))
                {
                    activatedEffects.Add(transform.Find("CardEffects").GetChild(i + 1).gameObject);
                }
                activatedEffects[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = card.activatedEffectText[i];
                activatedEffects[i].GetComponent<CardEffectGUIBehaviour>().effectNumber = i;
            }
            if (card.deathWalk)
            {
                deathWalk.GetComponent<CardEffectGUIBehaviour>().effectNumber = 3;
                deathWalk.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = card.deathWalkText;
            }
            shroud.GetComponent<CardEffectGUIBehaviour>().effectNumber = 4;
        }
    }

}
