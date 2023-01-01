using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class CurveTest : MonoBehaviour
{
    public GameObject rectangle;
    public Transform center;
    public float radius;
    public List<GameObject> segments;
    void Start()
    {
        SpawnCurve(9);
    }

    public void SpawnCurve(int numPoints)
    {
        Vector3 recPos;
        // float angle = Mathf.PI / numPoints;
        float angle = 1.8f / numPoints;
        for (int i = 0; i < numPoints; i++)
        {
            recPos = new Vector3(0, Mathf.Cos(angle * i) * radius, Mathf.Sin(angle * i) * radius);
            GameObject segment = Instantiate(rectangle, transform.position, Quaternion.identity, transform);
            segment.transform.localPosition = recPos;
            segments.Add(segment);

        }
        SortSegments();
        RotateSegments();

    }
    public void SortSegments()
    {
        segments = segments.OrderBy(segment => segment.transform.position.z).ToList();
        foreach (GameObject segment in segments)
        {
            segment.transform.SetAsLastSibling();
        }
    }

    public void RotateSegments()
    {
        segments = segments.OrderBy(segment => segment.transform.position.y).ToList();
        for (int i = 0; i < segments.Count; i++)
        {
            if (i != segments.Count-1)
            {
                Vector3 relativePos = segments[i + 1].transform.position - segments[i].transform.position;
                Quaternion relativeRot = Quaternion.LookRotation(relativePos, segments[i].transform.up);
                segments[i].transform.rotation = Quaternion.RotateTowards(segments[i].transform.rotation, relativeRot, 360);
                // Debug.Log(relativePos);
                // Debug.Log(relativeRot);

            }

        }
    }
}
