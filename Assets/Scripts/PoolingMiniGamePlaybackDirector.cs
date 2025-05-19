using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class PoolingMiniGamePlaybackDirector : MonoBehaviour
{
    public Action OnEnd;

    // public PlayableDirector introductionAnimation;
    public PlayerController Player;
    public NonPlayerCharacter NPC;
    public DialogueBalloon dialogueBalloon;
    public CameraZoom cameraZoom;
    List<(string, string)> screenplay = new List<(string, string)>();
    int currentLineIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        // introductionAnimation.stopped += OnPlayableDirectorStopped;
        // InitializeScreenplay();
        // Init();
    }

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
        new("NPC", "This room is the Pooling Layer of the CNN. This layer reduces the size of the data while keeps the important features."),
        new("NPC", "Pooling helps the network and computers become faster and more efficient by compressing small regions of the image."),
        new("NPC", "It ensures the CNN can still recognize important features, like roads or rivers."),
        new("NPC", "Place, in the input holder, the best pooling strategy to preserve key features while reducing complexity."),
        };
    }

    void Init()
    {
        Player.Disable();
        ZoomIn();
        dialogueBalloon.Hide();

        NextLine();
    }

    void NextLine()
    {
        dialogueBalloon.isCinematic = true;
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
                dialogueBalloon.PlaceUpperRight();
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
                break;
            case "action2":
                break;
        }
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
        dialogueBalloon.isCinematic = false;
        dialogueBalloon.Hide();
        ClearCallbacks();

        cameraZoom.ChangeZoomTarget(Player.gameObject);
        ZoomOut();

        Player.Enable();
        OnEnd?.Invoke();
    }

    void ClearCallbacks()
    {
        dialogueBalloon.OnDone -= NextLine;
    }

    void OnDisable()
    {
        dialogueBalloon.OnDone -= NextLine;
    }
}
