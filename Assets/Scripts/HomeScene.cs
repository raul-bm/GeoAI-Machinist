using UnityEngine;

public class HomeScene : MonoBehaviour
{
    private void Start()
    {
        PlayerPrefs.SetFloat("minZoomValue", 1.5f);
        PlayerPrefs.SetFloat("maxZoomValue", 5.7f);
    }

    public void LoadGame()
    {
        // 1 - Sample Scene
        // 2 - Overview Scene
        // 7 - Introduction CutsCene
        UnityEngine.SceneManagement.SceneManager.LoadScene(7);
        // GameManager.instance.StartOverviewScene();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LoadGame();
        }
    }
}
