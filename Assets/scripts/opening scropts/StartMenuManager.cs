using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartMenuManager : MonoBehaviour
{
    [Header("Menu Screens")]
    public GameObject menuScreen_Main;
    public GameObject menuScreen_Controls;
    public GameObject menuScreen_Credits;
    public GameObject MenuScreen_Gallery;

    [Header("Game + UI References")]
    public GameObject backButton;
    public GameObject startupController;
    public GameObject startMenuCanvas;

    [Header("Menu Buttons")]
    public Button startButton;
    public Button controlsButton;
    public Button creditsButton;
    public Button galleryButton;
    public Button backButtonComponent;

    [Header("Gallery UI")]
    public Button leftArrowButton;
    public Button rightArrowButton;
    public Image artDisplay;                // Now using Image instead of RawImage
    public Sprite[] galleryArtImages;       // Now using Sprite instead of Texture
    private int currentImageIndex = 0;

    void Start()
    {
        // Pause the game
        Time.timeScale = 0f;

        // Show only main menu
        menuScreen_Main.SetActive(true);
        menuScreen_Controls.SetActive(false);
        menuScreen_Credits.SetActive(false);
        MenuScreen_Gallery.SetActive(false);
        backButton.SetActive(false);

        // Block gameplay systems
        startupController.SetActive(false);

        // Assign main menu button actions
        startButton.onClick.AddListener(StartGame);
        controlsButton.onClick.AddListener(ShowControls);
        creditsButton.onClick.AddListener(ShowCredits);
        galleryButton.onClick.AddListener(ShowGallery);
        backButtonComponent.onClick.AddListener(ShowMainMenu);

        // Assign gallery arrows
        leftArrowButton.onClick.AddListener(ShowPreviousImage);
        rightArrowButton.onClick.AddListener(ShowNextImage);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (startMenuCanvas.activeSelf)
            {
                startMenuCanvas.SetActive(false);
                Time.timeScale = 1f; // Unpause
            }
            else
            {
                startMenuCanvas.SetActive(true);
                Time.timeScale = 0f; // Pause game
            }
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
        MenuScreen_Gallery.SetActive(false);
        backButton.SetActive(true);
    }

    public void ShowCredits()
    {
        menuScreen_Main.SetActive(false);
        menuScreen_Controls.SetActive(false);
        menuScreen_Credits.SetActive(true);
        MenuScreen_Gallery.SetActive(false);
        backButton.SetActive(true);
    }

    public void ShowMainMenu()
    {
        menuScreen_Main.SetActive(true);
        menuScreen_Controls.SetActive(false);
        menuScreen_Credits.SetActive(false);
        MenuScreen_Gallery.SetActive(false);
        backButton.SetActive(false);
    }

    public void ShowGallery()
    {
        MenuScreen_Gallery.SetActive(true);
        menuScreen_Main.SetActive(false);
        menuScreen_Controls.SetActive(false);
        menuScreen_Credits.SetActive(false);
        backButton.SetActive(true);

        if (galleryArtImages.Length > 0)
        {
            currentImageIndex = 0;
            artDisplay.sprite = galleryArtImages[0];
            artDisplay.preserveAspect = true;
        }
    }

    void ShowPreviousImage()
    {
        if (galleryArtImages.Length == 0) return;

        currentImageIndex--;
        if (currentImageIndex < 0)
            currentImageIndex = galleryArtImages.Length - 1;

        artDisplay.sprite = galleryArtImages[currentImageIndex];
        artDisplay.preserveAspect = true;
    }

    void ShowNextImage()
    {
        if (galleryArtImages.Length == 0) return;

        currentImageIndex++;
        if (currentImageIndex >= galleryArtImages.Length)
            currentImageIndex = 0;

        artDisplay.sprite = galleryArtImages[currentImageIndex];
        artDisplay.preserveAspect = true;
    }
}
