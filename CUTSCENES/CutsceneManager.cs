using UnityEngine;
using System;

public class CutsceneManager : MonoBehaviour
{
    public CutsceneTrigger cutsceneTrigger;

    private string currentCutsceneName;
    private Action onCompleteCallback;
    private bool isCutsceneActive = false;

    public void PlayCutscene(string cutsceneName, Action onComplete)
    {
        if (isCutsceneActive) return;

        currentCutsceneName = cutsceneName;
        onCompleteCallback = onComplete;
        isCutsceneActive = true;

        cutsceneTrigger.OnCutsceneEnd = HandleCutsceneComplete;
        cutsceneTrigger.PlayCutscene(cutsceneName);
    }

    private void HandleCutsceneComplete()
    {
        isCutsceneActive = false;
        onCompleteCallback?.Invoke();
    }
}
