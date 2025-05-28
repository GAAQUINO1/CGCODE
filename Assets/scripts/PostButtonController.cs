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

        LoadScenario(1);
    }

    public void LoadEndPosts()
    {
        Debug.Log($"\ud83d\udcc2 Loading END posts for {siteFolderName}");

        string root = Path.Combine(Application.streamingAssetsPath, "SITES", siteFolderName, "END");
        List<string> imagePaths = GetImages(root);

        if (imagePaths.Count == 0)
            Debug.LogWarning("\u26a0\ufe0f No END post images found.");

        AssignPostsToButtons(imagePaths);
    }

    public void LoadScenario(int scenarioNum)
    {
        Debug.Log($"\ud83d\udcc2 Loading Scenario {scenarioNum} for {siteFolderName}");

        string root = Path.Combine(Application.streamingAssetsPath, "SITES", siteFolderName, $"S{scenarioNum}");
        Debug.Log($"\ud83d\udcc1 Scenario root path: {root}");

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

        Debug.Log($"\ud83d\uddbc\ufe0f Scenario images found: {scenarioPaths.Count}");
        Debug.Log($"\ud83c\udfb2 Random images found: {randomPaths.Count}");

        List<string> selected = new();

        string selectedScenario = scenarioPaths.OrderBy(x => Random.value).FirstOrDefault();
        if (selectedScenario != null)

            Debug.Log($"🎯 Selected main scenario post: {selectedScenario}");  // ADD THIS

        selected.Add(selectedScenario);

        if (scenarioNum == 2 && consequencePaths.Count > 0)
        {
            int count = Random.Range(2, Mathf.Min(6, consequencePaths.Count + 1));
            var selectedConsequences = consequencePaths.OrderBy(x => Random.value).Take(count).ToList();

            Debug.Log($"🎯 Selected {selectedConsequences.Count} consequence posts from {consequencePaths.Count} total.");
            foreach (var con in selectedConsequences)
            {
                Debug.Log($"📌 Consequence selected: {Path.GetFileName(con)}");
            }

            selected.AddRange(selectedConsequences);
        }
        else if (scenarioNum == 2)
        {
            Debug.LogWarning("⚠️ Scenario 2: No consequence posts found.");
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
        Debug.Log($"\ud83d\udcc2 Looking for images in: {path}");

        if (!Directory.Exists(path))
        {
            Debug.LogWarning($"\u274c Directory not found: {path}");
            return new List<string>();
        }

        string[] files = Directory.GetFiles(path, "*.png");
        Debug.Log($"\ud83d\uddbc\ufe0f Found {files.Length} PNGs in {path}");

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

                Debug.Log($"🔗 Assigning to button {i}: {imagePath}");  // ADD THIS LINE

                Texture2D tex = LoadTexture(imagePath);

                if (tex == null)
                {
                    Debug.LogError($"\u274c Failed to load texture at: {imagePath}");
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
                    Debug.Log($"\ud83d\udcdd Button {i} label set to: {fileName}");
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

                    if (safePath.Contains("ScenarioPosts") && chatController != null)
                    {
                        Debug.Log($"🧪 Path check: {safePath}");

                        // Normalize slashes to avoid Windows/Linux path differences
                        string normalizedPath = safePath.Replace("\\", "/").ToLower();
                        bool isS1 = normalizedPath.Contains("/s1/scenarioposts");
                        bool isS2 = normalizedPath.Contains("/s2/scenarioposts");

                        Debug.Log($"🧪 isS1: {isS1} | isS2: {isS2}");

                        int scenarioNum = isS1 ? 1 : isS2 ? 2 : -1;

                        if (scenarioNum != -1)
                        {
                            Debug.Log($"📌 Scenario post viewed. Telling ChatController: site {siteIndex}, scenario {scenarioNum}");
                            chatController.MarkScenarioAsUnlocked(siteIndex, scenarioNum);
                        }
                        else
                        {
                            Debug.LogWarning($"❗ Unexpected path, did not mark any scenario as unlocked: {safePath}");
                        }
                        Debug.Log($"📌 Scenario post viewed. Telling ChatController: site {siteIndex}, scenario {scenarioNum}");
                        chatController.MarkScenarioAsUnlocked(siteIndex, scenarioNum);
                    }
                });

                Debug.Log($"\u2705 Button {i} assigned to image: {Path.GetFileName(imagePath)}");
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
            });
        }

        Debug.Log($"\ud83d\udccb Total posts assigned to buttons: {imagePaths.Count}");
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
