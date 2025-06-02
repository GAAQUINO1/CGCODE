using UnityEngine;
using UnityEngine.Video;
using System;

public class CutsceneTrigger : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public CanvasGroup cutsceneCanvas;
    public GameObject cutsceneRoot;
    public GameObject gameplayRoot;

    public Action OnCutsceneEnd;

    private bool isPlaying = false;
    private CanvasGroup gameplayCanvasGroup;

    void Awake()
    {
        gameplayCanvasGroup = gameplayRoot?.GetComponent<CanvasGroup>();
        if (gameplayRoot != null && gameplayCanvasGroup == null)
        {
            gameplayCanvasGroup = gameplayRoot.AddComponent<CanvasGroup>();
        }
    }

    public void PlayCutscene(string videoName)
    {
        if (isPlaying) return;

        isPlaying = true;
        if (gameplayCanvasGroup != null) gameplayCanvasGroup.alpha = 0f;

        cutsceneRoot.SetActive(true);

        string path = System.IO.Path.Combine(Application.streamingAssetsPath, "CUTSCENES/" + videoName + ".mp4");
        videoPlayer.url = path;
        videoPlayer.loopPointReached += EndCutscene;

        if (cutsceneCanvas != null) cutsceneCanvas.alpha = 1f;
        videoPlayer.Play();
    }

    void Update()
    {
        if (isPlaying && Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("⏩ Cutscene skipped by pressing S");
            EndCutscene(videoPlayer);
        }
    }

    public void EndCutscene(VideoPlayer vp)
    {
        if (!isPlaying) return;

        videoPlayer.Stop();
        videoPlayer.loopPointReached -= EndCutscene;

        if (cutsceneCanvas != null) cutsceneCanvas.alpha = 0f;
        if (cutsceneRoot != null) cutsceneRoot.SetActive(false);
        if (gameplayCanvasGroup != null) gameplayCanvasGroup.alpha = 1f;

        isPlaying = false;

        OnCutsceneEnd?.Invoke();
    }
}
