using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InputSolvedManager : BaseBoard
{
    public Action OnTurnOver;
    public GameObject[] sampleTiles;
    public GameObject spectralBandTile;
    public GameObject spectralBandContainerTile;
    public GameObject selectorSwitch;
    public GameObject teleportationDeviceTile;
    public TextMeshPro turnCounter;

    public InputMiniGamePlaybackDirector playbackDirector;
    public TimedDialogueBalloon timedDialogueBalloon;
    public DialogueBalloon dialogueBalloon;
    public HintBalloon hintBalloon;
    public CameraZoom cameraZoom;

    private List<string> bandTypes = new List<string> { "red", "green", "blue", "redEdge" };

    TeleportationDevice teleportationDevice;
    Dictionary<string, SpectralBandContainerSolved> containers = new Dictionary<string, SpectralBandContainerSolved>();
    SampleBox sampleBox;

    // Turn-related variables
    private List<Turn> turns = new List<Turn> {
        new(0, "River", new List<string>{"redEdge"}, "Choose ONE spectral band to reveal characteristics of a River and place it in the correct container."),
        new(1, "Highway", new List<string>{"blue"}, "Choose ONE spectral band to analyze a Highway, which is a man-made feature surrounded by vegetation or water."),
        new(2, "Residential", new List<string>{"red", "blue", "redEdge"}, "We need a band combination with THREE spectral bands to analyze a Residential area."),
    };

    private Dictionary<string, string> bandMessages = new Dictionary<string, string>()
    {
        { "red", "The Red band is useful for identifying urban areas, vegetation types, and soils." },
        { "blue", "The Blue band is useful for identifying man-made features and soil and vegetation discrimination." },
        { "redEdge", "The Red Edge spectral band is a good choice. It has high reflectance on vegetation and low reflectance on buildings." },
        { "green", "The green..." }
    };

    int currentTurn = 0;

    // Start is called before the first frame update
    void Start()
    {
        // Assign the sprite to each turn
        turns[0].SetSample(sampleTiles[0]); //River
        turns[1].SetSample(sampleTiles[1]); //Highway
        turns[2].SetSample(sampleTiles[2]); //Residential

        LayoutInputHolder();
        LayoutSample();
        LayoutBandContainers();
    }

    public void ZoomIn()
    {
        cameraZoom.ChangeZoomSmooth(1.2f);
    }

    public void ZoomOut(float zoom = 5f)
    {
        cameraZoom.ChangeZoomSmooth(zoom);
    }

    void LayoutInputHolder()
    {
        GameObject instance = Instantiate(teleportationDeviceTile, new Vector3(2.5f, 6.9f, 0f), Quaternion.identity);
        instance.transform.localScale = new(2f, 2f, 1f);
        teleportationDevice = instance.GetComponent<TeleportationDevice>();
    }

    private void LayoutSample()
    {
        Turn current = turns[currentTurn];
        GameObject instance = Instantiate(current.sample, current.position, Quaternion.identity);
        sampleBox = instance.GetComponent<SampleBox>();
        sampleBox.OnBreak += LayoutGrayscaleBands;

        teleportationDevice.Load(sampleBox, current.instruction);
    }

    private void LayoutGrayscaleBands(string sampleBox, Vector3 position)
    {
        hintBalloon.Hide();
        teleportationDevice.StopBlink();
        ZoomOut();

        GameObject tileChoice = spectralBandTile;

        Vector3 upper = position;
        upper.y++;
        //Instantiate tileChoice at the position returned by RandomPosition with no change in rotation
        GameObject blue = Instantiate(tileChoice, upper, Quaternion.identity);
        SampleSpectralBand scriptBlue = blue.GetComponent<SampleSpectralBand>();
        scriptBlue.LoadSprite(sampleBox + "_Blue");

        Vector3 upperRight = position;
        upperRight.y++;
        upperRight.x++;
        GameObject red = Instantiate(tileChoice, upperRight, Quaternion.identity);
        SampleSpectralBand scriptRed = red.GetComponent<SampleSpectralBand>();
        scriptRed.LoadSprite(sampleBox + "_Red");

        Vector3 right = position;
        right.x++;
        GameObject green = Instantiate(tileChoice, right, Quaternion.identity);
        SampleSpectralBand script = green.GetComponent<SampleSpectralBand>();
        script.LoadSprite(sampleBox + "_Green");

        Vector3 downRight = position;
        downRight.y--;
        downRight.x++;
        GameObject redEdge = Instantiate(tileChoice, downRight, Quaternion.identity);
        SampleSpectralBand scriptRedEdge = redEdge.GetComponent<SampleSpectralBand>();
        scriptRedEdge.LoadSprite(sampleBox + "_RedEdge");
    }

    private void LayoutBandContainers()
    {
        float verticalGap = 2f;
        float verticalOffset = 2f;
        float xPosition = 6.5f;

        for (int i = 0; i < bandTypes.Count; i++)
        {
            float yPosition = verticalOffset + i * verticalGap;
            Vector3 position = new(xPosition, yPosition, 0f);
            GameObject instance = Instantiate(spectralBandContainerTile, position, Quaternion.identity);
            SpectralBandContainerSolved spectralBandContainer = instance.GetComponent<SpectralBandContainerSolved>();
            spectralBandContainer.SetType(bandTypes[i]);
            spectralBandContainer.DrawConnections(inputPosition: new(-3.9f, (float)Math.Ceiling(Height / 2f) - yPosition, 0f));
            spectralBandContainer.OnHover += DisplayMessage;
            spectralBandContainer.OnUnhover += HideMessage;

            containers[bandTypes[i]] = spectralBandContainer;
        }
    }

    private void DisplayMessage(string bandName)
    {
        // Player thinks message
        string message = bandMessages[bandName];
        timedDialogueBalloon.SetSpeaker(Player.gameObject);
        timedDialogueBalloon.SetMessage(message);
        timedDialogueBalloon.PlaceUpperLeft();
        timedDialogueBalloon.Show();
    }

    private void HideMessage(string bandName)
    {
        timedDialogueBalloon.Hide();
    }

    protected override void GameOver()
    {
        cameraZoom.ChangeZoomTarget(Player.gameObject);
        GameManager.instance.solvedMinigames["Input"] = true;
        GameManager.instance.StartOverviewScene();
    }


}
