using UnityEngine;
using UnityEngine.UI;

public class PostExpander : MonoBehaviour
{
    public Image websiteImage;
    public Sprite[] expandedPostImages; // Unique expanded images per site
    public Button[] openPostButtons; // "View Post" buttons for each site
    public Button closePostButton; // Close button for expanded post
    public TabSwitcher tabSwitcher; // Reference to the TabSwitcher

    private Sprite originalImage;
    private int currentSiteIndex = -1; // Tracks the active site before expanding

    void Start()
    {
        originalImage = websiteImage.sprite; // Store original site image

        // Assign each post button its corresponding action
        for (int i = 0; i < openPostButtons.Length; i++)
        {
            int index = i;
            openPostButtons[i].onClick.AddListener(() => OpenPost(index));
        }

        closePostButton.onClick.AddListener(ClosePost);
        closePostButton.gameObject.SetActive(false); // Hide Close button at start
        HideAllPostButtons(); // Ensure only the active site's button is visible
    }

    public void ShowSite(int index)
    {
        currentSiteIndex = index;
        HideAllPostButtons(); // Hide all buttons first

        if (index < openPostButtons.Length)
        {
            openPostButtons[index].gameObject.SetActive(true); // Show only the correct site's button
        }
    }

    void OpenPost(int index)
    {
        if (index < expandedPostImages.Length)
        {
            currentSiteIndex = tabSwitcher.GetActiveTab(); // Store the active site before expanding
            websiteImage.sprite = expandedPostImages[index]; // Show the expanded post
            closePostButton.gameObject.SetActive(true); // Show Close button
            HideAllPostButtons(); // Hide "View Post" buttons when expanded
        }
    }

    void ClosePost()
    {
        websiteImage.sprite = originalImage; // Restore original website
        closePostButton.gameObject.SetActive(false); // Hide Close button
        tabSwitcher.ShowTab(currentSiteIndex); // Go back to the tab the player was on
    }

    void HideAllPostButtons()
    {
        foreach (Button button in openPostButtons)
        {
            button.gameObject.SetActive(false);
        }
    }
}
