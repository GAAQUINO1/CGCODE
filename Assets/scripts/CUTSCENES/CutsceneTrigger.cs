using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class CutsceneTrigger : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public CanvasGroup cutsceneCanvas;
    public GameObject cutsceneRoot; // parent object of the video player canvas
    public GameObject gameplayRoot; // the UI or objects to hide during cutscene
    public string videoFileName = "myCutscene"; // name of your video file in Resources/Cutscenes (no .mp4)

    private bool cutscenePlayed = false;

    void Start()
    {
        ChatController chat = FindObjectOfType<ChatController>();
        if (chat != null)
        {
            chat.OnStoryComplete += TriggerCutscene;
        }
    }

    public void TriggerCutscene()
    {
        if (cutscenePlayed) return;
        cutscenePlayed = true;

        gameplayRoot.SetActive(false);
        cutsceneRoot.SetActive(true);

        string path = System.IO.Path.Combine(Application.streamingAssetsPath, "CUTSCENES/" + videoFileName + ".mp4");
        Debug.Log("🎬 Loading video from: " + path);

        videoPlayer.url = path;
        videoPlayer.loopPointReached += OnVideoEnd;
        videoPlayer.Play();

        cutsceneCanvas.alpha = 1f;
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        cutsceneCanvas.alpha = 0f;
        cutsceneRoot.SetActive(false);
        gameplayRoot.SetActive(true);

        // OPTIONAL: reset game or load menu
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
