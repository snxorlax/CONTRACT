using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Display : MonoBehaviour
{
    public static float handOffset, enemyHandOffset, fieldOffset, discardOffset, centerVert;

    private void Awake()
    {
        handOffset = 5f;
        enemyHandOffset = -55f;
        fieldOffset = 60f;
        discardOffset = 15f;
        centerVert = -1275f;
    }
    
    public void DisplayHorizontal(List<GameObject> hand, float offset)
    {
        List<Vector2> positions = new List<Vector2>();
        if (hand.Count > 0)
        {
            positions = GeneratePositionsHorizontal(hand.Count, hand[0], offset);
        }

        for (int i = 0; i < hand.Count; i++)
        {
            hand[i].transform.localPosition = positions[i];
        }
    }    
    public void DisplayVertical(List<GameObject> hand, float offset)
    {
        List<Vector2> positions = new List<Vector2>();
        if (hand.Count > 0)
        {
            positions = GeneratePositionsVertical(hand.Count, hand[0], offset);
        }

        for (int i = 0; i < hand.Count; i++)
        {
            hand[i].transform.localPosition = positions[i];
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
