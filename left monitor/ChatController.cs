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

    private Coroutine gameCoroutine;
    private bool gameStarted = false;
    private string searchTag = "";
    private bool paused = true;
    private bool storyComplete = false;

    private string[] script;
    private List<GameObject> messages = new List<GameObject>();
    private Dictionary<string, bool> playerChoices = new Dictionary<string, bool>();

    [Header("Chat File")]

    public string chatFile = "ChatScript.txt";

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
        Debug.Log("▶️ Starting chat interaction.");
        if (!gameStarted) {
            gameStarted = true;
            paused = false;
        }
    }

    IEnumerator LoadScript()
    {
        Debug.Log("🔄 Loading script from file...");
        bool isChoiceSection = false;
        bool printingText = true;
        int seeking = 0;

        foreach (string line in script)
        {
            while (paused)
            {
                yield return null;
            }

            string trimmed = line.Trim();

			Debug.Log($"📜 Processing line: {trimmed}");
            if (trimmed == "[END]")
            {
                Debug.Log("🏁 Story complete. Ending chat.");
                storyComplete = true;
                HideChoices();
                break;
            }
            else if (string.IsNullOrWhiteSpace(trimmed))
            {
                printingText = false;
            }
            else if (printingText)
            {
                // normal message
                yield return StartCoroutine(DisplayMessage(line));
            }
            else if (isChoiceSection)
            {
                // choice selection
                isChoiceSection = false;
                string[] parts = trimmed.Split('|');
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
            else if (!trimmed.StartsWith("["))
            {
                continue;
            }
            else if (searchTag != "")
            {
                Debug.Log($"🔍 Searching for tag: {searchTag}");

                string[] possibilities = trimmed.Trim('[', ']')
                                                .Split('|')
                                                .Select(possibility => possibility.Trim())
                                                .ToArray();;
                Debug.Log($"🔎 Possible tags: {string.Join(", ", possibilities)}");
                Debug.Log(possibilities.Contains(searchTag));
                if (possibilities.Contains(searchTag))
                {
                    Debug.Log($"✅ Found matching tag: {searchTag}");
                    searchTag = "";
                    printingText = true;
                }
            }
            else if (trimmed == "[CHOICE]")
            {
                isChoiceSection = true;
                seeking++;
                continue;
            }
            else if (seeking > 0)
            {
                if (trimmed == "[MERGE]")
                {
                    Debug.Log("🔄 Merging sections, resuming chat.");
                    seeking--;
                    printingText = true;
                    if (seeking < 0)
                    {
                        Debug.LogError("🚨 Extraneous Merge.");
                        break;
                        yield return null;
                    }
                }
                continue;
            }
        }

        if (searchTag != "")
        {
            Debug.Log($"🚨 No matching content for: {searchTag}");
        }
        if (seeking > 0)
        {
            Debug.LogError("🚨 Unmatched Merge tags found in script.");
        }
        yield return null;
    }

    IEnumerator DisplayMessage(string message)
    {
        Debug.Log("▶️ Starting DisplayMessages() coroutine.");
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

        foreach (char letter in message)
        {
            textComponent.text += letter;
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

        Debug.Log("🗑️ Clearing screen.");
    }

    void ShowChoices(List<(string text, string tag)> choices)
    {
        Debug.Log($"🔵 {choices.Count} Choices Available: {string.Join(", ", choices)}");

        // **Choice Layout Fix**
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
        Debug.Log($"🎯 Choice Selected: {choiceTag}");

        playerChoices[choiceTag] = true;
        // ClearScreen();
        searchTag = choiceTag;
        paused = false;

        Debug.Log($"🔄 Loading next section of chat for choice: {choiceTag}");
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
