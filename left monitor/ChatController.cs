
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ChatController : MonoBehaviour
{
    public GameObject chatMessagePrefab;
    public Transform chatContent;
    public List<Button> choiceButtons;
    public float messageDelay = 1f;
    public float letterDelay = 0.05f;
    public float choiceDelay = 0.5f;

    private Queue<string> messages = new Queue<string>();
    private Coroutine chatCoroutine;
    private bool waitingForChoice = false;
    private bool storyComplete = false;
    private bool isChatLoaded = false;

    private Dictionary<string, bool> playerChoices = new Dictionary<string, bool>();
    private List<(string text, string tag)> currentChoices = new List<(string, string)>();

    [Header("Chat File")]
    public string chatFile = "ChatScript.txt";

    void Start()
    {
        HideChoices();
        Debug.Log("✅ Chat system ready. Waiting for player to interact.");
    }

    void LoadChatFromFile(string searchTag = "")
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, chatFile);

        if (!File.Exists(filePath))
        {
            Debug.LogError($"❌ Chat file not found: {filePath}");
            return;
        }

        string[] lines = File.ReadAllLines(filePath);
        messages.Clear();
        currentChoices.Clear();

        bool isReading = string.IsNullOrEmpty(searchTag);
        bool isChoiceSection = false;
        bool foundTag = false;

        foreach (string line in lines)
        {
            string trimmed = line.Trim();

            if (trimmed.StartsWith("[IF ") && trimmed.EndsWith("]"))
            {
                isReading = trimmed.Contains(searchTag);
                foundTag = isReading;
                continue;
            }

            if (!isReading) continue;

            if (trimmed == "[CHOICE]")
            {
                isChoiceSection = true;
                continue;
            }

            if (isChoiceSection && trimmed.Contains("|"))
            {
                string[] parts = trimmed.Split('|');
                foreach (string part in parts)
                {
                    string cleanText = part.Trim();
                    string tag = cleanText.ToUpper().Replace(" ", "_");
                    currentChoices.Add((cleanText, tag));
                }

                Debug.Log($"📝 Choices Loaded: {string.Join(", ", currentChoices)}");
                break;
            }

            messages.Enqueue(line);
        }

        if (!foundTag && !string.IsNullOrEmpty(searchTag))
        {
            Debug.LogError($"🚨 No matching [IF {searchTag}] found in ChatScript.txt! The story might break.");
        }

        isChatLoaded = true;
        storyComplete = messages.Count == 0 && currentChoices.Count == 0;
        Debug.Log(storyComplete ? "🏁 Story has ended." : $"🔄 Messages in queue: {messages.Count}");
    }

    public void StartChat()
    {
        if (!isChatLoaded) LoadChatFromFile();

        if (chatCoroutine != null)
        {
            Debug.LogWarning("⚠️ StartChat() called while another chatCoroutine is still running!");
            return;
        }

        if (messages.Count > 0)
        {
            Debug.Log($"🟢 StartChat() called - Messages in queue: {messages.Count}");
            chatCoroutine = StartCoroutine(DisplayMessages());
        }
        else
        {
            Debug.Log("🚫 StartChat() called but no messages in queue.");
        }
    }

    IEnumerator DisplayMessages()
    {
        Debug.Log("▶️ Starting DisplayMessages() coroutine.");

        while (messages.Count > 0)
        {
            string currentMessage = messages.Dequeue();

            if (currentMessage.Trim() == "[CHOICE]")
            {
                Debug.Log("⏸ Found [CHOICE], stopping chat and displaying buttons.");
                ShowChoices();
                yield break;
            }

            Debug.Log($"📩 Displaying message: {currentMessage}");
            yield return StartCoroutine(TypeMessage(currentMessage));
            yield return new WaitForSeconds(messageDelay);
        }

        Debug.Log("⏹️ DisplayMessages() finished. Checking for choices...");

        if (!storyComplete && currentChoices.Count > 0)
        {
            Debug.Log($"⏳ Waiting {choiceDelay} seconds before showing choices.");
            yield return new WaitForSeconds(choiceDelay);
            ShowChoices();
            yield return new WaitUntil(() => !waitingForChoice);
        }
        else
        {
            Debug.Log("🏁 No more choices available. Story has ended.");
            HideChoices();
        }

        chatCoroutine = null;
    }

    IEnumerator TypeMessage(string message)
    {
        GameObject newMessage = Instantiate(chatMessagePrefab, chatContent);
        newMessage.SetActive(true);
        TextMeshProUGUI textComponent = newMessage.GetComponent<TextMeshProUGUI>();
        textComponent.text = "";

        foreach (char letter in message)
        {
            textComponent.text += letter;
            yield return new WaitForSeconds(letterDelay);
        }
    }

    void ShowChoices()
    {
        HideChoices();

        if (currentChoices.Count == 0)
        {
            Debug.LogError("❌ No choices loaded! Check ChatScript.txt formatting.");
            return;
        }

        waitingForChoice = true;
        Debug.Log($"🔵 {currentChoices.Count} Choices Available: {string.Join(", ", currentChoices)}");

        // **Choice Layout Fix**
        for (int i = 0; i < 3; i++)
        {
            choiceButtons[i].gameObject.SetActive(i < currentChoices.Count);
        }

        for (int i = 0; i < currentChoices.Count; i++)
        {
            TextMeshProUGUI buttonText = choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = currentChoices[i].text;
            ResizeButton(choiceButtons[i]);
            int index = i;
            choiceButtons[i].onClick.AddListener(() => SelectChoice(currentChoices[index].tag));
        }
    }

    void SelectChoice(string choiceTag)
    {
        Debug.Log($"🎯 Choice Selected: {choiceTag}");

        playerChoices[choiceTag] = true;
        HideChoices();
        waitingForChoice = false;

        Debug.Log($"🔄 Loading next section of chat for choice: {choiceTag}");
        LoadChatFromFile(choiceTag);

        if (!storyComplete)
        {
            StartChat();
        }
        else
        {
            Debug.Log("🏁 No further content. Ending story.");
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