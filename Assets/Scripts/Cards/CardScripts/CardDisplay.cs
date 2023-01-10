using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class CardDisplay : NetworkBehaviour
{
    //Scriptable Object card associated with this GameObject
    public Card card;
    public Material cardMat, artMat;
    public GameObject fieldDestroy;
    //List of lists to be updated, based on what current card contains. Can be useful if differentiating a card from an animator
    public List<List<Image>> frameLists = new List<List<Image>>();
    public List<List<Image>> attackImageLists = new List<List<Image>>(); 
    public List<List<Image>> healthImageLists = new List<List<Image>>();
    public List<List<GameObject>> repositionLists = new List<List<GameObject>>();
    //List of art in different views
    public List<Image> artList;
    //List of battle text in different views
    public List<TextMeshProUGUI> attackTextList, healthTextList;
    public Image handIndicator, fieldIndicator;
    [Header("Scripts")]
    //Card Behaviour associated with this GameObject. Used to sync card between components
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
    public TextMeshProUGUI handAttackText, handHealthText;

    [Header ("CardText")]
    public TextMeshProUGUI cardName;
    public TextMeshProUGUI cardTypeText;
    public TextMeshProUGUI cardText;

    [Header ("FieldView Properties, Unit")]
    public Image unitFieldArt; 
    public Image unitFieldFrameOuter, unitFieldFrameMain, unitFieldFrameInner, unitFieldFrameShadow;

    [Header ("FieldView BattleStats, Unit")]
    public GameObject fieldStatBox;
    public Image FieldAttackFrame;
    public Image FieldAttackBackground_1, FieldAttackBackground_2, FieldAttackBackground_3;
    public Image FieldHealthFrame, FieldHealthBackground_1, FieldHealthBackground_2, FieldHealthBackground_3;
    public TextMeshProUGUI FieldAttackText, FieldHealthText;

    [Header("FieldView Properties, Relic")]
    public Image relicFieldArt;
    [Header("FieldDestroy, Unit, TL")]
    public Image fieldDestroyUnitArtTL;
    public Image fieldDestroyFrameOuterTL, fieldDestroyFrameMainTL, fieldDestroyFrameInnerTL, fieldDestroyFrameShadowTL;
    public TextMeshProUGUI destroyAttackTextTL, destroyHealthTextTL;
    [Header("FieldDestroy, Unit, BR")]
    public Image fieldDestroyUnitArtBR;
    public Image fieldDestroyFrameOuterBR, fieldDestroyFrameMainBR, fieldDestroyFrameInnerBR, fieldDestroyFrameShadowBR;
    public TextMeshProUGUI destroyAttackTextBR, destroyHealthTextBR;
    [Header("FieldDestroy, Relic, TL")]
    public Image fieldDestroyRelicArtTL;
    [Header("FieldDestroy, Relic, BR")]
    public Image fieldDestroyRelicArtBR;

    [Header ("Lists for Assigning Colors")]
    public List<Image> handFrameImages;
    public List<Image> unitFieldImages;
    public List<Image> handAttackImages, fieldAttackImages;
    public List<Image>  handHealthImages, fieldHealthImages;
    public List<Image> unitDestroyImagesTL, unitDestroyImagesBR, attackDestroyImagesTL, attackDestroyImagesBR, healthDestroyImagesTL, healthDestroyImagesBR;

    [Header ("Reposition Lists")]
    public List<GameObject> fieldReposition;
    public List<GameObject> destroyReposition;


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
        //Populate appropriate lists for assigning colors
        //check if handview exists
        if (handViewFront)
        {
            //Add handframes to be colored
            frameLists.Add(handFrameImages);
            //Add handAttack to be colored
            attackImageLists.Add(handAttackImages);
            //Add handHealth to be colored
            healthImageLists.Add(handHealthImages);
            //Add handArt to be assigned proper sprite
            artList.Add(handArt);
            //Add attacktext and healthtext to textlist
            attackTextList.Add(handAttackText);
            healthTextList.Add(handHealthText);
        }
        //check if fieldview exists
        if (fieldViewUnit)
        {
            //add player and enemy frames to be colored
            frameLists.Add(unitFieldImages);
            //add unitattack and enemy attack to be colored
            attackImageLists.Add(fieldAttackImages);
            //add unit health to be colored
            healthImageLists.Add(fieldHealthImages);
            //add player and enemy art to be assigned proper sprite
            artList.Add(unitFieldArt);
            artList.Add(relicFieldArt);
            //add player and enemy text to be updated
            attackTextList.Add(FieldAttackText);
            healthTextList.Add(FieldHealthText);
            //add relevant gameobjects to repositionlist to be positioned at runtime based on authority
            repositionLists.Add(fieldReposition);
        }
        if (fieldDestroy)
        {
            //add player and enemy frames to be colored
            frameLists.Add(unitDestroyImagesTL);
            frameLists.Add(unitDestroyImagesBR);
            //add TL and BR attack images
            attackImageLists.Add(attackDestroyImagesTL);
            attackImageLists.Add(attackDestroyImagesBR);
            //add TL and BR health images
            healthImageLists.Add(healthDestroyImagesTL);
            healthImageLists.Add(healthDestroyImagesBR);
            //add player and enemy art to be assigned proper sprite
            artList.Add(fieldDestroyRelicArtTL);
            artList.Add(fieldDestroyRelicArtBR);
            artList.Add(fieldDestroyUnitArtTL);
            artList.Add(fieldDestroyUnitArtBR);
            //add player and enemy text to be updated
            attackTextList.Add(destroyAttackTextTL);
            attackTextList.Add(destroyAttackTextBR);
            healthTextList.Add(destroyHealthTextTL);
            healthTextList.Add(destroyHealthTextBR);
            //add relevant gameobjects to repositionlist to be positioned at runtime based on authority
            repositionLists.Add(destroyReposition);
        }

    }
    //Main function to set art, colors, stats, text and effects for each applicable view of card
    public void SetCardProperties()
    {
        if (card)
        {
            SetArt();
            SetColors();
            SetStats();
            SetPlayerandEnemyPositions();
            SetText();
            SetCardEffects();
            cardBehaviour.SetCard();

            if (animateCard)
            {
                animateCard.card = card;
            }
        }
    }
    public void SetArt()
    {
        foreach (Image a in artList)
        {
            //Set material to instantiated material to allow for different main textures for different cards
            a.material = new Material(a.material);
            //Sync sprite via number in catalogue to texture of instantiated material
            a.material.SetTexture("MainTexture", artCatalogue.cardArt[card.cardNo].texture);
        }
    }
    //Applies the proper colors to the frame and statboxes of the views of card
    public void SetColors()
    {
        switch (card.cardType)
        {
            case Card.CardType.Henchman:
                for (int i = 0; i < henchmanColors.Count; i++)
                {
                    //Apply henchman colors to frames of all views
                    foreach (List<Image> f in frameLists)
                    {
                        f[i].material = new Material(f[i].material);
                        f[i].material.SetColor("MainColor", henchmanColors[i]);
                        f[i].color = henchmanColors[i];
                    }
                }
                for (int i = 0; i < attackColors.Count; i++)
                {
                    //Apply attack/health colors to all views
                    foreach (List<Image> a in attackImageLists)
                    {
                        a[i].material = new Material(a[i].material);
                        a[i].material.SetColor("MainColor", attackColors[i]);
                        a[i].color = attackColors[i];
                    }
                    foreach (List<Image> h in healthImageLists)
                    {
                        h[i].material = new Material(h[i].material);
                        h[i].material.SetColor("MainColor", henchmanHealthColors[i]);
                        h[i].color = henchmanHealthColors[i];
                    }
                }
                //set statbox to active if henchman
                handStatBox.SetActive(true);
                //sets the appropropriate bounty
                SetBounty(card.bounty);
                break;
            case Card.CardType.Relic:
                for (int i = 0; i < relicColors.Count; i++)
                {
                    handFrameImages[i].color = relicColors[i];
                }
                //set statbox to inactive if relic
                handStatBox.gameObject.SetActive(false);
                break;
            case Card.CardType.VillainousArt:
                for (int i = 0; i < vaColors.Count; i++)
                {
                    handFrameImages[i].color = vaColors[i];
                }
                handStatBox.gameObject.SetActive(false);
                break;
            case Card.CardType.Villain:
                //Apply villain colors to frames of all views
                for (int i = 0; i < villainColors.Count; i++)
                {
                    foreach (List<Image> f in frameLists)
                    {
                        f[i].material = new Material(f[i].material);
                        f[i].material.SetColor("MainColor", villainColors[i]);
                        f[i].color = villainColors[i];
                    }
                }
                //Apply attack/health colors to all views
                for (int i = 0; i < attackColors.Count; i++)
                {
                    foreach (List<Image> a in attackImageLists)
                    {
                        a[i].material = new Material(a[i].material);
                        a[i].material.SetColor("MainColor", attackColors[i]);
                        a[i].color = attackColors[i];
                    }
                    foreach (List<Image> h in healthImageLists)
                    {
                        h[i].material = new Material(h[i].material);
                        h[i].material.SetColor("MainColor", villainHealthColors[i]);
                        h[i].color = villainHealthColors[i];
                    }
                }
                //set statbox to active if villain
                handStatBox.SetActive(true);
                //sets appropriate bounty
                SetBounty(card.bounty);
                break;

            //Calamity will be more deeply implemented later
            case Card.CardType.Calamity:
                for (int i = 0; i < calamityColors.Count; i++)
                {
                    handFrameImages[i].color = calamityColors[i];
                }
                SetBounty(card.bounty);
                break;
        }
    }

    //Sets stats of all views to match card stat values
    public void SetStats()
    {
        foreach (TextMeshProUGUI attackText in attackTextList)
        {
            attackText.text = card.attack.ToString();
        }
        foreach (TextMeshProUGUI healthText in healthTextList)
        {
            healthText.text = card.health.ToString();
        }
    }
    public void SetPlayerandEnemyPositions()
    {
        if (!hasAuthority)
        {
            foreach (List<GameObject> l in repositionLists)
            {
                for (int i = 0; i < l.Count; i++)
                {
                    l[i].transform.localPosition = new Vector3(l[i].transform.localPosition.x, -l[i].transform.localPosition.y, l[i].transform.localPosition.z);
                }
                
            }
        }
    }

    //Sets text on card. Only visible in handView
    public void SetText()
    {
        cardText.text = card.cardText;
        cardName.text = card.cardName;
        cardTypeText.text = card.cardType.ToString();
    }

    //Assign the cardEffectObjects to their proper effects
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
    public void SetIndicator(string view, bool val)
    {
        switch (view)
        {
            case "Hand":
            handIndicator.enabled = val;
            break;
            case "Field":
            fieldIndicator.enabled = val;
            break;
        }
    }
}
