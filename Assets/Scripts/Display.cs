using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Display : MonoBehaviour
{
    public static float handOffset, enemyHandOffset, fieldOffset, discardOffset, centerVert, effectOffset;

    private void Awake()
    {
        handOffset = -15f;
        enemyHandOffset = -55f;
        fieldOffset = 60f;
        discardOffset = 15f;
        centerVert = -1150f;
        effectOffset = 110;
    }
    
    public void DisplayHorizontal(List<GameObject> card, float offset)
    {
        List<Vector2> positions = new List<Vector2>();
        if (card.Count > 0)
        {
            positions = GeneratePositionsHorizontal(card.Count, card[0], offset);
        }

        for (int i = 0; i < card.Count; i++)
        {
            card[i].transform.localPosition = positions[i];
        }
    }    
    public void DisplayHorizontalWorldSpace(List<GameObject> card, float offset)
    {
        List<Vector2> positions = new List<Vector2>();
        if (card.Count > 0)
        {
            positions = GeneratePositionsHorizontal(card.Count, card[0], offset);
        }

        for (int i = 0; i < card.Count; i++)
        {
            card[i].transform.position = positions[i];
        }
    }    
    public void DisplayVertical(List<GameObject> card, float offset)
    {
        List<Vector2> positions = new List<Vector2>();
        if (card.Count > 0)
        {
            positions = GeneratePositionsVertical(card.Count, card[0], offset);
        }

        for (int i = 0; i < card.Count; i++)
        {
            card[i].transform.localPosition = positions[i];
        }
    }    

    public List<Vector2> GeneratePositionsHorizontal(int numCards, GameObject card, float offset)
    {
        List<Vector2> positions = new List<Vector2>();
        Vector2 pos;
        float width = card.GetComponent<RectTransform>().rect.width;
        float totalWidth = (numCards * width) + (offset * (numCards - 1));
        float startX = -1 * (totalWidth / 2) + width/2;

        for (int i = 0; i < numCards; i++)
        {
            pos = new Vector2(startX + offset * i + width * i, 0.0f);
            positions.Add(pos);
        }

        return positions;
    }

    //Rotates cards in player hand, adjust heights, and sets correct sibling indexes. with the focused card have no rotation
    public void FanHand(List<GameObject> hand, GameObject focusedCard)
    {
        RotateCardsinHand(hand, focusedCard);
        PositionCardsinFan(hand, focusedCard);
        SetCardOrder(hand, focusedCard);
    }
    //Rotates cards in player hand when card is focused, with focused card having no rotation
    public void RotateCardsinHand(List<GameObject> hand, GameObject focusedCard)
    {
        float rotZ = 2f;
        if (hand.Contains(focusedCard))
        {
            foreach (GameObject card in hand)
            {
                if (card != focusedCard)
                {
                    //cards above and below focused card will share a rotation with each other, other than end cards
                    if (hand.IndexOf(focusedCard) - hand.IndexOf(card) < 0)
                    {
                        card.transform.localRotation = Quaternion.Euler(0, 0, (-rotZ));
                    }
                    else if ((hand.IndexOf(focusedCard) - hand.IndexOf(card)) > 0)
                    {
                        card.transform.localRotation = Quaternion.Euler(0, 0, (rotZ));
                    }
                    if (hand.IndexOf(card) == 0)
                    {
                        card.transform.localRotation = Quaternion.Euler(0, 0, (4));
                    }
                    else if (hand.IndexOf(card) == hand.Count - 1)
                    {
                        card.transform.localRotation = Quaternion.Euler(0, 0, (-4));
                    }
                }
            }
        }
    }
    //Set the positions of cards in fan when a card in hand is focused
    public void PositionCardsinFan(List<GameObject> hand, GameObject focusedCard)
    {
        if (hand.Contains(focusedCard))
        {
            foreach (GameObject card in hand)
            {
                //following logic doesn't apply to the focused card
                if (card != focusedCard)
                {
                    //changes y value of local position according to difference in index from focused card
                    float posY = 10 / Mathf.Abs((hand.IndexOf(focusedCard) - hand.IndexOf(card)));
                    // disallows the position of non-end cards to be above the ends
                    posY = Mathf.Clamp(posY, 0, 10f);
                    // the further away from the focused card, the less distance between cards there is
                    float posX = (hand.IndexOf(focusedCard) - hand.IndexOf(card)) * 3f;
                    //set localpos of each card
                    card.transform.localPosition = new Vector2(card.transform.localPosition.x + posX, posY);
                    //first and last cards of hand will always have the same position, adds consistency to the various possible hand configs
                    if (hand.IndexOf(card) == 0 || hand.IndexOf(card) == hand.Count - 1)
                    {
                        //same x values as other cards, but always the same y val
                        card.transform.localPosition = new Vector2(card.transform.localPosition.x + posX, -.5f);
                    }
                }
            }
        }
        
    }
    public void SetCardOrder(List<GameObject>hand, GameObject focusedCard)
    {
        int handCount = hand.Count;
        foreach (GameObject card in hand)
        {
            card.transform.SetSiblingIndex(handCount - 1 - Mathf.Abs(hand.IndexOf(focusedCard) - hand.IndexOf(card)));
        }
    }
    //Reset rotations of all cards. Specifically when the hand is no longer focused
    public void ResetRotations(List<GameObject> cards)
    {
        foreach (GameObject g in cards)
        {
            g.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }
    }
    public List<Vector2> GeneratePositionsVertical(int numCards, GameObject card, float offset)
    {
        List<Vector2> positions = new List<Vector2>();
        Vector2 pos;
        float height = card.GetComponent<RectTransform>().rect.height * card.transform.localScale.y;
        float totalHeight = (numCards * height) + (offset * (numCards - 1));
        float startX = -1 * (totalHeight / 2) + height/2;

        for (int i = 0; i < numCards; i++)
        {
            pos = new Vector2(centerVert, startX + offset * i + height * i);
            positions.Add(pos);
        }

        return positions;
    }
}
