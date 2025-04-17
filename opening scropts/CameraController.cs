using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public Transform startPosition, defaultPosition, leftMonitorPos, rightMonitorPos;
    public float moveDuration = 2f;
    private bool isInteracting = false;
    private bool allowExit = true;
    private string currentMonitor = "";

    public GameObject leftMonitorUI;
    public GameObject rightMonitorUI;

    void Start()
    {
        transform.position = startPosition.position;
        transform.rotation = startPosition.rotation;

        LeanTween.move(gameObject, defaultPosition.position, moveDuration).setEase(LeanTweenType.easeInOutQuad);
        LeanTween.rotate(gameObject, defaultPosition.rotation.eulerAngles, moveDuration).setEase(LeanTweenType.easeInOutQuad);

        leftMonitorUI.SetActive(true);
        rightMonitorUI.SetActive(true);
    }

    void Update()
    {
        if (!allowExit) return;

        if (Input.GetMouseButtonDown(0)) // Left Click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("LeftMonitor"))
                    FocusOnMonitor("left");

                if (hit.collider.CompareTag("RightMonitor"))
                    FocusOnMonitor("right");
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow)) // 🔥 Press Left Arrow to go to Left Monitor
        {
            FocusOnMonitor("left");
        }

        if (Input.GetKeyDown(KeyCode.RightArrow)) // 🔥 Press Right Arrow to go to Right Monitor
        {
            FocusOnMonitor("right");
        }

        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl)) // 🔥 Press Control to return to Default
        {
            ReturnToDefault();
        }
    }

    void FocusOnMonitor(string monitor)
    {
        if (currentMonitor == monitor) return;
        if (isInteracting) return;
        isInteracting = true;

        Transform target = null;

        if (monitor == "left")
        {
            currentMonitor = "left";
            target = leftMonitorPos;

            // 🔥 Move the camera first, then start chat AFTER the movement finishes
            LeanTween.move(gameObject, target.position, 0.5f).setEase(LeanTweenType.easeInOutQuad);
            LeanTween.rotate(gameObject, target.rotation.eulerAngles, 0.5f).setEase(LeanTweenType.easeInOutQuad)
                .setOnComplete(() =>
                {
                    isInteracting = false;
                    ChatController chatController = FindFirstObjectByType<ChatController>();
                    if (chatController != null)
                    {
                        chatController.StartChat();
                    }
                });
        }
        else if (monitor == "right")
        {
            currentMonitor = "right";
            target = rightMonitorPos;
            LeanTween.move(gameObject, target.position, 0.5f).setEase(LeanTweenType.easeInOutQuad);
            LeanTween.rotate(gameObject, target.rotation.eulerAngles, 0.5f).setEase(LeanTweenType.easeInOutQuad)
                .setOnComplete(() => isInteracting = false);
        }
    }

    void ReturnToDefault()
    {
        if (!allowExit) return;

        currentMonitor = "";
        LeanTween.move(gameObject, defaultPosition.position, 0.5f).setEase(LeanTweenType.easeInOutQuad);
        LeanTween.rotate(gameObject, defaultPosition.rotation.eulerAngles, 0.5f).setEase(LeanTweenType.easeInOutQuad);
    }

    public void AllowExit()
    {
        allowExit = true;
    }

    public void PreventExit()
    {
        allowExit = false;
    }
}
