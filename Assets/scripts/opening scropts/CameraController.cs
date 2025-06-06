using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{

    public enum ViewState { Default, LeftMonitor, RightMonitor }

    private ViewState currentView = ViewState.Default;

    public ViewState CurrentView => currentView; // Add this public getter


    public Transform startPosition, defaultPosition, leftMonitorPos, rightMonitorPos;
    public float moveDuration = 2f;
    private bool isInteracting = false;
    private bool allowExit = true;

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

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) // 🔥 Left Arrow or A
        {
            FocusOnMonitor("left");
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) // 🔥 Right Arrow or D
        {
            FocusOnMonitor("right");
        }

        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.W)) // 🔥 Press Control to return to Default
        {
            ReturnToDefault();
        }
    }

    void FocusOnMonitor(string monitor)
    {
        if (isInteracting) return;
        isInteracting = true;

        Transform target = null;

        if (monitor == "left")
        {
            target = leftMonitorPos;
            currentView = ViewState.LeftMonitor; // ✅ Add this

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
            target = rightMonitorPos;
            currentView = ViewState.RightMonitor; // ✅ Add this

            LeanTween.move(gameObject, target.position, 0.5f).setEase(LeanTweenType.easeInOutQuad);
            LeanTween.rotate(gameObject, target.rotation.eulerAngles, 0.5f).setEase(LeanTweenType.easeInOutQuad)
                .setOnComplete(() => isInteracting = false);
        }
    }

    void ReturnToDefault()
    {
        if (!allowExit) return;

        currentView = ViewState.Default; // ✅ Add this

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
