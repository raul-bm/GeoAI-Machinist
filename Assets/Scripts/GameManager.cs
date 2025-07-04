using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // GameManager is a Singleton
    public static GameManager instance = null;

    public Vector2Int playerPositionOverview = new(1, 1);

    public int playerCoinPoints = 0;
    public Dictionary<string, bool> solvedMinigames = new()
    {
        {"Input", false},
        {"Convolutional 1", false},
        {"Activation 1", false},
        {"Convolutional 2", true},
        {"Activation 2", true},
        {"Pooling 1", false},
        {"Convolutional 3", true},
        {"Activation 3", true},
        {"Convolutional 4", true},
        {"Activation 4", true},
        {"Pooling 2", true},
        {"Output", false}
    };

    public Dictionary<string, bool> blockedDoors = new()
    {
        {"Input", false},
        {"Convolutional 1", false},
        {"Activation 1", false},
        {"Convolutional 2", true},
        {"Activation 2", true},
        {"Pooling 1", false},
        {"Convolutional 3", true},
        {"Activation 3", true},
        {"Convolutional 4", true},
        {"Activation 4", true},
        {"Pooling 2", true},
        {"Output", false}
    };

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        // Debug.Log("Wake Game Manger");
    }

    /*private void Update()
    {
        // Input SOLVED
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha1))
        {
            solvedMinigames["Input"] = true;

            UnityEngine.SceneManagement.SceneManager.LoadScene(14);
            playerPositionOverview = new Vector2Int(3, 8);
        }
        // Convolutional SOLVED
        else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha2))
        {
            solvedMinigames["Convolutional 1"] = true;

            UnityEngine.SceneManagement.SceneManager.LoadScene(11);
            playerPositionOverview = new Vector2Int(6, 8);
        }
        // Activation SOLVED
        else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha3))
        {
            solvedMinigames["Activation 1"] = true;

            UnityEngine.SceneManagement.SceneManager.LoadScene(12);
            playerPositionOverview = new Vector2Int(9, 8);
        }
        // Pooling SOLVED
        else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha4))
        {
            solvedMinigames["Pooling 1"] = true;

            UnityEngine.SceneManagement.SceneManager.LoadScene(13);
            playerPositionOverview = new Vector2Int(6, 5);
        }
        // Data labeling
        else if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(1);
        }
        // Input not solved
        else if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            solvedMinigames["Input"] = false;

            UnityEngine.SceneManagement.SceneManager.LoadScene(5);
            playerPositionOverview = new Vector2Int(3, 8);
        }
        // Convolutional not solved
        else if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            solvedMinigames["Convolutional 1"] = false;

            UnityEngine.SceneManagement.SceneManager.LoadScene(6);
            playerPositionOverview = new Vector2Int(6, 8);
        }
        // Activation not solved
        else if(Input.GetKeyDown(KeyCode.Alpha3))
        {
            solvedMinigames["Activation 1"] = false;

            UnityEngine.SceneManagement.SceneManager.LoadScene(8);
            playerPositionOverview = new Vector2Int(9, 8);
        }
        // Pooling not solved
        else if(Input.GetKeyDown(KeyCode.Alpha4))
        {
            solvedMinigames["Pooling 1"] = false;

            UnityEngine.SceneManagement.SceneManager.LoadScene(10);
            playerPositionOverview = new Vector2Int(6, 5);
        }
        // Output not solved
        else if(Input.GetKeyDown(KeyCode.Alpha5))
        {
            solvedMinigames["Input"] = true;
            solvedMinigames["Convolutional 1"] = true;
            solvedMinigames["Activation 1"] = true;
            solvedMinigames["Pooling 1"] = true;
            solvedMinigames["Output"] = false;

            UnityEngine.SceneManagement.SceneManager.LoadScene(9);
            playerPositionOverview = new Vector2Int(12, 2);
        }
        // Final
        else if(Input.GetKeyDown(KeyCode.P))
        {
            solvedMinigames["Input"] = true;
            solvedMinigames["Convolutional 1"] = true;
            solvedMinigames["Activation 1"] = true;
            solvedMinigames["Pooling 1"] = true;
            solvedMinigames["Output"] = true;

            StartOverviewScene();
        }
        // NOTHING SOLVED
        else if(Input.GetKeyDown(KeyCode.O))
        {
            solvedMinigames["Input"] = false;
            solvedMinigames["Convolutional 1"] = false;
            solvedMinigames["Activation 1"] = false;
            solvedMinigames["Pooling 1"] = false;
            solvedMinigames["Output"] = false;

            StartOverviewScene();
        }
    }*/

    public void ResetSolvedMinigames()
    {
        solvedMinigames = new()
        {
            {"Input", false},
            {"Convolutional 1", false},
            {"Activation 1", false},
            {"Convolutional 2", true},
            {"Activation 2", true},
            {"Pooling 1", false},
            {"Convolutional 3", true},
            {"Activation 3", true},
            {"Convolutional 4", true},
            {"Activation 4", true},
            {"Pooling 2", true},
            {"Output", false}
        };
    }

    public void StartDataLabeling()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    public void StartOverviewScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(2);
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(3, UnityEngine.SceneManagement.LoadSceneMode.Additive);
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(4, UnityEngine.SceneManagement.LoadSceneMode.Additive);
    }

    public void StartMiniGame(string type)
    {
        switch (type)
        {
            case "Input":
                if(!IsSolved(type)) UnityEngine.SceneManagement.SceneManager.LoadScene(5);
                else UnityEngine.SceneManagement.SceneManager.LoadScene(14);
                playerPositionOverview = new Vector2Int(3, 8);
                break;
            case "Convolutional 1":
                if(!IsSolved(type)) UnityEngine.SceneManagement.SceneManager.LoadScene(6);
                else UnityEngine.SceneManagement.SceneManager.LoadScene(11);
                playerPositionOverview = new Vector2Int(6, 8);
                break;
            case "Activation 1":
                if (!IsSolved(type)) UnityEngine.SceneManagement.SceneManager.LoadScene(8);
                else UnityEngine.SceneManagement.SceneManager.LoadScene(12);
                playerPositionOverview = new Vector2Int(9, 8);
                break;
            case "Output":
                if(IsSolved("Input") && IsSolved("Convolutional 1") && IsSolved("Activation 1") && IsSolved("Pooling 1"))
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(9);
                    playerPositionOverview = new Vector2Int(12, 2);
                }
                break;
            case "Pooling 1":
                if(!IsSolved(type)) UnityEngine.SceneManagement.SceneManager.LoadScene(10);
                else UnityEngine.SceneManagement.SceneManager.LoadScene(13);
                playerPositionOverview = new Vector2Int(6, 5);
                break;
            default:
                break;
        }
    }

    public bool IsSolved(string type)
    {
        return solvedMinigames[type];
    }

    public bool IsGameOver()
    {
        bool allSolved = true;
        foreach (KeyValuePair<string, bool> entry in solvedMinigames)
        {
            if (entry.Value == false)
            {
                allSolved = false;
                break;
            }
        }

        return allSolved;
    }

    public void GameOver()
    {
        enabled = false;
    }
}
