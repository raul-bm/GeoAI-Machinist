using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class OutputMiniGamePlaybackDirector : MonoBehaviour
{
    public PlayableDirector playbackRobotWalk;
    public PlayerController Player;
    public NonPlayerCharacter NPC;

    public GameObject inputScreen;
    public DialogueBalloon dialogueBalloon;
    public CameraZoom cameraZoom;
    List<(string, string)> screenplay = new List<(string, string)>();
    int currentLineIndex = 0;

    public void StartAnimation()
    {
        InitializeScreenplay();
        Init();
    }

    void OnPlayableDirectorStopped(PlayableDirector aDirector)
    {
        NextLine();
    }

    void InitializeScreenplay()
    {
        screenplay = new List<(string, string)>() {
        new("NPC", "This room is the Output Layer of the CNN."),
        new("NPC", "At this point, each layer has extracted and highlighted features in the image."),
        new("NPC", "Follow me to see how the image looks now."), // instruction
        new("action", "action1"), // Robot Walk
        new("action", "action2"), // Wait Player
        new("action", "action3"), // Show feature map
        new("NPC", "We cannot interpret the extracted features, and we don't have the classification result yet."),
        new("NPC", "Find and activate the Flattenning Pull Lever to flatten the matrix representing the image."),
        // new("NPC", "By flattening, we prepare the image for the final step, involving a fully-connected layer."),
        new("NPC", "Flattening prepares the image for a layer where every pixel connects to decision nodes."),
        new("NPC", "This step ensures all pixel information contributes to the final decision-making process in the classification."),
        new("action", "action4"), // Wait player flatten
        new("NPC", "Did you see how each pixel connects to each decision node?"),
        // new("NPC", "Each pixel connects to each class node, which means we have a fully-connected layer in this final step."),
        // new("action", "action4"), // Layer flattened, class nodes animation done
        // new("NPC", "The connections displays whether the pixel has a positive, negative or small weight  in the classification result."), // instruction
        new("NPC", "The connections show whether each pixel has a positive, negative, or negligible weight in determining the classification result."),
        // new("NPC", "Approach a class node to highlight the weights."),
        new("NPC", "The classification is almost done. We have class nodes with different values. Just one more step to go..."),
        new("NPC", "We need a softmax activation function to calculate the probability of the image belonging to each class."),
        };
    }

    void Init()
    {
        Player.Disable();
        ZoomIn();
        dialogueBalloon.Hide();

        NextLine();
    }

    public void NextLine()
    {
        ClearCallbacks();

        if (screenplay.Count <= currentLineIndex)
        {
            End();
            return;
        }

        var line = screenplay[currentLineIndex];
        // Debug.Log("Current line: " + line.Item1 + " - " + line.Item2);
        switch (line.Item1)
        {
            case "action":
                ExecuteAction(line.Item2);
                break;
            case "NPC":
                dialogueBalloon.SetSpeaker(NPC.gameObject);
                dialogueBalloon.PlaceUpperLeft();
                if (HasSpeakerChanged())
                {
                    cameraZoom.ChangeZoomTarget(NPC.gameObject);
                }
                dialogueBalloon.SetMessage(line.Item2);
                dialogueBalloon.Show();
                dialogueBalloon.OnDone += NextLine;
                break;
        }

        currentLineIndex++;
    }

    private bool HasSpeakerChanged()
    {
        if (currentLineIndex < 1) return true;
        return !screenplay[currentLineIndex].Item1.Equals(screenplay[currentLineIndex - 1].Item1);
    }

    void ExecuteAction(string actionId)
    {
        switch (actionId)
        {
            case "action1":
                WalkToSample();
                break;
            case "action2":
                WaitPlayer();
                break;
            case "action3":
                StartCoroutine(ShowFeatureMap());
                break;
            case "action4":
                WaitPlayerFlatten();
                break;
            default:
                // Do nothing
                break;
        }
    }

    void WalkToSample()
    {
        playbackRobotWalk.Play();
        playbackRobotWalk.stopped += OnPlayableDirectorStopped;
        dialogueBalloon.Hide();
    }

    void WaitPlayer()
    {
        dialogueBalloon.Hide();
        ZoomOut();
        Player.Enable();
        cameraZoom.ChangeZoomTarget(Player.gameObject);
        NPC.OnHover += NextLine;
    }

    IEnumerator ShowFeatureMap()
    {
        Player.Disable();
        ZoomIn();
        cameraZoom.ChangeZoomTarget(inputScreen);
        yield return new WaitForSeconds(2.5f);
        Player.Enable();

        cameraZoom.ChangeZoomTarget(NPC.gameObject);
        NextLine();
    }

    void WaitPlayerFlatten()
    {
        dialogueBalloon.Hide();
        ZoomOut();
        cameraZoom.ChangeZoomTarget(Player.gameObject);
        NPC.OnHover += DisplayFlattenInstruction;
        GameObject selectorObject = GameObject.Find("SelectorSwitch");
        SelectorSwitch selectorSwitch = selectorObject.GetComponent<SelectorSwitch>();
        selectorSwitch.Enable();
    }

    void DisplayFlattenInstruction()
    {
        // NPC speaks message
        string message = "Activate the Flattenning Pull Lever to flatten the image and prepare it for the classification.";
        dialogueBalloon.SetSpeaker(NPC.gameObject);
        dialogueBalloon.SetMessage(message);
        dialogueBalloon.PlaceUpperLeft();
        dialogueBalloon.Show();
        dialogueBalloon.OnDone += dialogueBalloon.Hide;
    }

    void DisplaySoftmaxInstruction()
    {
        // NPC speaks message
        string message = "We need a softmax activation function to calculate the probability of the image belonging to each class.";
        dialogueBalloon.SetSpeaker(NPC.gameObject);
        dialogueBalloon.SetMessage(message);
        dialogueBalloon.PlaceUpperLeft();
        dialogueBalloon.Show();
        dialogueBalloon.OnDone += dialogueBalloon.Hide;
    }

    void HintSoftmax()
    {
        ZoomOut();
        GameObject softmaxObject = GameObject.Find("ActivationBox");
        ActivationBox activationBox = softmaxObject.GetComponent<ActivationBox>();
        activationBox.Blink();
        activationBox.Release();
    }

    void ZoomIn()
    {
        cameraZoom.ZoomIn();
    }

    void ZoomOut()
    {
        cameraZoom.ZoomOut();
    }

    void End()
    {
        NPC.OnHover -= DisplayFlattenInstruction;
        dialogueBalloon.Hide();
        ClearCallbacks();

        cameraZoom.ChangeZoomTarget(Player.gameObject);
        ZoomOut();

        Player.Enable();
        HintSoftmax();
        NPC.OnHover += DisplaySoftmaxInstruction;
    }

    void ClearCallbacks()
    {
        dialogueBalloon.OnDone -= NextLine;
        NPC.OnHover -= NextLine;
    }

    void OnDisable()
    {
        dialogueBalloon.OnDone -= NextLine;
        NPC.OnHover -= DisplaySoftmaxInstruction;
    }
}
