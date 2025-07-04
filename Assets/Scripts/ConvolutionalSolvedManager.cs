using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ConvolutionalMiniGameResolvedManager : BaseBoard
{
    // Pre-fabs
    public GameObject convolutionalViewObject;
    public GameObject loadingScreen;

    // UI constants
    public static float pixelSize = 0.01f;
    static public float verticalOffsetImages = 5f;
    readonly int KernelAmount = 3;

    // Instances
    List<ConvolutionalViewSolved> convolutionalViews = new List<ConvolutionalViewSolved>();
    public TimedDialogueBalloon timedDialogueBalloon;

    public DialogueBalloon dialogueBalloon;
    public HintBalloon hintBalloon;

    public CameraZoom cameraZoom;

    public TextAsset convDataText;
    ConvData data;
    [System.Serializable]
    class ConvData
    {
        public List<double> inputMatrix = new List<double>();
        public List<double> kernelMatrix = new List<double>();
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

    // Start is called before the first frame update
    void Awake()
    {
        UpdateProgress(0f);
        LoadMatrix();
        StartCoroutine(LayoutConvolutionalViews());
    }

    void UpdateProgress(float progress)
    {
        Image bar = GameObject.Find("ProgressBar").GetComponent<Image>();
        bar.fillAmount = progress;
    }

    IEnumerator LayoutConvolutionalViews()
    {
        float verticalGap = 4.5f;
        float xPosition = 2f;
        float verticalOffset = 2.5f;

        for (int i = 0; i < KernelAmount; i++)
        {
            float yPosition = verticalOffset + i * verticalGap;
            Vector3 position = new(xPosition, yPosition, 0f);
            GameObject instanceView = Instantiate(convolutionalViewObject, position, Quaternion.identity);
            ConvolutionalViewSolved script = instanceView.GetComponent<ConvolutionalViewSolved>();
            script.InitKernel(GetFlatKernelMatrix(i), UnflatMatrix(GetFlatKernelMatrix(i), 3));
            yield return null;
            script.InitInput(UnflatMatrix(data.inputMatrix, 64));
            script.OnConvolutionStopped += OnConvolutionStopped;
            convolutionalViews.Add(script);
            UpdateProgress((float)(i + 1) / KernelAmount);
            yield return null;
        }

        RegisterConvolutionalViewsMessages();
        loadingScreen.SetActive(false);

        foreach(var script in convolutionalViews)
        {
            script.StartConvolution();
        }
    }

    private void UnregisterConvolutionalViewsMessages()
    {
        for (int i = 0; i < KernelAmount; i++)
        {
            ConvolutionalViewSolved script = convolutionalViews[i];
            script.OnHover -= DisplayKernelMessage;
            script.OnUnhover -= HideKernelMessage;
        }
    }

    private void RegisterConvolutionalViewsMessages()
    {
        for (int i = 0; i < KernelAmount; i++)
        {
            ConvolutionalViewSolved script = convolutionalViews[i];
            script.OnHover += DisplayKernelMessage;
            script.OnUnhover += HideKernelMessage;
        }
    }

    List<double> GetFlatKernelMatrix(int i)
    {
        List<double> flatKernel = new List<double>();
        //https://www.researchgate.net/figure/Vertical-and-horizontal-edge-detector-kernel_fig3_343947492
        switch (i)
        {
            case 0:
                // double[,] verticalEdgeDetection = {
                //         {1, 0, -1},
                //         {1, 0, -1},
                //         {1, 0, -1},
                //     };
                List<double> flatVerticalEdgeDetection = new List<double> { 1, 0, -1, 1, 0, -1, 1, 0, -1 };
                flatKernel = flatVerticalEdgeDetection;
                break;
            case 1:
                // double[,] horizontalEdgeDetection = {
                //         {1, 1, 1},
                //         {0,0,0},
                //         {-1,-1,-1},
                //     };
                List<double> flatHorizontalEdgeDetection = new List<double> { 1, 1, 1, 0, 0, 0, -1, -1, -1 };
                flatKernel = flatHorizontalEdgeDetection;
                break;
            case 2:
                flatKernel = data.kernelMatrix;
                break;
            default:
                double[,] zeroes = new double[3, 3];
                List<double> flatZeroes = new List<double> { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                flatKernel = flatZeroes;
                break;
        }

        return flatKernel;
    }

    void OnConvolutionStopped(int id)
    {
        if (id <= convolutionalViews.Count && id > 0)
        {
            // Update outputline
            if (convolutionalViews[id - 1].HasKernel())
            {
                switch (id)
                {
                    case 1:
                        convolutionalViews[0].UpdateOutputState("wrong");
                        break;
                    case 2:
                        convolutionalViews[1].UpdateOutputState("wrong");
                        break;
                    case 3:
                        convolutionalViews[2].UpdateOutputState("correct");
                        break;
                }
            }
            else
            {
                convolutionalViews[id - 1].UpdateOutputState("inactive");
            }
        }
    }

    void LoadMatrix()
    {
        // Debug.Log(convDataText.text);
        data = JsonUtility.FromJson<ConvData>(convDataText.text);

        if (data == null)
        {
            Debug.LogError("Failed to retrieve from JSON");
        }
    }

    private void DisplayKernelMessage(int id)
    {
        string message = "";
        switch (id)
        {
            case 1:
                message = "This is a vertical edge detection kernel. It highlights vertical features, such as vertical agricultural fields boundaries, edges of water bodies or forests.";
                break;
            case 2:
                message = "This is a horizontal edge detection kernel. It highlights horizontal features, such as horizontal agricultural field boundaries, coastlines or riverbanks.";
                break;
            case 3:
                message = "This kernel detects continuous regions, not just edges, and assigns different pixel values (shades of blue) to highlight different areas.";
                // message = "This kernel identify continuous regions, and not only horizontal and vertical edges. Plus, it generates different pixel values (shades of blue) to different regions.";
                break;
        }
        timedDialogueBalloon.SetSpeaker(Player.gameObject);
        timedDialogueBalloon.SetMessage(message);
        timedDialogueBalloon.PlaceUpperLeft();
        timedDialogueBalloon.Show();
    }

    private void HideKernelMessage(int id)
    {
        timedDialogueBalloon.Hide();
    }

    protected override void GameOver()
    {
        GameManager.instance.solvedMinigames["Convolutional 1"] = true;

        Player.Enable();
        cameraZoom.ChangeZoomTarget(Player.gameObject);

        GameObject.FindGameObjectWithTag("Wormhole").GetComponent<SpriteRenderer>().color = Color.green;
    }

}
