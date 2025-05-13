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

    public event System.Action OnStoryComplete;

    private Coroutine gameCoroutine;
    private bool gameStarted = false;
    private string searchTag = "";
    private bool paused = true;
    private bool storyComplete = false;

    public ScrollRect chatScrollRect;

    private string[] script;
    private List<GameObject> messages = new List<GameObject>();
    private Dictionary<string, bool> playerChoices = new Dictionary<string, bool>();

    private List<string> ScriptQueue = new List<string>();
    private const string SCRIPT_INDICATOR = "!!NEW_SCRIPT!!";

    [Header("Chat File")]
    public string chatFile = "ChatScript.txt";

    void Start()
    {
        HideChoices();
        LoadScriptFile(chatFile);
        gameCoroutine = StartCoroutine(LoadScript());

        // TODO ANGEL LOOK HERE
        AddScriptFile("Next.txt");
    }

    public void StartChat()
    {
        if (!gameStarted)
        {
            gameStarted = true;
            paused = false;
        }
    }

    public void AddScriptFile(string fileName)
    {
        Debug.Log("Adding Script to Queue");
        ScriptQueue.Add(fileName);
        foreach( var x in ScriptQueue) {
            Debug.Log( x.ToString());
        }

    }

    private void LoadScriptFile(string fileName)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);
        script = File.ReadAllLines(filePath);

        if (!File.Exists(filePath))
        {
            Debug.LogError($"❌ Chat file not found: {filePath}");
            return;
        }

        Debug.Log("✅ Chat system ready. Waiting for player to interact.");
    }

    IEnumerator LoadScript()
    {
        Debug.Log("🔄 Loading script from file...");
        bool isChoiceSection = false;
        bool printingText = true;
        int seeking = 0;

        CutsceneTrigger cutsceneTrigger = FindFirstObjectByType<CutsceneTrigger>();

        for (int i = 0; i < script.Length; i++)
        {
            string line = script[i].Trim();
            while (paused) yield return null;

            Debug.Log($"📜 Processing line: {line}");

            if (line.StartsWith("[CUTSCENE:"))
            {
                string cutsceneName = line.Substring(10, line.Length - 11);
                paused = true;

                CutsceneManager cutsceneManager = FindFirstObjectByType<CutsceneManager>();
                bool cutsceneFinished = false;

                cutsceneManager.PlayCutscene(cutsceneName, () =>
                {
                    cutsceneFinished = true;
                    paused = false;
                });

                // Wait here until the cutscene finishes
                while (!cutsceneFinished)
                {
                    yield return null;
                }

                // ✅ Make sure printingText is reset after the cutscene
                printingText = true;

                continue;
            }


            if (line == "[END]")
            {
                storyComplete = true;
                OnStoryComplete?.Invoke();
                HideChoices();
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
                // choice selection
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
                    // skip non-matching blocks safely
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

        // current script has ended
        Debug.Log("Current Script has ended.");
        Debug.Log("Script Queue Length: " + ScriptQueue.Count.ToString());
        if (ScriptQueue.Count > 0)
        {
            List<(string text, string tag)> choices = new List<(string, string)>();
            foreach (string Script in ScriptQueue)
            {
                choices.Add((Script, string.Concat(SCRIPT_INDICATOR, Script)));
                if (choices.Count >= 3)
                {
                    break;
                }
            }
            ShowChoices(choices);
        }

        yield return null;
    }

    IEnumerator DisplayMessage(string message)
    {
        yield return StartCoroutine(TypeMessage(message));
        yield return new WaitForSeconds(messageDelay);
    }


    IEnumerator TypeMessage(string message)
    {
        GameObject newMessage = Instantiate(chatMessagePrefab, chatContent);
        newMessage.SetActive(true);
        TextMeshProUGUI textComponent = newMessage.GetComponent<TextMeshProUGUI>();
        textComponent.text = "";

        messages.Add(newMessage);

        // Get the ScrollRect once at the start
        ScrollRect scrollRect = GetComponentInChildren<ScrollRect>();

        foreach (char letter in message)
        {
            textComponent.text += letter;

            // Force layout update
            Canvas.ForceUpdateCanvases();

            // Scroll to bottom after each character (or you can do this every few characters)
            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 0f;
            }

            yield return new WaitForSeconds(letterDelay);
        }
    }


    void ClearScreen()
    {
        foreach (GameObject msg in messages)
        {
            Destroy(msg);
        }
        messages.Clear();
        HideChoices();
    }

    void ShowChoices(List<(string text, string tag)> choices)
    {
        for (int i = 0; i < 3; i++)
        {
            choiceButtons[i].gameObject.SetActive(i < choices.Count);
        }

        for (int i = 0; i < choices.Count; i++)
        {
            TextMeshProUGUI buttonText = choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = choices[i].text;
            ResizeButton(choiceButtons[i]);
            int index = i;
            choiceButtons[i].onClick.AddListener(() => SelectChoice(choices[index].tag));
        }
    }

    void SelectChoice(string choiceTag)
    {
        if (choiceTag.StartsWith(SCRIPT_INDICATOR))
        // new script
        {
            string script = choiceTag.Substring(SCRIPT_INDICATOR.Length);
            ScriptQueue.Remove(script);
            LoadScriptFile(script);

            StopCoroutine(gameCoroutine);
            gameCoroutine = StartCoroutine(LoadScript());
        }
        else
        // normal choice
        {
            playerChoices[choiceTag] = true;
            HideChoices();

            searchTag = choiceTag;
            paused = false;
        }
    }

    void HideChoices()
    {
        foreach (Button button in choiceButtons)
        {
            button.gameObject.SetActive(false);
            button.onClick.RemoveAllListeners();
        }
    }

    void ResizeButton(Button button)
    {
        TextMeshProUGUI textComponent = button.GetComponentInChildren<TextMeshProUGUI>();
        RectTransform buttonRect = button.GetComponent<RectTransform>();

        float padding = 20f;
        buttonRect.sizeDelta = new Vector2(textComponent.preferredWidth + padding, textComponent.preferredHeight + padding);
    }
}
