using Min_Max_Slider;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private Animator menuAnimator;
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject pause;
    [SerializeField] private GameObject controls;
    [SerializeField] private GameObject options;
    [SerializeField] private MinMaxSlider zoomSlider;
    [SerializeField] private TextMeshProUGUI minZoomValueText;
    [SerializeField] private TextMeshProUGUI maxZoomValueText;

    private bool isPaused = false;

    void Start()
    {
        menu.SetActive(false);
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) menuAnimator.Play("PauseMenuDissapears"); //GameResumed();
            else GamePaused();
        }
    }

    private void GamePaused()
    {
        LoadZoomValues();

        menu.SetActive(true);
        isPaused = true;
        Time.timeScale = 0;

        menuAnimator.Play("PauseMenuAppears");
    }

    public void GameResumed()
    {
        if (options.activeSelf == true) SaveNewZoomValues();

        menu.SetActive(false);
        pause.SetActive(true);
        options.SetActive(false);
        controls.SetActive(false);
        isPaused = false;
        Time.timeScale = 1;
    }

    // Method for the button "Resume"
    public void ResumeButton()
    {
        menuAnimator.Play("PauseMenuDissapears");  //GameResumed();
    }

    // Method for the button "Controls"
    public void ControlsButton()
    {
        pause.SetActive(false);
        controls.SetActive(true);
    }

    // Method for the button "Options"
    public void OptionsButton()
    {
        pause.SetActive(false);
        options.SetActive(true);
    }

    // Method for the button "Main menu"
    public void MainMenuButton()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("HomeScene");
    }

    // Method for the buttons "Return"
    public void ReturnButton()
    {
        if (options.activeSelf == true) SaveNewZoomValues();

        controls.SetActive(false);
        options.SetActive(false);
        pause.SetActive(true);
    }

    private void LoadZoomValues()
    {
        zoomSlider.minValue = PlayerPrefs.GetFloat("minZoomValue");
        minZoomValueText.text = zoomSlider.minValue.ToString();
        zoomSlider.maxValue = PlayerPrefs.GetFloat("maxZoomValue");
        maxZoomValueText.text = zoomSlider.maxValue.ToString();
    }

    private void SaveNewZoomValues()
    {
        zoomSlider.minValue = Mathf.Round(zoomSlider.minValue * 10.0f) * 0.1f;
        zoomSlider.maxValue = Mathf.Round(zoomSlider.maxValue * 10.0f) * 0.1f;

        PlayerPrefs.SetFloat("minZoomValue", zoomSlider.minValue);
        PlayerPrefs.SetFloat("maxZoomValue", zoomSlider.maxValue);

        // Find the scripts of "camera zoom" to update the new values
        CameraZoom[] cameraZoomScripts = FindObjectsOfType(typeof(CameraZoom)) as CameraZoom[];

        foreach(CameraZoom cameraZoom in cameraZoomScripts)
        {
            cameraZoom.ValuesChanged(zoomSlider.minValue, zoomSlider.maxValue);
        }
    }
}
