using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class ArrowScript : MonoBehaviour
{
    public GameObject arrowHead;
    public GameObject bodySegment;
    public List<GameObject> activeSegments, hiddenSegments;
    public Transform curveCenter;
    public float curveAngle;
    public float curveTilt;
    public float radius;
    public Canvas canvas;
    private void OnEnable()
    {
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
    }
    public void DrawArrow(Vector3 startPos)
    {
        transform.position = startPos;
        Vector2 localPos = GetComponent<RectTransform>().position;
        // Debug.Log("SS mousePosition =" + Input.mousePosition);
        Vector2 arrowVector = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), Input.mousePosition, Camera.main, out localPos);
        // Debug.Log("LS mousePosition =" + localPos);
        radius = localPos.magnitude;
        Debug.Log(radius);
        ShowArrow();
        DisplaySegments(startPos);
        PositionSegments(DisplaySegments(startPos), startPos, arrowVector);
        PositionArrowHead();
        RotateArrowHead(startPos, DisplaySegments(startPos));
        SortSegments();
        RotateSegments(startPos);
    }
    public void PositionArrowHead()
    {
        arrowHead.transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
    public void RotateArrowHead(Vector3 startPos, int numSegments)
    {
        Vector2 diff = Camera.main.ScreenToWorldPoint(Input.mousePosition) - startPos;
        diff.Normalize();

        float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        arrowHead.transform.rotation = Quaternion.Euler(0, 0f, rot_z - 90);
    }
    public int DisplaySegments(Vector3 startPos)
    {
        Vector2 diff = Camera.main.ScreenToWorldPoint(Input.mousePosition) - startPos;
        // Debug.Log(diff);
        int numSegments = (int)diff.magnitude * 2;
        numSegments = Mathf.Clamp(numSegments, 0, 13);
        if (activeSegments.Count < numSegments)
        {
            if (hiddenSegments.Count > 0)
            {
                activeSegments.Insert(0, hiddenSegments[0]);
                hiddenSegments[0].SetActive(true);
                hiddenSegments.RemoveAt(0);
            }
        }
        else if (activeSegments.Count > numSegments)
        {
            hiddenSegments.Insert(0, activeSegments[0]);
            activeSegments[0].SetActive(false);
            activeSegments.RemoveAt(0);
        }
        return numSegments;
    }
    public void PositionSegments(int numSegments, Vector3 startPos, Vector2 arrowVector)
    {
        if (numSegments > 0)
        {
            Vector3 recPos;
            curveAngle = 1.1f / numSegments;
            Vector3 direction = arrowVector.normalized;
            Vector3 axis = direction * radius / numSegments;
            for (int i = 0; i < activeSegments.Count; i++)
            {
                //Resets rotation
                activeSegments[i].transform.rotation = Quaternion.Euler(90, 0, 0);
                recPos = new Vector3(0, Mathf.Cos(curveAngle * i - Mathf.PI / 2) * radius, Mathf.Sin(curveAngle * i - Mathf.PI/2) * radius);
                // activeSegments[i].transform.position = transform.position + recPos;
                activeSegments[i].transform.localPosition = recPos;

            }
        }
    }
    public void SortSegments()
    {
        activeSegments = activeSegments.OrderBy(segment => segment.transform.position.z).ToList();
        foreach (GameObject segment in activeSegments)
        {
            segment.transform.SetAsLastSibling();
        }
    }
    public void RotateSegments(Vector3 startPos)
    {
        Vector3 diff = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - (Vector2)transform.position;
        float arrowAngle = Mathf.Atan2(diff.y, diff.x);
        activeSegments = activeSegments.OrderBy(segment => segment.transform.position.y).ToList();
        for (int i = 0; i < activeSegments.Count; i++)
        {

            Quaternion relativeRot = Quaternion.identity;
            if (i < activeSegments.Count -1)
            {

                Vector3 relativePos = activeSegments[i + 1].transform.localPosition - activeSegments[i].transform.localPosition;
                relativeRot = Quaternion.LookRotation(relativePos, activeSegments[i].transform.up);
            }
            if ( i == activeSegments.Count -1)
            {
                Vector3 relativePos = transform.position + new Vector3(0, Mathf.Cos(curveAngle * (i + 1)) * radius, Mathf.Sin(curveAngle * (i + 1)) * radius) - activeSegments[i].transform.position;
                relativeRot = Quaternion.LookRotation(relativePos, activeSegments[i].transform.up);
            }
            activeSegments[i].transform.localRotation = Quaternion.RotateTowards(activeSegments[i].transform.rotation, relativeRot, 360);
            activeSegments[i].transform.localRotation = Quaternion.Euler(activeSegments[i].transform.localEulerAngles.x, 0, 0);



        }
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * arrowAngle - 90);

    }

    public void ShowArrow()
    {
       foreach (Transform t in transform)
        {
            t.gameObject.SetActive(true);
       }
    }
    public void HideArrow()
    {
        foreach (Transform t in transform)
        {
            t.gameObject.SetActive(false);
       }
    }
    
}
