using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PoolingView : MonoBehaviour
{
    public event Action<string> OnHover;
    public event Action<string> OnUnhover;
    public Action<string> OnPoolingStopped;

    // Scene objects
    public InputHolder poolingBoxHolder;
    public Locker locker;
    public GameObject inputScreen;
    public GameObject outputScreen;

    public GameObject center;

    // Matrices and functions
    public PoolingBox poolingBox;
    PoolingBox movingPoolingBox;
    public InputMatrix inputMatrix;
    public OutputPoolingMatrix outputMatrix;
    public string type;
    public int id;
    static int viewCounter = 0;

    // Activation
    bool isApplying = false;
    bool poolingBoxAtInputHolder = false;
    int animationStep = 32;
    int iPooling = 0;
    int jPooling = 0;
    int matrixSize = 62;
    // TODO: abstract OutputLine
    string outputState = "inactive"; // inactice, wrong, correct
    LineRenderer outputLineRenderer;
    readonly float inactiveWidth = 0.02f;
    private Color workingStartColor;
    private Color workingEndColor;
    private Color wrongColor = Color.red;
    private Color inactiveColor = Color.gray;

    void Awake()
    {
        viewCounter++;
        id = viewCounter;

        ResetPooling();

        LayoutKernelHolder();
        LayoutInputScreen();
        LayoutOutputScreen();
    }

    /* UI-related methods */

    private void LayoutKernelHolder()
    {
        poolingBoxHolder.Init(id);
        poolingBoxHolder.DrawConnection();
        poolingBoxHolder.OnAddedObject += StartPooling;
    }

    private void LayoutInputScreen()
    {
        // Draw line
        Transform line = inputScreen.transform.Find("OutputLine");
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

        Vector3 startPoint = new(0f, -1f, 0f);
        Vector3 endPoint = new(2f, -0.5f, 0f);
        Connection conn = new(startPoint, endPoint, lineRenderer);
        conn.DrawLine(1f);
    }

    private void LayoutOutputScreen()
    {
        Transform line = outputScreen.transform.Find("OutputLine");
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

        Vector3 startPoint = new(0f, -0.5f, 0f);
        Vector3 endPoint = new(2.5f, -0.5f, 0f);
        Connection conn = new(startPoint, endPoint, lineRenderer);
        conn.DrawStraightLine();

        outputLineRenderer = conn.lineRenderer;
        UpdateOutputState("inactive");
    }

    public GameObject GetPivot()
    {
        return center;
    }

    /* Activation method */

    void ResetPooling()
    {
        iPooling = 0;
        jPooling = 0;
        isApplying = false;
    }

    public void InitInput(double[,] input)
    {
        inputMatrix.SetColor("redblue");
        inputMatrix.SetMatrix(input, matrixSize);
    }

    public bool IsApplyingPooling()
    {
        return isApplying;
    }

    public bool HasPoolingBox()
    {
        return poolingBoxAtInputHolder;
    }

    public void InitPoolingBox(string type)
    {
        this.type = type;
        poolingBox.Init(id);
        poolingBox.SetFunction(type);

        locker.Init(id);
        locker.AddPoolingBox(poolingBox.gameObject);

        GameObject instance = Instantiate(poolingBox.gameObject, poolingBox.transform.position, Quaternion.identity);
        movingPoolingBox = instance.GetComponent<PoolingBox>();
        movingPoolingBox.gameObject.SetActive(false);
        movingPoolingBox.Init(id);
        movingPoolingBox.SetFunction(type);
    }

    public void RemovePoolingBox()
    {
        poolingBoxAtInputHolder = false;
        OnUnhover?.Invoke(type);
        StopPooling();
        ResetPooling();
        outputMatrix.Reset();
    }

    void StartPooling()
    {
        poolingBoxAtInputHolder = true;
        poolingBox.transform.localScale = new(0.3f, 0.3f, 1f);
        poolingBox.OnGrabbed += RemovePoolingBox;

        GameObject inputPixel = inputMatrix.GetPixelObject(iPooling, jPooling);
        movingPoolingBox.PlaceAt(inputPixel.transform.position);
        movingPoolingBox.transform.localScale = new(0.1f, 0.1f, 1f);
        movingPoolingBox.gameObject.SetActive(true);

        outputMatrix.Reset();
        iPooling = 0;
        jPooling = 0;
        isApplying = true;

        int iOutput = 0;
        int jOutput = 0;

        for (int i = 1; i < matrixSize; i+=2)
        {
            for (int j = 1; j < matrixSize; j+=2)
            {
                List<double> listPixels = new List<double>()
                {
                    inputMatrix.GetPixelValue(i,j),
                    inputMatrix.GetPixelValue(i+1,j),
                    inputMatrix.GetPixelValue(i,j+1),
                    inputMatrix.GetPixelValue(i+1,j+1)
                };

                double poolingResult = poolingBox.ApplyFunction(listPixels);

                // retrieve the pixel from the output matrix
                // change its value and color
                outputMatrix.SetPixel(iOutput, jOutput, poolingResult, isActivationResult: true);
                outputMatrix.HidePixel(iOutput, jOutput);

                jOutput++;
            }
            jOutput = 0;
            iOutput++;
        }
    }

    void ApplyPooling()
    {
        if (!isApplying)
        {
            return;
        }

        // move the kernel center over it
        GameObject inputPixel = inputMatrix.GetPixelObject(iPooling, jPooling);
        movingPoolingBox.PlaceAt(inputPixel.transform.position);

        int jNext = jPooling;
        int iNext = iPooling;
        int stepDone = 0;
        // Debug.Log("start loop iConvNext " + iConvNext + ", jConvNext " + jConvNext);
        while ((stepDone < animationStep) && (jNext < matrixSize) && (iNext < matrixSize))
        {
            // Debug.Log("iConvNext " + iNext + " limit " + matrixSize);
            // Debug.Log("jConvNext " + jNext + " limit " + (jActivation + animationStep));
            outputMatrix.ShowPixel(iNext, jNext);
            jNext++;
            if (jNext >= matrixSize)
            {
                iNext++;
                jNext = 1; // stride
            }
            stepDone++;
        }
        // Debug.Log("end loop iConvNext " + iConvNext + ", jConvNext " + jConvNext);


        jPooling += animationStep;
        if (jPooling >= matrixSize)
        {
            iPooling++;
            jPooling = 0;
        }
        if (iPooling >= matrixSize)
        {
            StopPooling();
        }
    }

    void StopPooling()
    {
        poolingBox.OnGrabbed -= StopPooling;
        movingPoolingBox.gameObject.SetActive(false);

        isApplying = false;
        OnPoolingStopped?.Invoke(type);
    }

    // Update is called once per frame
    void Update()
    {
        ApplyPooling();
        AnimateOutputState();
    }


    // TODO: abstract OutputLine
    public void UpdateOutputState(string newLineState)
    {
        outputState = newLineState;
        switch (outputState)
        {
            case "correct":
                outputLineRenderer.startColor = workingStartColor;
                outputLineRenderer.endColor = workingEndColor;
                break;
            case "wrong":
                outputLineRenderer.material.color = wrongColor;
                outputLineRenderer.startColor = wrongColor;
                outputLineRenderer.endColor = wrongColor;
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

    public void AnimateOutputState()
    {
        if (!outputLineRenderer)
        {
            // Debug.Log("Could not find output line render");
            return;
        }

        if (outputState.Equals("correct"))
        {
            outputLineRenderer.material.color = Color.Lerp(Color.white, Color.cyan, Mathf.PingPong(Time.time, 0.5f));
            outputLineRenderer.startWidth = Mathf.Lerp(inactiveWidth, inactiveWidth * 5, Mathf.PingPong(Time.time, 0.5f));
            outputLineRenderer.endWidth = Mathf.Lerp(inactiveWidth, inactiveWidth * 5, Mathf.PingPong(Time.time, 0.5f));
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Debug.Log("on trigger enter activation view");
        if (HasPoolingBox())
        {
            OnHover?.Invoke(type);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (HasPoolingBox())
        {
            OnHover?.Invoke(type);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Debug.Log("on trigger exit activation view");
        if (HasPoolingBox())
        {
            OnUnhover?.Invoke(type);
        }
    }

}
