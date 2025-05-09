using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class OverviewBoardManager : BaseBoard
{
    float startCorridor;
    float endCorridor;

    public TimedDialogueBalloon timedDialogueBalloon; // Thought balloon
    public DialogueBalloon NPCDialogueBalloon;

    public GameObject cnnLayerRoom;

    public GameObject inputHolder;
    public GameObject sampleTile;

    public CameraZoom cameraZoom;

    static bool firstLoad = true;

    // Start is called before the first frame update
    void Start()
    {
        startCorridor = Height * (1f / 3f);
        endCorridor = Height * (2f / 3f);

        m_Tilemap = GetComponentInChildren<Tilemap>();
        m_Wallsmap = GameObject.Find("Walls").GetComponent<Tilemap>();
        m_Grid = GetComponentInChildren<Grid>();

        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                Tile tile;

                if (IsBorder(x, y) && !IsCorridor(x, y))
                {
                    tile = GetWallTile(x, y);
                    m_Wallsmap.SetTile(new Vector3Int(x, y, 1), tile);
                }

                // Also have floor behind the walls
                tile = GroundTiles[Random.Range(0, GroundTiles.Length)];
                m_Tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }

        LayoutCNNLayers();
        LayoutInputHolder();

        if (GameManager.instance.IsGameOver())
        {
            return;
        }

        NPC.Spawn(this, new Vector2Int(0, 8));
        NPC.OnHover += DisplayIntroduction;
        if (firstLoad)
        {
            GameManager.instance.playerPositionOverview = new(1, 8);
            firstLoad = false;
            DisplayIntroduction();
            NPCDialogueBalloon.OnDone += ZoomOutOnFirst;
        }
        else
        {
            ZoomOut();
        }
        Player.Spawn(this, GameManager.instance.playerPositionOverview);
    }

    public void DisplayIntroduction()
    {
        ZoomIn();
        NPCDialogueBalloon.SetSpeaker(NPC.gameObject);
        NPCDialogueBalloon.SetMessage("This room has a Convolutional Neural Network (CNN) to classify land use and cover. A Convolutional Neural Network is a sequence of stages to extract information from images.\nFour layers are damaged—enter their room to fix them!");
        NPCDialogueBalloon.PlaceUpperRight();
        NPCDialogueBalloon.Show();
        NPCDialogueBalloon.OnDone += NPCDialogueBalloon.Hide;
    }

    void ZoomOutOnFirst()
    {
        NPCDialogueBalloon.OnDone -= ZoomOutOnFirst;
        ZoomOut();
    }

    public void FollowPlayer()
    {
        cameraZoom.ChangeZoomTarget(Player.gameObject);
    }

    public void ZoomIn()
    {
        cameraZoom.ZoomIn();
    }

    public void ZoomOut()
    {
        cameraZoom.ZoomOut();
    }

    // TODO: move to Abstract class 
    protected new bool IsBorder(int x, int y)
    {
        return x == 0 || y == 0 || x == Width - 1 || y == Height - 1;
    }

    private bool IsCorridor(int x, int y)
    {
        bool isRight = x == Width - 1;

        float startCorridor = Height * (1f / 3f);
        float endCorridor = Height * (2f / 3f);
        bool isWithinRange = y > startCorridor && y < endCorridor;
        // Debug.Log("isRight " + isRight + ", startCorridor: " + startCorridor + ", endCorridor: " + endCorridor + ", x: " + x + ", y:" + y);
        return isRight && isWithinRange;
    }

    protected new Tile GetWallTile(int x, int y)
    {
        // [TopLeft, Top, TopRight, Left, Right, BottomLeft, Bottom, BottomRight]
        if (x == 0 && y == Height - 1) // TopLeft
        {
            return WallTiles[0];
        }

        if (x == 0 && y == 0) // BottomLeft
        {
            return WallTiles[5];
        }

        if (x == Width - 1 && y == 0) // BottomRight
        {
            return WallTiles[7];
        }

        if (x == Width - 1 && y == Height - 1) // TopRight
        {
            return WallTiles[2];
        }

        if (x == Width - 1 && y == startCorridor) // Corner Bottom Right
        {
            return WallTiles[9];
        }

        if (x == Width - 1 && y == endCorridor) // Corner Top Right
        {
            return WallTiles[8];
        }

        if (x == 0) // Left
        {
            return WallTiles[3];
        }

        if (x == Width - 1) // Right
        {
            return WallTiles[4];
        }

        if (y == 0) // Bottom
        {
            return WallTiles[6];
        }

        if (y == Height - 1) // Top
        {
            return WallTiles[1];
        }

        // Default
        return WallTiles[1];
    }


    void LayoutCNNLayers()
    {
        // // TODO: Read layers from actual CNN model

        List<string> layerNames = new List<string> {
            "Input", "Convolutional 1", "Activation 1", "Convolutional 2",
            "Activation 2", "Pooling 1", "Convolutional 3", "Activation 3",
            "Convolutional 4", "Activation 4", "Pooling 2", "Output" };
        List<Vector3> positions = new List<Vector3> {
            new Vector3(3f, 9f, 0f), new Vector3(6f, 9f, 0f), new Vector3(9f, 9f, 0f), new Vector3(12f, 9f, 0f),
            new Vector3(3f, 6f, 0f), new Vector3(6f, 6f, 0f), new Vector3(9f, 6f, 0f), new Vector3(12f, 6f, 0f),
            new Vector3(3f, 3f, 0f), new Vector3(6f, 3f, 0f), new Vector3(9f, 3f, 0f), new Vector3(12f, 3f, 0f)
        };

        GameObject tileChoice = cnnLayerRoom;

        for (int i = 0; i < 12; i++)
        {
            CNNLayer cnnLayer = tileChoice.GetComponent<CNNLayer>();
            cnnLayer.type = layerNames[i];
            cnnLayer.isEndOfRow = cnnLayer.type.Equals("Convolutional 2") || cnnLayer.type.Equals("Activation 3") || cnnLayer.type.Equals("Output");

            Vector3 fixedPosition = positions[i];

            GameObject instance = Instantiate(tileChoice, fixedPosition, Quaternion.identity);
            instance.GetComponent<CNNLayer>().DrawConnection();
            instance.GetComponent<CNNLayer>().OnHover += DisplayLayerInfo;
            instance.GetComponent<CNNLayer>().OnUnhover += HideLayerInfo;
        }
    }

    void LayoutInputHolder()
    {
        GameObject instance = Instantiate(inputHolder, new Vector3(1f, 9f, 0f), Quaternion.identity);
        InputHolder script = instance.GetComponent<InputHolder>();
        // script.Spawn(this, new Vector2Int(1, 9));
        script.DrawConnection();

        LayoutSample(script);
    }

    void LayoutSample(InputHolder inputHolder)
    {
        GameObject instance = Instantiate(sampleTile, new Vector3(1f, 10f, 0f), Quaternion.identity);
        SampleBox sampleBox = instance.GetComponent<SampleBox>();
        sampleBox.Block();
        inputHolder.FeedInputSample(sampleBox);
    }

    void DisplayLayerInfo(string type)
    {
        Dictionary<string, string> myDict = new()
        {
            {"Input", "This layer receives the raw data (e.g., images) for processing."},
            {"Convolutional 1", "This layer extracts features using filters."},
            {"Activation 1", "This layer applies a non-linear activation function."},
            {"Convolutional 2", "This layer extracts more complex patterns."},
            {"Activation 2", "This layer introduces non-linearity after the second convolutional layer."},
            {"Pooling 1", "This layer reduces image dimensions to require less computational power."},
            {"Convolutional 3", "This layer captures deeper patterns in the input data."},
            {"Activation 3", "This layer applies a non-linear activation function after the third convolutional layer."},
            {"Convolutional 4", "This layer further extracts features."},
            {"Activation 4", "This layer adds non-linearity in deeper layers."},
            {"Pooling 2", "This layer further reduces image dimensions to prepare for output."},
            {"Output", "This layer generates predictions or final classification results."}
        };

        bool solved = GameManager.instance.solvedMinigames[type];
        string message = myDict[type];
        if (!solved)
        {
            message += " It is damaged, so I need to enter to fix it.";
        }
        timedDialogueBalloon.SetSpeaker(Player.gameObject);
        timedDialogueBalloon.SetMessage(message);
        timedDialogueBalloon.PlaceUpperLeft();
        timedDialogueBalloon.Show();
    }

    void HideLayerInfo(string type)
    {
        timedDialogueBalloon.Hide();
    }

    protected override void GameOver()
    {
        // Not needed
    }
}
