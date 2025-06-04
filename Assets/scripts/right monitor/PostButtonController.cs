using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class SmartPostButtonController : MonoBehaviour
{
    [System.Serializable]
    public class PostEntry
    {
        public Button triggerButton;
        public TMP_Text label; // Text on the button
    }

    [Header("Display Target")]
    public RawImage postDisplayImage;

    [Header("Post Button/Image Pairs")]
    public List<PostEntry> postButtons = new List<PostEntry>();

    [Header("Button Group Root")]
    public GameObject buttonGroup;

    [Header("Back Button")]
    public GameObject backButton;

    [Header("Folder & Chat Info")]
    public string siteFolderName = "YikYak";
    public int siteIndex = 0;
    public ChatController chatController;

    private Texture originalTexture;

    void Start()
    {
        if (postDisplayImage != null)
        {
            originalTexture = postDisplayImage.texture;
        }

        LoadScenarioInternal(1);
    }

    private void LoadScenarioInternal(int scenarioNum)
    {
        Debug.Log($"📂 [Preload] Loading Scenario {scenarioNum} for {siteFolderName}");

        string root = Path.Combine(Application.streamingAssetsPath, "SITES", siteFolderName, $"S{scenarioNum}");

        List<string> scenarioPaths = scenarioNum == 1
            ? GetImages(Path.Combine(root, "ScenarioPosts"))
            : GetImages(Path.Combine(root, "ScenarioPosts"));

        List<string> consequencePaths = scenarioNum == 2
            ? GetImages(Path.Combine(root, "ConsequencePosts"))
            : new List<string>();

        List<string> randomPaths = GetImages(Path.Combine(root, "RandomPosts"));

        List<string> selected = new();

        if (scenarioPaths.Count > 0)
        {
            string selectedScenario = scenarioPaths.OrderBy(x => Random.value).First();
            selected.Add(selectedScenario);
        }

        if (scenarioNum == 2 && consequencePaths.Count > 0)
        {
            int count = Random.Range(2, Mathf.Min(6, consequencePaths.Count + 1));
            selected.AddRange(consequencePaths.OrderBy(x => Random.value).Take(count));
        }

        int needed = postButtons.Count - selected.Count;
        if (randomPaths.Count > 0)
        {
            selected.AddRange(randomPaths.OrderBy(x => Random.value).Take(needed));
        }

        AssignPostsToButtons(selected.OrderBy(x => Random.value).ToList());
    }


    public void LoadEndPosts()
    {
        Debug.Log($"📂 Loading END posts for {siteFolderName}");

        string root = Path.Combine(Application.streamingAssetsPath, "SITES", siteFolderName, "END");
        List<string> imagePaths = GetImages(root);

        if (imagePaths.Count == 0)
            Debug.LogWarning("⚠️ No END post images found.");

        AssignPostsToButtons(imagePaths);
    }

    public void LoadScenario(int scenarioNum)
    {
        if (PlayerPrefs.GetInt("IntroComplete", 0) == 0)
        {
            Debug.LogWarning("⛔ Cannot load scenarios before intro is completed.");
            return;
        }

        Debug.Log($"📂 Loading Scenario {scenarioNum} for {siteFolderName}");

        string root = Path.Combine(Application.streamingAssetsPath, "SITES", siteFolderName, $"S{scenarioNum}");
        Debug.Log($"📁 Scenario root path: {root}");

        List<string> scenarioPaths = new();
        List<string> consequencePaths = new();
        List<string> randomPaths = new();

        if (scenarioNum == 1)
        {
            scenarioPaths = GetImages(Path.Combine(root, "ScenarioPosts"));
            randomPaths = GetImages(Path.Combine(root, "RandomPosts"));
        }
        else if (scenarioNum == 2)
        {
            scenarioPaths = GetImages(Path.Combine(root, "ScenarioPosts"));
            consequencePaths = GetImages(Path.Combine(root, "ConsequencePosts"));
            randomPaths = GetImages(Path.Combine(root, "RandomPosts"));
        }

        List<string> selected = new();

        string selectedScenario = scenarioPaths.OrderBy(x => Random.value).FirstOrDefault();
        if (selectedScenario != null)
        {
            Debug.Log($"🎯 Selected main scenario post: {selectedScenario}");
            selected.Add(selectedScenario);
        }

        if (scenarioNum == 2 && consequencePaths.Count > 0)
        {
            int count = Random.Range(2, Mathf.Min(6, consequencePaths.Count + 1));
            var selectedConsequences = consequencePaths.OrderBy(x => Random.value).Take(count).ToList();
            selected.AddRange(selectedConsequences);
        }

        int needed = postButtons.Count - selected.Count;
        if (randomPaths.Count > 0)
        {
            selected.AddRange(randomPaths.OrderBy(x => Random.value).Take(needed));
        }

        selected = selected.OrderBy(x => Random.value).ToList();
        AssignPostsToButtons(selected);
    }

    private List<string> GetImages(string path)
    {
        Debug.Log($"📂 Looking for images in: {path}");


        if (!Directory.Exists(path))
        {
            Debug.LogWarning($"❌ Directory not found: {path}");
            return new List<string>();
        }

        string[] files = Directory.GetFiles(path, "*.png");

        Debug.Log($"🖼️ Found images: {string.Join(", ", files)}");

        return files.ToList();
    }

    private void AssignPostsToButtons(List<string> imagePaths)
    {
        Debug.Log($"🧩 Assigning {imagePaths.Count} images to {postButtons.Count} buttons");

        for (int i = 0; i < postButtons.Count; i++)
        {
            if (i < imagePaths.Count)
            {
                string imagePath = imagePaths[i];
                Texture2D tex = LoadTexture(imagePath);

                if (tex == null)
                {
                    Debug.LogError($"❌ Failed to load texture at: {imagePath}");
                    postButtons[i].triggerButton.gameObject.SetActive(false);
                    continue;
                }

                string fileName = Path.GetFileNameWithoutExtension(imagePath);
                if (postButtons[i].label != null)
                {
                    postButtons[i].label.text = fileName;
                    postButtons[i].label.enableAutoSizing = true;
                    postButtons[i].label.fontSizeMin = 10;
                    postButtons[i].label.fontSizeMax = 24;
                    postButtons[i].label.overflowMode = TextOverflowModes.Ellipsis;
                }

                int safeIndex = i;
                string safePath = imagePath;
                Texture2D safeTex = tex;

                postButtons[safeIndex].triggerButton.gameObject.SetActive(true);
                postButtons[safeIndex].triggerButton.onClick.RemoveAllListeners();
                postButtons[safeIndex].triggerButton.onClick.AddListener(() =>
                {
                    postDisplayImage.texture = safeTex;
                    postDisplayImage.enabled = true;
                    buttonGroup.SetActive(false);
                    backButton.SetActive(true);

                    if (siteFolderName == "YikYak")
                    {
                        postDisplayImage.rectTransform.sizeDelta = new Vector2(183, 180);

                        RectTransform contentRect = postDisplayImage.transform.parent.GetComponent<RectTransform>();
                        if (contentRect != null)
                        {
                            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, 180); // enough to avoid scroll gap
                        }
                    }

                    ScrollRect scrollRect = postDisplayImage.GetComponentInParent<ScrollRect>();
                    if (scrollRect != null)
                    {
                        Canvas.ForceUpdateCanvases();
                        scrollRect.verticalNormalizedPosition = 1f;
                    }

                    if (safePath.Contains("ScenarioPosts") && chatController != null)
                    {
                        string normalizedPath = safePath.Replace("\\", "/").ToLower();
                        bool isS1 = normalizedPath.Contains("/s1/scenarioposts");
                        bool isS2 = normalizedPath.Contains("/s2/scenarioposts");

                        int scenarioNum = isS1 ? 1 : isS2 ? 2 : -1;

                        if (scenarioNum != -1)
                        {
                            chatController.MarkScenarioAsUnlocked(siteIndex, scenarioNum);
                        }
                    }
                });
            }
            else
            {
                postButtons[i].triggerButton.gameObject.SetActive(false);
            }
        }

        if (backButton != null)
        {
            backButton.SetActive(false);
            backButton.GetComponent<Button>().onClick.RemoveAllListeners();
            backButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                postDisplayImage.texture = originalTexture;
                postDisplayImage.enabled = true;
                buttonGroup.SetActive(true);
                backButton.SetActive(false);

                if (siteFolderName == "YikYak")
                {
                    postDisplayImage.rectTransform.sizeDelta = new Vector2(183, 740);
                    RectTransform contentRect = postDisplayImage.transform.parent.GetComponent<RectTransform>();
                    if (contentRect != null)
                    {
                        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, 740);
                    }
                }
            });
        }
    }

    private Texture2D LoadTexture(string filePath)
    {
        if (!File.Exists(filePath)) return null;
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(fileData);
        return tex;
    }

}
