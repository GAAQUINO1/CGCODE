using UnityEngine;
using UnityEngine.UI;


public class SiteManager : MonoBehaviour
{
    [Header("Each site's root GameObject")]
    public GameObject[] siteRoots;

    [Header("Images on the tabs themselves")]
    public Image[] tabImages;

    [Header("Custom Sprites Per Tab")]
    public Sprite[] unselectedSprites; // same order as tabImages
    public Sprite[] selectedSprites;   // same order as tabImages

    private int currentIndex = -1;

    public void ShowSite(int index)
    {
        if (index == currentIndex) return;

        // Activate correct site
        for (int i = 0; i < siteRoots.Length; i++)
        {
            siteRoots[i].SetActive(i == index);
        }

        // Update tab visuals
        for (int i = 0; i < tabImages.Length; i++)
        {
            if (i < unselectedSprites.Length && i < selectedSprites.Length)
            {
                tabImages[i].sprite = (i == index) ? selectedSprites[i] : unselectedSprites[i];
            }
        }

        currentIndex = index;
    }
}
