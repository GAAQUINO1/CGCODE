using UnityEngine;
using UnityEngine.UI;

public class TabSwitcher : MonoBehaviour
{
    public Image websiteImage;
    public Sprite[] websiteSprites; // Different images for each site
    public Button[] tabButtons;
    public PostExpander postExpander; // Reference to the PostExpander script

    private int activeTabIndex = 0; // Tracks the currently active tab

    void Start()
    {
        for (int i = 0; i < tabButtons.Length; i++)
        {
            int index = i;
            tabButtons[i].onClick.AddListener(() => ShowTab(index));
        }

        ShowTab(0); // Default to the first site
    }

    public void ShowTab(int index)
    {
        if (index < websiteSprites.Length)
        {
            websiteImage.sprite = websiteSprites[index]; // Change the website image
            activeTabIndex = index; // Store the new active tab
        }

        if (postExpander != null)
        {
            postExpander.ShowSite(index); // Tell PostExpander which site is active
        }
    }

    public int GetActiveTab()
    {
        return activeTabIndex; // Return the currently active tab
    }
}
