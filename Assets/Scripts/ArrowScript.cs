using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
public class ArrowScript : MonoBehaviour
{
    public GameObject arrowHead;
    public GameObject bodySegment;
    public List<GameObject> allSegments, animatingSegments;
    public Transform segments, segmentMask;
    public int numSegments;
    public float curveAngle;
    public float curveTilt;
    public float radius;
    private void Awake()
    {
        for (int i = 0; i < numSegments; i++)
        {
            GameObject newSegment = Instantiate(bodySegment, transform.position, Quaternion.identity, segmentMask.Find("Offset"));
            allSegments.Add(newSegment);

        }
        InitArrow();
    }
    public void InitArrow()
    {
        radius = 600f;
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
        FormatSegmentMask(localPos);
        RotateArrow();
        RotateArrowHead(startPos);
        AnimateArrow();
        SortSegments();
    }
    public void PositionArrowHead()
    {
        arrowHead.transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
    public void FormatSegmentMask(Vector2 localPos)
    {
        Image maskImage = segmentMask.GetComponent<Image>();
        Vector2 segmentPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(segmentMask.GetComponent<RectTransform>(), Input.mousePosition, Camera.main, out segmentPos);
        maskImage.rectTransform.sizeDelta = new Vector2(50, localPos.y - segments.transform.position.y);
        Vector3 newPos = new Vector3 (0, -segments.localPosition.y + maskImage.rectTransform.rect.height/2, 0);
        segmentMask.transform.localPosition = newPos;
        newPos = new Vector3 (0, segments.localPosition.y - maskImage.rectTransform.rect.height/2 + radius, 0);
        segmentMask.transform.Find("Offset").localPosition = newPos;

    }
    public void RotateArrowHead(Vector3 startPos)
    {
        Vector2 diff = Camera.main.ScreenToWorldPoint(Input.mousePosition) - startPos;
        diff.Normalize();
        Vector2 localPos = GetComponent<RectTransform>().position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), Input.mousePosition, Camera.main, out localPos);

        float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        float rot_x = Mathf.Tan((Mathf.PI * localPos.y/(4*radius)) ) * Mathf.Rad2Deg;
        rot_x = Mathf.Clamp(rot_x, -68, 68);

        arrowHead.transform.localRotation = Quaternion.Euler(rot_x,0, 0);
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
        // bool yRange = false;
        float ySegmentDiff = Mathf.Abs(segment.transform.position.y - transform.position.y);
        float xSegmentDiff = Mathf.Abs(segment.transform.position.x - transform.position.x);
        float yArrowDiff = Mathf.Abs(arrowHead.transform.position.y - transform.position.y);
        float xArrowDiff = Mathf.Abs(arrowHead.transform.position.x - transform.position.x);
        if (segment.transform.localPosition.z < 0)
        {
            zRange = true;
        }
        else if (segment.transform.localPosition.z > 0)
        {
            zRange = false;
        }
        // if (segment.transform.localPosition.y > 0)
        // {
        //     if ((yArrowDiff > .03f && xArrowDiff > .03f) && xSegmentDiff < xArrowDiff + .1f && ySegmentDiff < yArrowDiff +.1f)
        //     {
        //         yRange = true;
        //     }
        //     else if ((yArrowDiff < .05f && xSegmentDiff < xArrowDiff) || (xArrowDiff < .05f && ySegmentDiff < yArrowDiff))
        //     {
        //         // Debug.Log(yArrowDiff + " " + xArrowDiff);
        //         yRange = true;
        //     }
        // }
        // else if (segment.transform.localPosition.y < 0 || ySegmentDiff > yArrowDiff)
        // {
        //     yRange = false;
        // }

        if (zRange == true)
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
