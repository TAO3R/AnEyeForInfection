using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DoorCameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator doorAnimator;    // Animator for the door
    [SerializeField] private Animator cameraAnimator;  // Animator for the camera
    [SerializeField] private string nextSceneName = "GameScene";

    [Header("Timing")]
    [SerializeField] private float doorOpenDelay = 1f;   // wait after triggering door
    [SerializeField] private float cameraMoveDelay = 2f; // wait after triggering camera

    // private void OnEnable()
    // {
    //     if (MenuEvents.Instance != null)
    //     {
    //         MenuEvents.Instance.StartSelected += PlayStartSequence;
    //         MenuEvents.Instance.QuitSelected += QuitGame;
    //     }
    // }
    //
    // private void OnDisable()
    // {
    //     if (MenuEvents.Instance != null)
    //     {
    //         MenuEvents.Instance.StartSelected -= PlayStartSequence;
    //         MenuEvents.Instance.QuitSelected -= QuitGame;
    //     }
    // }

    private void PlayStartSequence()
    {
        StartCoroutine(StartSequenceRoutine());
    }

    private IEnumerator StartSequenceRoutine()
    {
        // Trigger door animation
        if (doorAnimator != null)
            doorAnimator.SetTrigger("Open");

        // Wait for door animation to partially open
        yield return new WaitForSeconds(doorOpenDelay);

        // Trigger camera animation
        if (cameraAnimator != null)
            cameraAnimator.SetTrigger("MoveThrough");

        // Wait for camera animation to finish
        yield return new WaitForSeconds(cameraMoveDelay);

        // Finally load the scene
        SceneManager.LoadScene(nextSceneName);
    }

    private void QuitGame()
    {
        Debug.Log("Quitting game...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
