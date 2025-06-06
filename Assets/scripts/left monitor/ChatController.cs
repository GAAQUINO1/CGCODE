using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class ChatController : MonoBehaviour
{
    public GameObject chatMessagePrefab;
    public Transform chatContent;
    public List<Button> choiceButtons;
    public float messageDelay = 1f;
    public float letterDelay = 0.05f;
    public float choiceDelay = 0.5f;

    private List<(int siteIndex, int scenarioNumber)> pendingUnlocks = new List<(int, int)>();

    public event System.Action OnStoryComplete;

    private Coroutine gameCoroutine;
    private bool gameStarted = false;
    private string searchTag = "";
    private bool paused = true;
    private bool storyComplete = false;

    // [siteIndex, scenarioNumber] => has the player seen this scenario post?
    private bool[,] scenarioSeen = new bool[3, 3]; // [site count, max scenario count]


    public ScrollRect chatScrollRect;

    private string[] script;
    private List<GameObject> messages = new List<GameObject>();
    private Dictionary<string, bool> playerChoices = new Dictionary<string, bool>();

    [Header("Chat File")]
    public string chatFile = "ChatScript.txt";

    [Header("Audio Settings")]
    public bool enableVoiceAudio = true;


    [Header("Speaker Colors (Set in Inspector)")]
    public Color alfieColor = Color.yellow;
    public Color cloudGirlColor = Color.cyan;
    public Color systemColor = Color.green;
    public Color defaultColor = Color.white;

    [Header("Scenario Story Controls")]
    private Dictionary<(int site, int scenario), ScenarioStory> scenarioLookup = new();
    public SmartPostButtonController[] siteControllers;

    private AudioController audioController;
    private Dictionary<string, int> audioKeys = new();

    void Start()
    {
        // personalized audio controller
        audioController = gameObject.AddComponent<AudioController>();

        PlayerPrefs.DeleteAll(); // ⚠️ Only use during testing

        RegisterScenario(new ScenarioStory
        {
            label = "Grocery Sale",
            fileName = "ChatScript_Site0_S1.txt",
            siteIndex = 0,
            scenarioNumber = 1
        });
        RegisterScenario(new ScenarioStory
        {
            label = "Main Laundry",
            fileName = "ChatScript_Site0_S2.txt",
            siteIndex = 0,
            scenarioNumber = 2
        });

        // BLOG (siteIndex = 1)
        //  RegisterScenario(new ScenarioStory
        //  {
        //     label = "Scenario 1 BLOG",
        //     fileName = "ChatScript_Site1_S1.txt",
        //     siteIndex = 1,
        //     scenarioNumber = 1
        // });
        // RegisterScenario(new ScenarioStory
        // {
        //    label = "Scenario 2 BLOG",
        //    fileName = "ChatScript_Site1_S2.txt",
        //     siteIndex = 1,
        //     scenarioNumber = 2
        //   });

        // REDDIT (siteIndex = 2)
        RegisterScenario(new ScenarioStory
        {
            label = "Scenario 1 REDDIT",
            fileName = "ChatScript_Site2_S1.txt",
            siteIndex = 2,
            scenarioNumber = 1
        });
        //  RegisterScenario(new ScenarioStory
        // {
        //      label = "Scenario 2 REDDIT",
        //      fileName = "ChatScript_Site2_S2.txt",
        //      siteIndex = 2,
        //      scenarioNumber = 2
        //   });

        HideChoices();

        string filePath = Path.Combine(Application.streamingAssetsPath, chatFile);
        script = File.ReadAllLines(filePath);

        if (!File.Exists(filePath))
        {
            Debug.LogError($"❌ Chat file not found: {filePath}");
            return;
        }

        Debug.Log("✅ Chat system ready. Waiting for player to interact.");
        gameCoroutine = StartCoroutine(LoadScript());
    }

    public void RegisterScenario(ScenarioStory story)
    {
        var key = (story.siteIndex, story.scenarioNumber);
        if (!scenarioLookup.ContainsKey(key))
            scenarioLookup[key] = story;
    }


    public void StartChat()
    {
        if (!gameStarted)
        {
            gameStarted = true;
            paused = false;
        }
    }

    public void ShowAvailableStories()
    {
        Debug.Log("📢 Showing available scenario stories...");
        // DEBUG — Print PlayerPrefs seen status
        for (int s = 0; s < 3; s++)
        {
            for (int n = 1; n <= 2; n++)
            {
                int val = PlayerPrefs.GetInt($"ScenarioSeen_{s}_{n}", 0);
                Debug.Log($"🧠 ScenarioSeen_{s}_{n} = {val}");
            }
        }

        HideChoices();

        int count = 0;
        bool hasPlayable = false;

        foreach (var story in scenarioLookup.Values)
        {
            int site = story.siteIndex;
            int scenario = story.scenarioNumber;
            int seen = PlayerPrefs.GetInt($"ScenarioSeen_{site}_{scenario}", 0);

            bool alreadyPlayed = story.played;

            // Scenario 1 can appear if seen and not already played
            if (scenario == 1 && seen == 1 && !alreadyPlayed)
            {
                Debug.Log($"✅ S1 ready: {story.label}");
                AddChoiceButton(story, ref count);
                hasPlayable = true;
            }

            // Scenario 2 only appears if:
            // - S1 was played
            // - S2 post was seen
            // - S2 not already played
            else if (scenario == 2)
            {
                int s1Played = scenarioLookup[(site, 1)].played ? 1 : 0;
                int s2Seen = PlayerPrefs.GetInt($"ScenarioSeen_{site}_2", 0);

                if (s1Played == 1 && s2Seen == 1 && !alreadyPlayed)
                {
                    Debug.Log($"✅ S2 ready: {story.label}");
                    AddChoiceButton(story, ref count);
                    hasPlayable = true;
                }
                else
                {
                    Debug.Log($"❌ Skipping S2 for {story.label} — s1Played={s1Played}, s2Seen={s2Seen}, played={alreadyPlayed}");
                }
            }
        }

        if (!hasPlayable)
        {
            Debug.Log("🔒 No available scenarios to play. Hiding all buttons.");
            HideChoices();
        }
    }

    void AddChoiceButton(ScenarioStory story, ref int count)
    {
        if (story.played || count >= choiceButtons.Count) return;

        var button = choiceButtons[count];
        button.gameObject.SetActive(true);
        Debug.Log($"🔘 Button {count} now assigned to: {story.label}");
        button.onClick.RemoveAllListeners(); // 🔧 CLEAR OLD LISTENERS HERE
        button.GetComponentInChildren<TextMeshProUGUI>().text = story.label;

        string file = story.fileName;
        int site = story.siteIndex;
        int scenarioNum = story.scenarioNumber;

        button.onClick.AddListener(() =>
        {
            story.played = true;
            HideChoices();
            StartCoroutine(PlayScenarioChat(file, site, scenarioNum));
        });

        count++;
    }

    IEnumerator PlayScenarioChat(string fileName, int siteIndex, int scenarioNum)
    {
        Debug.Log($"📖 Playing chat file: {fileName} from site {siteIndex}, scenario {scenarioNum}");

        string path = Path.Combine(Application.streamingAssetsPath, fileName);
        if (!File.Exists(path))
        {
            Debug.LogError($"❌ Missing scenario chat file: {path}");
            yield break;
        }

        string[] lines = File.ReadAllLines(path);

        bool isChoiceSection = false;
        bool printingText = true;
        int seeking = 0;
        CutsceneManager cutsceneManager = FindObjectOfType<CutsceneManager>();

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith("//")) continue;
            while (paused) yield return null;

            Debug.Log($"📜 Scenario Line: {line}");

            // Handle cutscene
            if (line.StartsWith("[CUTSCENE:"))
            {
                string cutsceneName = line.Substring(10, line.Length - 11);
                Debug.Log($"🎬 Triggering cutscene: {cutsceneName}");

                paused = true;
                bool cutsceneFinished = false;

                cutsceneManager.PlayCutscene(cutsceneName, () =>
                {
                    cutsceneFinished = true;
                    paused = false;
                });

                while (!cutsceneFinished)
                    yield return null;

                continue;
            }

            // Handle END
            if (line == "[END]")
            {
                storyComplete = true;
                OnStoryComplete?.Invoke();
                HideChoices();
                PlayerPrefs.SetInt("IntroComplete", 1);
                PlayerPrefs.Save();

                foreach (var unlock in pendingUnlocks)
                {
                    ApplyScenarioUnlock(unlock.siteIndex, unlock.scenarioNumber);
                }
                pendingUnlocks.Clear();

                break;
            }

            // Handle CHOICE section
            if (line == "[CHOICE]")
            {
                isChoiceSection = true;
                seeking++;
                HideChoices();
                continue;
            }

            if (isChoiceSection && !line.StartsWith("[") && line.Contains("|"))
            {
                List<(string text, string tag)> choices = new();
                foreach (string part in line.Split('|'))
                {
                    string clean = part.Trim();
                    string tag = clean.ToUpper().Replace(" ", "_");
                    choices.Add((clean, tag));
                }

                ShowChoices(choices);
                paused = true;
                while (paused) yield return null;
                isChoiceSection = false;
                printingText = false;
                continue;
            }

            // Skip lines unless tag matches
            // Lock into branch mode until [MERGE]
            if (!string.IsNullOrEmpty(searchTag))
            {
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    string[] tags = line.Trim('[', ']').Split('|');
                    if (tags.Contains(searchTag))
                    {
                        printingText = true;
                        searchTag = ""; // Clear tag so we don't match again
                        continue; // Begin printing this branch
                    }
                    else
                    {
                        // Different tag while still searching — skip
                        continue;
                    }
                }

                // Still skipping (either not a tag or not matching)
                continue;
            }

            // Once in a branch, skip any new tag headers
            if (printingText && line.StartsWith("[") && !line.Equals("[MERGE]"))
            {
                continue;
            }


            // After searchTag is cleared, rely solely on `printingText` to control visibility
            if (!printingText)
            {
                if (line == "[MERGE]")
                {
                    seeking--;
                    if (seeking <= 0)
                    {
                        printingText = true;
                        searchTag = "";
                    }
                }

                continue; // still skipping until merge
            }
            if (!printingText && !line.StartsWith("["))
            {
                continue;
            }

            // End of a branch
            if (line == "[MERGE]")
            {
                seeking--;
                if (seeking <= 0)
                {
                    printingText = true; // ✅ THIS IS THE REAL FIX
                    searchTag = "";
                }
                continue;
            }

            // Show dialogue if printingText is true
            if (printingText)
            {
                yield return StartCoroutine(DisplayMessage(line));
            }
        }

        Debug.Log($"✅ Finished scenario chat: {fileName}");

        if (siteIndex < siteControllers.Length)
        {
            if (scenarioNum == 1)
                siteControllers[siteIndex].LoadScenario(2);
            else if (scenarioNum == 2)
                siteControllers[siteIndex].LoadEndPosts();
        }

        ShowAvailableStories();
    }


    private string GetSpeakerColorHex(string speaker)
    {
        speaker = speaker.ToUpper();

        if (speaker == "ALFIE")
            return ColorUtility.ToHtmlStringRGB(alfieColor);
        if (speaker == "CLOUD GIRL")
            return ColorUtility.ToHtmlStringRGB(cloudGirlColor);
        if (speaker == "SYSTEM")
            return ColorUtility.ToHtmlStringRGB(systemColor);

        return ColorUtility.ToHtmlStringRGB(defaultColor);
    }

    IEnumerator LoadScript()
    {
        Debug.Log("🔄 Loading script from file...");
        bool isChoiceSection = false;
        bool printingText = true;
        int seeking = 0;

        CutsceneTrigger cutsceneTrigger = FindObjectOfType<CutsceneTrigger>();

        for (int i = 0; i < script.Length; i++)
        {
            string line = script[i].Trim();
            while (paused) yield return null;

            Debug.Log($"📜 Processing line: {line}");

            if (!line.StartsWith("[") && line.Contains(":"))
            {
                // keep audio indexing per line passed
                string[] parts = line.Split(new char[] { ':' }, 2);
                string speaker = parts[0].Trim();

                if (audioKeys.ContainsKey(speaker))
                {
                    audioKeys[speaker] += 1;
                }
                else
                {
                    audioKeys[speaker] = 1;
                }
            }

            if (line.StartsWith("[CUTSCENE:"))
            {
                string cutsceneName = line.Substring(10, line.Length - 11);
                paused = true;

                CutsceneManager cutsceneManager = FindObjectOfType<CutsceneManager>();
                bool cutsceneFinished = false;

                cutsceneManager.PlayCutscene(cutsceneName, () =>
                {
                    cutsceneFinished = true;
                    paused = false;
                });

                while (!cutsceneFinished)
                {
                    yield return null;
                }

                printingText = true;
                continue;
            }
            else if (line == "[END]")
            {
                storyComplete = true;
                OnStoryComplete?.Invoke();
                HideChoices();
                // ✅ Handle any queued scenario unlocks
                foreach (var unlock in pendingUnlocks)
                {
                    ApplyScenarioUnlock(unlock.siteIndex, unlock.scenarioNumber);
                }
                pendingUnlocks.Clear();
                break;
            }
            else if (string.IsNullOrWhiteSpace(line))
            {
                printingText = false;
            }
            else if (printingText)
            {
                yield return StartCoroutine(DisplayMessage(line));
            }
            else if (isChoiceSection)
            {
                isChoiceSection = false;
                string[] parts = line.Split('|');
                List<(string text, string tag)> choices = new List<(string, string)>();
                foreach (string part in parts)
                {
                    string cleanText = part.Trim();
                    string tag = cleanText.ToUpper().Replace(" ", "_");
                    choices.Add((cleanText, tag));
                }
                Debug.Log($"📝 Choices Loaded: {string.Join(", ", choices)}");
                yield return new WaitForSeconds(choiceDelay);
                ShowChoices(choices);
                paused = true;
                yield return null;
            }
            else if (!line.StartsWith("["))
            {
                continue;
            }
            else if (searchTag != "")
            {
                string[] possibilities = line.Trim('[', ']')
                    .Split('|')
                    .Select(p => p.Trim())
                    .ToArray();

                if (possibilities.Contains(searchTag))
                {
                    searchTag = "";
                    printingText = true;
                }
                else
                {
                    continue;
                }
            }
            else if (line == "[CHOICE]")
            {
                isChoiceSection = true;
                seeking++;
                continue;
            }
            else if (seeking > 0 && line == "[MERGE]")
            {
                seeking--;
                printingText = true;
                continue;
            }
        }

        yield return null;
    }

    IEnumerator DisplayMessage(string message)
    {

        if (enableVoiceAudio && message.Contains(':'))
        {
            string[] parts = message.Split(new char[] { ':' }, 2);
            string speaker = parts[0].Trim();
            audioController.ReadLine(speaker.ToLower().Replace(" ", ""), audioKeys[speaker]);
        }

        yield return StartCoroutine(TypeMessage(message));

        if (enableVoiceAudio)
            while (!audioController.finished) yield return null;

        yield return new WaitForSeconds(messageDelay);
    }

    IEnumerator TypeMessage(string message)
    {
        GameObject newMessage = Instantiate(chatMessagePrefab, chatContent);
        newMessage.SetActive(true);
        TextMeshProUGUI textComponent = newMessage.GetComponent<TextMeshProUGUI>();
        messages.Add(newMessage);

        string speaker = "";
        string spokenLine = message;
        string colorHex = ColorUtility.ToHtmlStringRGB(defaultColor);

        if (message.Contains(":"))
        {
            string[] parts = message.Split(new char[] { ':' }, 2);
            speaker = parts[0].Trim();
            spokenLine = parts[1].Trim();
            colorHex = GetSpeakerColorHex(speaker);
        }

        string visibleText = string.IsNullOrEmpty(speaker)
            ? spokenLine
            : $"<b>{speaker}:</b> {spokenLine}";

        string openTag = $"<color=#{colorHex}>";
        string closeTag = "</color>";

        ScrollRect scrollRect = GetComponentInChildren<ScrollRect>();

        for (int i = 0; i <= visibleText.Length; i++)
        {
            string partial = visibleText.Substring(0, i);
            textComponent.text = openTag + partial + closeTag;

            Canvas.ForceUpdateCanvases();
            if (scrollRect != null)
                scrollRect.verticalNormalizedPosition = 0f;

            yield return new WaitForSeconds(letterDelay);
        }
    }

    void ShowChoices(List<(string text, string tag)> choices)
    {
        HideChoices(); // 🧹 Clear all buttons first

        int max = Mathf.Min(choiceButtons.Count, choices.Count);
        Debug.Log($"🧪 Showing {max} choices (from {choices.Count} entries)");

        for (int i = 0; i < max; i++)
        {
            var btn = choiceButtons[i];
            btn.gameObject.SetActive(true);

            string displayText = choices[i].text;
            string tagAtThisIndex = choices[i].tag;

            // 🧠 Log exact match
            Debug.Log($"🔗 Assigning Button {i}: \"{displayText}\" → [{tagAtThisIndex}]");

            // Update button text
            TextMeshProUGUI buttonText = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = displayText;
            }
            else
            {
                Debug.LogWarning($"⚠️ Button {i} missing TextMeshProUGUI child!");
            }

            // 🛠️ Capture loop values into local variables
            int capturedIndex = i;
            string capturedText = displayText;
            string capturedTag = tagAtThisIndex;

            // Clean and assign listener safely
            btn.onClick.RemoveAllListeners();
            btn.onClick = new Button.ButtonClickedEvent();
            btn.onClick.AddListener(() =>
            {
                Debug.Log($"🟢 Player clicked button {capturedIndex}: \"{capturedText}\" → Tag [{capturedTag}]");
                SelectChoice(capturedTag);
            });
        }
    }

    void SelectChoice(string choiceTag)
    {
        Debug.Log($"🟢 Player selected choice with tag: {choiceTag}");
        Debug.Log($"✅ Confirmed: Player clicked button with tag [{choiceTag}] at time {Time.time}");

        playerChoices[choiceTag] = true;
        HideChoices();
        searchTag = choiceTag;
        paused = false;
    }

    void HideChoices()
    {
        foreach (Button button in choiceButtons)
        {
            button.gameObject.SetActive(false);
            button.onClick.RemoveAllListeners();
        }
    }

    [System.Serializable]
    public class ScenarioStory
    {
        public string label;
        public string fileName;
        public int siteIndex;
        public int scenarioNumber;
        public bool played = false;
    }

    public void MarkScenarioAsUnlocked(int siteIndex, int scenarioNumber)
    {
        if (!storyComplete)
        {
            Debug.Log($"⏳ Deferring unlock of scenario {scenarioNumber} on site {siteIndex} until intro finishes.");
            pendingUnlocks.Add((siteIndex, scenarioNumber));
            return;
        }

        ApplyScenarioUnlock(siteIndex, scenarioNumber);
    }

    private void ApplyScenarioUnlock(int siteIndex, int scenarioNumber)
    {
        string key = $"ScenarioSeen_{siteIndex}_{scenarioNumber}";
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();

        Debug.Log($"🧠 Scenario marked as seen: {key} = 1");

        ShowAvailableStories();
    }

    [ContextMenu("🔁 Reset All Scenario Progress")]
    void ResetScenarioPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("🔁 All PlayerPrefs reset.");

        foreach (var story in scenarioLookup.Values)
        {
            story.played = false;
        }

        ShowAvailableStories(); // Optional: refresh UI after reset
    }
}