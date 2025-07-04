using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActivationSolvedManager : BaseBoard
{
    // Pre-fabs
    public GameObject activationViewObject;
    public GameObject loadingScreen;

    // Constants
    readonly int ActivationFunctionAmount = 3;

    // Instances
    Dictionary<string, ActivationViewSolved> activationViews = new Dictionary<string, ActivationViewSolved>();
    public TimedDialogueBalloon timedDialogueBalloon;
    public DialogueBalloon dialogueBalloon;
    public CameraZoom cameraZoom;

    // Data
    public TextAsset dataText;
    ActivationData data;
    [System.Serializable]
    class ActivationData
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
        StartCoroutine(LayoutActivationViews());
    }

    void UpdateProgress(float progress)
    {
        // Debug.Log("Update progress " + progress);
        Image bar = GameObject.Find("ProgressBar").GetComponent<Image>();
        bar.fillAmount = progress;
    }


    IEnumerator LayoutActivationViews()
    {
        float verticalGap = 4.5f;
        float xPosition = 2f;
        float verticalOffset = 2.5f;

        for (int i = 0; i < ActivationFunctionAmount; i++)
        {
            float yPosition = verticalOffset + i * verticalGap;
            Vector3 position = new(xPosition, yPosition, 0f);
            GameObject instanceView = Instantiate(activationViewObject, position, Quaternion.identity);
            ActivationViewSolved script = instanceView.GetComponent<ActivationViewSolved>();
            script.InitActivationBox(GetActivationType(i));
            script.InitInput(UnflatMatrix(data.inputMatrix, 62));
            script.OnActivationStopped += OnActivationStopped;
            activationViews.Add(GetActivationType(i), script);
            UpdateProgress((float)(i + 1) / ActivationFunctionAmount);
            yield return null;
        }

        RegisterActivationViewsMessages();
        loadingScreen.SetActive(false);

        foreach(var script in activationViews)
        {
            script.Value.StartActivation();
        }
    }

    private void UnregisterActivationViewsMessages()
    {
        foreach (KeyValuePair<string, ActivationViewSolved> entry in activationViews)
        {
            entry.Value.OnHover -= DisplayActivationFunctionMessage;
            entry.Value.OnUnhover -= HideActivationFunctionMessage;
        }
    }

    private void RegisterActivationViewsMessages()
    {
        foreach (KeyValuePair<string, ActivationViewSolved> entry in activationViews)
        {
            entry.Value.OnHover += DisplayActivationFunctionMessage;
            entry.Value.OnUnhover += HideActivationFunctionMessage;
        }
    }

    private string GetActivationType(int idx)
    {
        switch (idx)
        {
            case 0:
                return "Linear";
            case 1:
                return "ReLu";
            case 2:
                return "Sigmoid";
        }
        return "";
    }

    void LoadMatrix()
    {
        // Debug.Log(dataText.text);
        data = JsonUtility.FromJson<ActivationData>(dataText.text);

        if (data == null)
        {
            Debug.LogError("Failed to retrieve from JSON");
        }
    }

    void OnActivationStopped(string type)
    {
        // Update outputline
        if (activationViews[type].HasActivationBox())
        {
            if (type == "ReLu")
            {
                activationViews[type].UpdateOutputState("correct");
            }
            else
            {
                activationViews[type].UpdateOutputState("wrong");
            }
        }
        else
        {
            activationViews[type].UpdateOutputState("inactive");
        }
    }

    private void DisplayActivationFunctionMessage(string type)
    {
        string message = "";
        switch (type)
        {
            case "Linear":
                message = "The linear function is f(x) = x. It resembles a straight line and it is simply repeating the values, so it doesn't learn new features.";
                break;
            case "ReLu":
                message = "The ReLu function is f(x) = max(0,x). It is simple and non-linear. In this case, it reveals a new complex feature: the streets footprints.";
                break;
            case "Sigmoid":
                message = "The sigmoid is f(x) = 1 / (1 + exp(-x)). It is non-linear, but it requires more computational power. Besides, the new complex feature is not helpful.";
                break;
        }
        timedDialogueBalloon.SetSpeaker(Player.gameObject);
        timedDialogueBalloon.SetMessage(message);
        timedDialogueBalloon.PlaceUpperLeft();
        timedDialogueBalloon.Show();
    }

    private void HideActivationFunctionMessage(string type)
    {
        timedDialogueBalloon.Hide();
    }

    protected override void GameOver()
    {
        GameManager.instance.solvedMinigames["Activation 1"] = true;

        Player.Enable();
        cameraZoom.ChangeZoomTarget(Player.gameObject);

        GameObject.FindGameObjectWithTag("Wormhole").GetComponent<SpriteRenderer>().color = Color.green;
    }
}
