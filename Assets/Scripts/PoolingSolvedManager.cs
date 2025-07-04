using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PoolingSolvedManager : BaseBoard
{
    // Pre-fabs
    public GameObject poolingViewObject;
    public GameObject loadingScreen;

    // Constants
    readonly int PoolingFunctionAmount = 3;

    // Instances
    public PoolingMiniGamePlaybackDirector playbackDirector;
    Dictionary<string, PoolingViewSolved> poolingViews = new Dictionary<string, PoolingViewSolved>();
    public TimedDialogueBalloon timedDialogueBalloon;
    public DialogueBalloon dialogueBalloon;
    public CameraZoom cameraZoom;

    // Data
    public TextAsset dataText;
    PoolingData data;
    [System.Serializable]
    class PoolingData
    {
        public List<double> inputMatrix = new List<double>();
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
    void Start()
    {
        UpdateProgress(0f);
        LoadMatrix();
        StartCoroutine(LayoutPoolingViews());
    }

    void UpdateProgress(float progress)
    {
        // Debug.Log("Update progress " + progress);
        Image bar = GameObject.Find("ProgressBar").GetComponent<Image>();
        bar.fillAmount = progress;
    }

    IEnumerator LayoutPoolingViews()
    {
        float verticalGap = 4.5f;
        float xPosition = 2f;
        float verticalOffset = 2.5f;

        for (int i = 0; i < PoolingFunctionAmount; i++)
        {
            float yPosition = verticalOffset + i * verticalGap;
            Vector3 position = new(xPosition, yPosition, 0f);
            GameObject instanceView = Instantiate(poolingViewObject, position, Quaternion.identity);
            PoolingViewSolved script = instanceView.GetComponent<PoolingViewSolved>();
            script.InitPoolingBox(GetPoolingType(i));
            script.InitInput(UnflatMatrix(data.inputMatrix, 64));
            script.OnPoolingStopped += OnPoolingStopped;
            poolingViews.Add(GetPoolingType(i), script);
            UpdateProgress((float)(i + 1) / PoolingFunctionAmount);
            yield return null;
        }

        RegisterPoolingViewsMessages();
        loadingScreen.SetActive(false);

        foreach(var script in poolingViews)
        {
            script.Value.StartPooling();
        }
    }

    private void UnregisterPoolingViewsMessages()
    {
        foreach (KeyValuePair<string, PoolingViewSolved> entry in poolingViews)
        {
            entry.Value.OnHover -= DisplayPoolingFunctionMessage;
            entry.Value.OnUnhover -= HidePoolingFunctionMessage;
        }
    }

    private void RegisterPoolingViewsMessages()
    {
        foreach (KeyValuePair<string, PoolingViewSolved> entry in poolingViews)
        {
            entry.Value.OnHover += DisplayPoolingFunctionMessage;
            entry.Value.OnUnhover += HidePoolingFunctionMessage;
        }
    }

    private string GetPoolingType(int idx)
    {
        switch (idx)
        {
            case 0:
                return "Min";
            case 1:
                return "Max";
            case 2:
                return "Average";
        }
        return "";
    }

    void LoadMatrix()
    {
        // Debug.Log(dataText.text);
        data = JsonUtility.FromJson<PoolingData>(dataText.text);

        if (data == null)
        {
            Debug.LogError("Failed to retrieve from JSON");
        }
    }

    void OnPoolingStopped(string type)
    {
        // Update outputline
        if (poolingViews[type].HasPoolingBox())
        {
            if (type == "Max")
            {
                poolingViews[type].UpdateOutputState("correct");
            }
            else
            {
                poolingViews[type].UpdateOutputState("wrong");
            }
        }
        else
        {
            poolingViews[type].UpdateOutputState("inactive");
        }
    }

    private void DisplayPoolingFunctionMessage(string type)
    {
        string message = "";
        switch (type)
        {
            case "Max":
                message = "Max function picks the highest value from a group. In pooling, it keeps the strongest feature (in this case the darkest pixel).";
                break;
            case "Min":
                message = "The Min function selects the lowest value from a group. It focuses on the weakest feature in that region (in this case the brighter pixel).";
                break;
            case "Average":
                message = "The Average function calculates the mean of all the values in the group. It gives a balanced view of the area, blending all the features together.";
                break;
        }
        timedDialogueBalloon.SetSpeaker(Player.gameObject);
        timedDialogueBalloon.SetMessage(message);
        timedDialogueBalloon.PlaceUpperLeft();
        timedDialogueBalloon.Show();
    }

    private void HidePoolingFunctionMessage(string type)
    {
        timedDialogueBalloon.Hide();
    }

    protected override void GameOver()
    {
        GameManager.instance.solvedMinigames["Pooling 1"] = true;

        Player.Enable();
        cameraZoom.ChangeZoomTarget(Player.gameObject);

        GameObject.FindGameObjectWithTag("Wormhole").GetComponent<SpriteRenderer>().color = Color.green;
    }
}
