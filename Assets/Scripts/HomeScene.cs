using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeScene : MonoBehaviour
{
    [SerializeField] TMP_InputField textNickname;
    [SerializeField] Button buttonStart;

    private void Start()
    {
        PlayerPrefs.SetFloat("minZoomValue", 1.5f);
        PlayerPrefs.SetFloat("maxZoomValue", 5.7f);

        textNickname.onValueChanged.AddListener(OnValueChanged);
    }

    public void LoadGame()
    {
        PlayerPrefs.SetString("nickname", textNickname.text);
        // 1 - Sample Scene
        // 2 - Overview Scene
        // 7 - Introduction CutsCene
        GameManager.instance.ResetSolvedMinigames();
        UnityEngine.SceneManagement.SceneManager.LoadScene(7);
        // GameManager.instance.StartOverviewScene();

    }

    public void OnValueChanged(string value)
    {
        if (value.Length <= 0) buttonStart.interactable = false;
        else buttonStart.interactable = true;
    }
}
