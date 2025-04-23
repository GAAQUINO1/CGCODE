using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartMenuManager : MonoBehaviour
{
    public GameObject menuScreen_Main;
    public GameObject menuScreen_Controls;
    public GameObject menuScreen_Credits;
    public GameObject backButton;
    public GameObject startupController;  // chair animation, chat system, camera logic
    public GameObject startMenuCanvas;

    public Button startButton;
    public Button controlsButton;
    public Button creditsButton;
    public Button backButtonComponent;

    void Start()
    {
        // Start paused
        Time.timeScale = 0f;

        // Show only main menu
        menuScreen_Main.SetActive(true);
        menuScreen_Controls.SetActive(false);
        menuScreen_Credits.SetActive(false);
        backButton.SetActive(false);

        // Block gameplay
        startupController.SetActive(false);

        // Assign button actions
        startButton.onClick.AddListener(StartGame);
        controlsButton.onClick.AddListener(ShowControls);
        creditsButton.onClick.AddListener(ShowCredits);
        backButtonComponent.onClick.AddListener(ShowMainMenu);
    }

    void Update()
    {
        // Reset scene if R is pressed
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("🔄 Resetting the game...");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        startMenuCanvas.SetActive(false);     // Hide menu UI
        startupController.SetActive(true);    // Start your normal animation/camera/chat
    }

    public void ShowControls()
    {
        menuScreen_Main.SetActive(false);
        menuScreen_Controls.SetActive(true);
        menuScreen_Credits.SetActive(false);
        backButton.SetActive(true);
    }

    public void ShowCredits()
    {
        menuScreen_Main.SetActive(false);
        menuScreen_Controls.SetActive(false);
        menuScreen_Credits.SetActive(true);
        backButton.SetActive(true);
    }

    public void ShowMainMenu()
    {
        menuScreen_Main.SetActive(true);
        menuScreen_Controls.SetActive(false);
        menuScreen_Credits.SetActive(false);
        backButton.SetActive(false);
    }
}
