using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Display : MonoBehaviour
{
    public static float handOffset, enemyHandOffset, fieldOffset, discardOffset, centerVert, effectOffset;

    private void Awake()
    {
        handOffset = 5f;
        enemyHandOffset = -55f;
        fieldOffset = 60f;
        discardOffset = 15f;
        centerVert = -1275f;
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
