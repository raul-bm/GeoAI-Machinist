using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuAnimation : MonoBehaviour
{
    [SerializeField] private PauseMenu pauseMenuScript;

    public void PauseMenuDissapeared()
    {
        pauseMenuScript.GameResumed();
    }
}
