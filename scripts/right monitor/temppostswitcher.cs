using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PostSwitcher : MonoBehaviour
{
    [System.Serializable]
    public class PostEntry
    {
        public Button triggerButton;
        public Texture postImage;
    }

    [Header("Display Target")]
    public RawImage postDisplayImage;

    [Header("Post Button/Image Pairs")]
    public List<PostEntry> posts = new List<PostEntry>();

    [Header("Button Group Root")]
    public GameObject buttonGroup; // One parent object that contains all your buttons

    [Header("Back Button")]
    public GameObject backButton;

    private Texture originalTexture;

    void Start()
    {
        foreach (PostEntry entry in posts)
        {
            entry.triggerButton.onClick.AddListener(() => ShowPost(entry));
        }

        if (backButton != null)
        {
            backButton.SetActive(false);
            backButton.GetComponent<Button>().onClick.AddListener(BackToPostList);
        }

        if (postDisplayImage != null)
        {
            originalTexture = postDisplayImage.texture;
        }
    }

    void ShowPost(PostEntry entry)
    {
        if (postDisplayImage != null && entry.postImage != null)
        {
            postDisplayImage.texture = entry.postImage;
        }

        if (buttonGroup != null)
            buttonGroup.SetActive(false);

        if (backButton != null)
            backButton.SetActive(true);
    }

    void BackToPostList()
    {
        if (postDisplayImage != null)
        {
            postDisplayImage.texture = originalTexture;
        }

        if (buttonGroup != null)
            buttonGroup.SetActive(true);

        if (backButton != null)
            backButton.SetActive(false);
    }
}
