using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class ArrowScript : MonoBehaviour
{
    public GameObject arrowHead;
    public GameObject bodySegment;
    public List<GameObject> allSegments, animatingSegments;
    public Transform segments;
    public int numSegments;
    public float curveAngle;
    public float curveTilt;
    public float radius;
    private void Awake()
    {
        for (int i = 0; i < numSegments; i++)
        {
            GameObject newSegment = Instantiate(bodySegment, transform.position, Quaternion.identity, segments);
            allSegments.Add(newSegment);

        }
        radius = 700f;
        InitPositions(numSegments);
        InitRotations();
    }
    public void DrawArrow(Vector3 startPos)
    {
        transform.position = startPos;
        Vector2 localPos = GetComponent<RectTransform>().position;
        Vector2 arrowVector = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), Input.mousePosition, Camera.main, out localPos);
        ShowArrow();
        PositionArrowHead();
        RotateArrowHead(startPos);
        RotateArrow();
        AnimateArrow();
        SortSegments();
    }
    public void PositionArrowHead()
    {
        arrowHead.transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
    public void RotateArrowHead(Vector3 startPos)
    {
        Vector2 diff = Camera.main.ScreenToWorldPoint(Input.mousePosition) - startPos;
        diff.Normalize();

        float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        arrowHead.transform.rotation = Quaternion.Euler(12f, 0f, rot_z - 90);
    }
    public void InitPositions(int numSegments)
    {
        if (numSegments > 0)
        {
            Vector3 recPos;
            curveAngle = 2 * Mathf.PI / numSegments;
            for (int i = 0; i < allSegments.Count; i++)
            {
                GameObject currentSegment = allSegments[i];
                recPos = new Vector3(0, Mathf.Cos(curveAngle * i - 5 * Mathf.PI / 8) * radius, Mathf.Sin(curveAngle * i - 5 * Mathf.PI / 8) * radius);
                // allSegments[i].transform.position = transform.position + recPos;
                currentSegment.transform.localPosition = recPos;
                if (!animatingSegments.Contains(currentSegment))
                {
                    // if (allSegments.Count > 0)
                    // {
                        // Vector3 offset = transform.position - allSegments[0].transform.position;
                        // segments.transform.position = segments.transform.position + offset;
                    // }

                }

            }
        }
    }
    public void SortSegments()
    {
        allSegments = allSegments.OrderBy(segment => segment.transform.position.z).ToList();
        foreach (GameObject segment in allSegments)
        {
            segment.transform.SetAsLastSibling();
        }
    }
    public void InitRotations()
    {
        // allSegments = allSegments.OrderBy(segment => segment.transform.position.y).ToList();
        for (int i = 0; i < allSegments.Count; i++)
        {

            Quaternion relativeRot = Quaternion.identity;
            if (i < allSegments.Count -1)
            {

                Vector3 relativePos = allSegments[i + 1].transform.localPosition - allSegments[i].transform.localPosition;
                relativeRot = Quaternion.LookRotation(relativePos, allSegments[i].transform.up);
            }
            if ( i == allSegments.Count -1)
            {
                Vector3 relativePos = allSegments[0].transform.localPosition - allSegments[i].transform.localPosition;
                relativeRot = Quaternion.LookRotation(relativePos, allSegments[i].transform.up);
            }
            allSegments[i].transform.localRotation = Quaternion.RotateTowards(allSegments[i].transform.localRotation, relativeRot, 360);
            // allSegments[i].transform.localRotation = Quaternion.Euler(-90, 0, 0);
        }
    }
    public void RotateArrow()
    {
        Vector2 diff = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        diff.Normalize();

        float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, rot_z - 90);
    }
    public void PositionArrow()
    {

    }
    public void AnimateArrow()
    {
        for (int i = 0; i < allSegments.Count; i++ )
        {
            if (i != allSegments.Count - 1)
            {
                StartAnimateSegment(allSegments[i], allSegments[i+1]);
            }
            else if (i == allSegments.Count - 1)
            {
                StartAnimateSegment(allSegments[i], allSegments[0]);
            }
        }
    }
    public void StartAnimateSegment(GameObject currentSegment, GameObject targetSegment)
    {
        if (!animatingSegments.Contains(currentSegment))
        {
            animatingSegments.Add(currentSegment);
            StartCoroutine(AnimateSegment(currentSegment, targetSegment));
        }
    }

    public IEnumerator AnimateSegment(GameObject currentSegment, GameObject targetSegment)
    {
        float counter = 0;
        while (counter < 10000f)
        {
            FilterSegment(currentSegment);
            
            currentSegment.transform.localPosition = Vector3.Slerp(currentSegment.transform.localPosition, targetSegment.transform.localPosition, .002f);

            Vector3 relativePos = targetSegment.transform.localPosition - currentSegment.transform.localPosition;


            Quaternion relativeRot = Quaternion.LookRotation(relativePos);
            currentSegment.transform.localRotation = Quaternion.RotateTowards(currentSegment.transform.localRotation, relativeRot, 360f);

            counter += .0005f;

            yield return null;
        }
        animatingSegments.Remove(currentSegment);
    }
    public void FilterSegment(GameObject segment)
    {
        bool zRange = false;
        bool yRange = false;
        if (segment.transform.localPosition.z < 0)
        {
            zRange = true;
        }
        else if (segment.transform.localPosition.z > 0)
        {
            zRange = false;
        }
        if (segment.transform.localPosition.y > 0 && segment.transform.position.y < Mathf.Abs(arrowHead.transform.localPosition.y))
        {
            yRange = true;
        }
        else if (segment.transform.localPosition.y < 0 || segment.transform.localPosition.y > Mathf.Abs(arrowHead.transform.localPosition.y))
        {
            yRange = false;
        }

        if (zRange == true && yRange ==true)
        {
            segment.SetActive(true);
        }
        else
        {
            segment.SetActive(false);
        }

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
