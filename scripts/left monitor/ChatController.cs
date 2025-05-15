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

    [Header("Chat File")]
    public string chatFile = "ChatScript.txt";

    [Header("Speaker Colors (Set in Inspector)")]
    public Color alfieColor = Color.yellow;
    public Color cloudGirlColor = Color.cyan;
    public Color systemColor = Color.green;
    public Color defaultColor = Color.white;

    void Start()
    {
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

    public void StartChat()
    {
        if (!gameStarted)
        {
            gameStarted = true;
            paused = false;
        }
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
        yield return StartCoroutine(TypeMessage(message));
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

    void ResizeButton(Button button)
    {
        TextMeshProUGUI textComponent = button.GetComponentInChildren<TextMeshProUGUI>();
        RectTransform buttonRect = button.GetComponent<RectTransform>();

        float padding = 20f;
        buttonRect.sizeDelta = new Vector2(textComponent.preferredWidth + padding, textComponent.preferredHeight + padding);
    }
}
