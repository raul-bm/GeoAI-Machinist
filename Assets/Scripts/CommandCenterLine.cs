using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandCenterLine : MonoBehaviour
{
    float xStart = 0f;
    float yStart = 2.1f;
    float xEnd = 10f;
    float yEnd = 5.5f;

    // Start is called before the first frame update
    void Start()
    {
        LineRenderer lineRenderer = transform.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogError("Failed to retrieve LineRenderer");
            return;
        }

        Vector3 startPoint = new(xStart, yStart, 0f);
        Vector3 endPoint = new(xEnd, yEnd, 0f);

        Connection conn = new(startPoint, endPoint, lineRenderer);
        conn.DrawLine(4f);
    }
}
