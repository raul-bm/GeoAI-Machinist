using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DenseView : MonoBehaviour
{
    public event Action<string> OnHover;
    public event Action<string> OnUnhover;

    // Prefab
    public GameObject lineObject;
    public GameObject logitObject;
    Transform weightsRoot;
    LogitNode logitNode;

    // Properties
    string label = "";
    string address = "";

    Vector3 logitPosition = new(5f, 4.7f, 0f);
    readonly float MinWeight = -0.15f;
    readonly float MaxWeight = 0.15f;
    readonly float MinLogit = -26f;
    readonly float MaxLogit = 16f;
    double greenCounter = 0;
    double redCounter = 0;
    double negligibleCounter = 0;
    float lineWidth = 0.05f; // same as pixel size in the FlatMatrix

    // Data
    TextAsset dataText;
    OutputData data;

    [System.Serializable]
    class OutputData
    {
        public string label;
        public double logit;
        public double bias;

        public List<double> weights = new List<double>();
    }

    double[,] UnflatMatrix(List<double> flatten, int size)
    {
        double[,] unflatten = new double[size, size];
        for (int k = 0; k < flatten.Count; k++)
        {
            double value = flatten[k];
            int i = k / size;
            int j = k - i * size;
            // Debug.Log("flatenning: k " + k + ", value " + value + ", i " + i + ", j" + j);
            unflatten[i, j] = value;
        }

        return unflatten;
    }

    void Awake()
    {
        weightsRoot = transform.Find("WeightsRoot");
    }

    public void SetType(string type)
    {
        label = type;

        address = label + "OutputData";
        Addressables.LoadAssetAsync<TextAsset>(address).Completed += OnLoadDone;
    }

    // Instantiate the loaded prefab on complete
    private void OnLoadDone(AsyncOperationHandle<TextAsset> operation)
    {
        if (operation.Status == AsyncOperationStatus.Succeeded)
        {
            dataText = operation.Result;
            ReadMatrix();
            LayoutLogit();
            LayoutWeightLines();
        }
        else
        {
            Debug.LogError($"Asset for {address} failed to load.");
        }
    }

    void ReadMatrix()
    {
        // Debug.Log(dataText.text);
        data = JsonUtility.FromJson<OutputData>(dataText.text);
        if (data == null)
        {
            Debug.LogError("Failed to retrieve from JSON");
        }
    }

    void LayoutWeightLines()
    {
        float verticalOffset = 4.3f;
        float horizontalOffset = 1f;
        float gap = lineWidth / 3;

        float maxYPosition = verticalOffset + data.weights.Count * lineWidth - transform.position.y;
        // Debug.Log("maxYPosition " + maxYPosition);
        // Debug.Log("data.weights.Count * lineWidth " + data.weights.Count * lineWidth);
        // Debug.Log("transform.position.y " + transform.position.y);
        greenCounter = 0;
        redCounter = 0;
        negligibleCounter = 0;
        for (int i = 0; i < data.weights.Count; i++)
        {
            GameObject instance = Instantiate(lineObject, new(0f, 0f, 0f), Quaternion.identity);
            instance.transform.parent = weightsRoot;
            instance.transform.localPosition = new(0f, 0f, 0f);

            // Draw a line
            float xPosition = horizontalOffset;
            float yPosition = maxYPosition - i * lineWidth;
            Vector3 position = new(xPosition, yPosition, 0f);
            LineRenderer lineRenderer = instance.GetComponent<LineRenderer>();
            Vector3 startPoint = position;
            Vector3 endPoint = logitPosition;

            Connection conn = new(startPoint, endPoint, lineRenderer);
            conn.DrawStraightLine();

            // Change color according to weight value
            double weight = data.weights[i];
            lineRenderer.startColor = GetLineColor(weight);
            lineRenderer.endColor = lineRenderer.startColor;
            lineRenderer.startWidth = lineWidth - gap;
            lineRenderer.endWidth = lineRenderer.startWidth;

            weightsRoot.gameObject.SetActive(false);
        }
        // Debug.Log(label);
        // Debug.Log("green " + greenCounter);
        // Debug.Log("red counter " + redCounter);
        // Debug.Log("above threshold counter " + (greenCounter + redCounter));
        // Debug.Log("negligible " + negligibleCounter);

    }

    Color GetLineColor(double weight)
    {
        double threshold = 0.045f;
        if (weight > threshold)
        {
            greenCounter++;
            return Color.green;
        }
        if (weight < -threshold)
        {
            redCounter++;
            return Color.red;
        }
        negligibleCounter++;
        return Color.gray;
    }

    void LayoutLogit()
    {
        // GameObject instance = Instantiate(logitObject, logitPosition, Quaternion.identity);
        logitObject.transform.localPosition = logitPosition;
        logitObject.transform.localScale = new(0.6f, 0.6f, 0f);

        logitNode = logitObject.GetComponent<LogitNode>();
        logitNode.SetLogitMode(data.logit);

        Dictionary<string, string> labels = new Dictionary<string, string>{
            {"highway", "Highway"},
            {"forest", "Forest"},
            {"river", "River"},
            {"permanentcrop", "Permanent Crop"},
            {"industrial", "Industrial"},
            {"annualcrop", "Annual Crop"},
            {"sealake", "Sea or Lake"},
            {"herbaceous", "Herbaceous Vegetation"},
            {"residential", "Residential"},
            {"pasture", "Pasture"},
        };

        logitNode.SetLabel(labels[label]);
        logitNode.OnHover += ShowWeights;
        logitNode.OnUnhover += HideWeights;
    }

    public LogitNode GetLogitNode()
    {
        return logitNode;
    }

    public void ShowWeights()
    {
        // Debug.Log("On hover logit node show weights");
        weightsRoot.gameObject.SetActive(true);
    }

    public void HideWeights()
    {
        // Debug.Log("On unhover logit node hide weights");
        weightsRoot.gameObject.SetActive(false);
    }
}
