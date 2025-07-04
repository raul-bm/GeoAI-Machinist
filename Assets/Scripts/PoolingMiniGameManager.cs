using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PoolingMiniGameManager : BaseBoard
{
    // Pre-fabs
    public GameObject poolingViewObject;
    public GameObject loadingScreen;

    // Constants
    readonly int PoolingFunctionAmount = 3;

    // Instances
    public PoolingMiniGamePlaybackDirector playbackDirector;
    Dictionary<string, PoolingView> poolingViews = new Dictionary<string, PoolingView>();
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
        StartCoroutine(LayoutAll());
    }

    void UpdateProgress(float progress)
    {
        // Debug.Log("Update progress " + progress);
        Image bar = GameObject.Find("ProgressBar").GetComponent<Image>();
        bar.fillAmount = progress;
    }

    void IncrementProgress(float progress)
    {
        // Debug.Log("IncrementProgress by " + progress);
        Image bar = GameObject.Find("ProgressBar").GetComponent<Image>();
        bar.fillAmount += progress;
    }


    // Start is called before the first frame update
    IEnumerator LayoutAll()
    {
        InitializeTilemap();

        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                DrawFloor(x, y);

                if (IsBorder(x, y) && !IsExit(x, y))
                {
                    DrawWall(x, y);
                }
                if (IsExit(x, y))
                {
                    DrawExit(x, y);
                }
            }
            IncrementProgress(0.01f);
            yield return null;
        }

        Player.Spawn(this, new Vector2Int(2, 1));
        NPC.Spawn(this, new Vector2Int(1, 1));
        Player.Disable();
        Player.OnDropObject += CheckGameOver;

        LoadMatrix();

        StartCoroutine(LayoutPoolingViews());
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
            PoolingView script = instanceView.GetComponent<PoolingView>();
            script.InitPoolingBox(GetPoolingType(i));
            script.InitInput(UnflatMatrix(data.inputMatrix, 64));
            script.OnPoolingStopped += OnPoolingStopped;
            poolingViews.Add(GetPoolingType(i), script);
            UpdateProgress((float)(i + 1) / PoolingFunctionAmount);
            yield return null;
        }

        RegisterPoolingViewsMessages();
        loadingScreen.SetActive(false);
        playbackDirector.StartAnimation();
        playbackDirector.OnEnd += SetupNPC;
    }

    void SetupNPC()
    {
        NPC.OnHover += DisplayInstruction;
    }

    void DisplayInstruction()
    {
        // NPC speaks message
        string robotMessage = "Choose the best pooling function that reduces the data of the image but keeps the important features.";
        dialogueBalloon.SetSpeaker(NPC.gameObject);
        dialogueBalloon.SetMessage(robotMessage);
        dialogueBalloon.PlaceUpperLeft();
        dialogueBalloon.Show();
        dialogueBalloon.OnDone += dialogueBalloon.Hide;
    }

    private void UnregisterPoolingViewsMessages()
    {
        foreach (KeyValuePair<string, PoolingView> entry in poolingViews)
        {
            entry.Value.OnHover -= DisplayPoolingFunctionMessage;
            entry.Value.OnUnhover -= HidePoolingFunctionMessage;
        }
    }

    private void RegisterPoolingViewsMessages()
    {
        foreach (KeyValuePair<string, PoolingView> entry in poolingViews)
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

        if (NeedRemoveWrongPooling())
        {
            StartCoroutine(OnWrongPooling());
        }
        else
        {
            CheckGameOver();
        }
    }

    void CheckGameOver()
    {
        if (Player.IsGrabbing())
        {
            return;
        }
        if (IsGameOver())
        {
            StartCoroutine(AnimateGameOver());
        }
        else if (NeedRemoveWrongPooling())
        {
            DisplayWrongPoolingMessage();
            NPC.OnHover += DisplayWrongPoolingMessage;
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

    IEnumerator AnimateGameOver()
    {
        yield return new WaitForSeconds(1f);

        Player.Disable();
        NPC.OnHover -= DisplayInstruction;

        // Show the correct answer
        cameraZoom.ChangeZoomTarget(poolingViews["Max"].GetPivot());
        cameraZoom.ChangeZoomSmooth(4f);
        yield return new WaitForSeconds(4f);

        // NPC speaks message
        cameraZoom.ChangeZoomTarget(NPC.gameObject);
        dialogueBalloon.isCinematic = true;
        ZoomIn();
        string message = "Good job picking the best pooling function for this scenario. This room is repaired, let's get back to the CNN Room.";
        dialogueBalloon.SetSpeaker(NPC.gameObject);
        dialogueBalloon.SetMessage(message);
        dialogueBalloon.PlaceUpperLeft();
        dialogueBalloon.Show();
        dialogueBalloon.OnDone += GameOver;
    }

    bool IsGameOver()
    {
        return IsMaxDone() && !AreOtherPoolingsConnected();
    }

    bool AreOtherPoolingsConnected()
    {
        bool otherPoolingBoxConnected = false;
        foreach (KeyValuePair<string, PoolingView> entry in poolingViews)
        {
            if (entry.Key.Equals("Max"))
            {
                continue;
            }

            if (entry.Value.HasPoolingBox())
            {
                otherPoolingBoxConnected = true;
                break;
            }
        }
        return otherPoolingBoxConnected;
    }

    bool IsMaxDone()
    {
        bool reluDone = poolingViews["Max"].HasPoolingBox() && !poolingViews["Max"].IsApplyingPooling();
        return reluDone;
    }

    bool NeedRemoveWrongPooling()
    {
        return IsMaxDone() && AreOtherPoolingsConnected();
    }

    void ZoomIn()
    {
        cameraZoom.ChangeZoomSmooth(1.4f);
    }

    private void DisplayWrongPoolingMessage()
    {
        string message = "Oops, I just need to connect the best pooling function.";
        timedDialogueBalloon.SetSpeaker(Player.gameObject);
        timedDialogueBalloon.SetMessage(message);
        timedDialogueBalloon.PlaceUpperLeft();
        timedDialogueBalloon.Show(7f);

        // NPC speaks message
        string robotMessage = "Oops, you need to connect only the best pooling function.";
        dialogueBalloon.SetSpeaker(NPC.gameObject);
        dialogueBalloon.SetMessage(robotMessage);
        dialogueBalloon.PlaceUpperLeft();
        dialogueBalloon.Show();
        dialogueBalloon.OnDone += dialogueBalloon.Hide;
    }

    IEnumerator OnWrongPooling()
    {
        yield return new WaitForSeconds(6.5f); // time to read the activation function message
        DisplayWrongPoolingMessage();
        NPC.OnHover += DisplayWrongPoolingMessage;
    }

    protected override void GameOver()
    {
        GameManager.instance.solvedMinigames["Pooling 1"] = true;

        Player.Enable();
        cameraZoom.ChangeZoomTarget(Player.gameObject);

        GameObject.FindGameObjectWithTag("Wormhole").GetComponent<Exit>().UnlockExit();
    }
}
