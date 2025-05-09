

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Game Object - Prefab
// positions pre-defined - a tiny label for each
// rectangle with matching color
// Connections towards the end of the room foreach rectangle
// Label
// Message - display on Stay
// void MatchSpectralBand() -> place the different bands of the same Sample aligned (same position)
public class SpectralBandContainer : MonoBehaviour
{
    public event Action<string> OnHover;
    public event Action<string> OnUnhover;
    string type;

    public Sprite sprite;

    // TODO: abstract OutputLine
    string lineState = "inactive"; // inactive, wrong, correct
    LineRenderer outputLineRenderer;
    readonly float inactiveWidth = 0.05f;
    private Color workingStartColor;
    private Color workingEndColor;
    private Color wrongColor = Color.red;
    private Color inactiveColor = Color.gray;

    Dictionary<string, string> typeToLabel = new Dictionary<string, string> {
        {"red", "Red (B4)"},
        {"green", "Green (B3)"},
        {"blue", "Blue (B2)"},
        {"swir", "SWIR (B12)"},
        {"redEdge", "Red Edge (B5)"}
    };

    public event Action<string> OnFilled;
    private int countMatched = 0;
    private const int totalMatched = 1;

    public void SetType(string bandType)
    {
        type = bandType;

        TextMeshPro label = transform.Find("SpectralBandLabel").GetComponent<TextMeshPro>();
        label.text = typeToLabel[type];

        Transform square = transform.Find("Square");
        SpriteRenderer spriteRenderer = square.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = GetColor();
    }

    public bool IsMatch(SampleSpectralBand sampleSpectralBand)
    {
        // Debug.Log("Spectral band" + sampleSpectralBand.GetBandType() + " is MATCH with ?" + type);
        return sampleSpectralBand.GetBandType().Equals(type);
    }

    public void MatchSpectralBand(SampleSpectralBand sampleSpectralBand)
    {
        Transform parentSquare = transform;
        if (parentSquare == null)
        {
            Debug.LogError("Failed to get parent based on sample spectral band class");
            return;
        }

        // float verticalOffset = 0.2f;
        // Change scale
        sampleSpectralBand.gameObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.6f);

        // change box parent
        sampleSpectralBand.gameObject.transform.parent = parentSquare;
        sampleSpectralBand.gameObject.transform.position = new Vector3(parentSquare.position.x, parentSquare.position.y);
        sampleSpectralBand.Block();

        countMatched++;
        if (countMatched == totalMatched)
        {
            OnFilled?.Invoke(type);
        }
    }

    Color GetColor()
    {
        switch (type)
        {
            case "red":
                return Color.red;
            case "green":
                return Color.green; // dark green
            case "blue":
                return Color.blue;
            case "swir":
                return Color.yellow; // bright green
            case "redEdge":
                return Color.magenta;
            default:
                return Color.white;
        }
    }

    public void DrawConnections(Vector3 inputPosition)
    {
        DrawInputConnection(inputPosition);
        DrawOutputConnection();
    }

    void DrawInputConnection(Vector3 inputPosition)
    {
        Transform line = transform.Find("InputLine");
        if (line == null)
        {
            Debug.LogError("Failed to retrieve Line");
            return;
        }
        LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogError("Failed to retrieve LineRenderer");
            return;
        }

        Vector3 startPoint = inputPosition;
        Vector3 endPoint = new(-0.5f, -1f, 0f);
        // Debug.Log("Draw connection from " + startPoint + " to " + endPoint);
        Connection conn = new(startPoint, endPoint, lineRenderer);
        conn.DrawLine(5f);
    }

    void DrawOutputConnection()
    {
        Transform line = transform.Find("OutputLine");
        if (line == null)
        {
            Debug.LogError("Failed to retrieve Line");
            return;
        }
        LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogError("Failed to retrieve LineRenderer");
            return;
        }

        workingStartColor = lineRenderer.startColor;
        workingEndColor = lineRenderer.endColor;

        Vector3 startPoint = new(-0.1f, -1f, 0f);
        Vector3 endPoint = new(3.4f, -1f, 0f);
        // Debug.Log("Draw connection from " + startPoint + " to " + endPoint);
        Connection conn = new(startPoint, endPoint, lineRenderer);
        conn.DrawStraightLine();

        outputLineRenderer = conn.lineRenderer;
    }

    public void Reset()
    {
        countMatched = 0;

        UpdateState("inactive");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (countMatched > 0)
        {
            OnHover?.Invoke(type);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (countMatched > 0)
        {
            OnHover?.Invoke(type);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (countMatched > 0)
        {
            OnUnhover?.Invoke(type);
        }
    }

    public void UpdateState(string newLineState)
    {
        lineState = newLineState;
        switch (lineState)
        {
            case "correct":
                outputLineRenderer.startColor = workingStartColor;
                outputLineRenderer.endColor = workingEndColor;
                break;
            case "wrong":
                outputLineRenderer.material.color = Color.white;
                outputLineRenderer.startColor = Color.white;
                outputLineRenderer.endColor = Color.white;
                outputLineRenderer.startWidth = inactiveWidth;
                outputLineRenderer.endWidth = inactiveWidth;
                break;
            case "inactive":
            default:
                outputLineRenderer.material.color = inactiveColor;
                outputLineRenderer.startColor = inactiveColor;
                outputLineRenderer.endColor = inactiveColor;
                outputLineRenderer.startWidth = inactiveWidth;
                outputLineRenderer.endWidth = inactiveWidth;
                break;
        }
    }

    void Update()
    {
        switch (lineState)
        {
            case "correct":
                outputLineRenderer.material.color = Color.Lerp(Color.white, Color.cyan, Mathf.PingPong(Time.time, 0.5f));
                outputLineRenderer.startWidth = Mathf.Lerp(inactiveWidth, inactiveWidth * 5, Mathf.PingPong(Time.time, 0.5f));
                outputLineRenderer.endWidth = Mathf.Lerp(inactiveWidth, inactiveWidth * 5, Mathf.PingPong(Time.time, 0.5f));
                break;
            case "wrong":
                outputLineRenderer.material.color = Color.white;
                outputLineRenderer.startColor = Color.white;
                outputLineRenderer.endColor = Color.white;
                outputLineRenderer.startWidth = inactiveWidth;
                outputLineRenderer.endWidth = inactiveWidth;
                break;
            case "inactive":
            default:
                outputLineRenderer.material.color = inactiveColor;
                outputLineRenderer.startColor = inactiveColor;
                outputLineRenderer.endColor = inactiveColor;
                outputLineRenderer.startWidth = inactiveWidth;
                outputLineRenderer.endWidth = inactiveWidth;
                break;
        }
    }

}
