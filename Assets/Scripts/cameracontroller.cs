using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private string nextSceneName = "GameScene";

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip whooshClip;
    [SerializeField] private float whooshStartOffset = 0f; // seconds before/after camera starts moving

    [Header("Timing")]
    [SerializeField] private float doorOpenDelay = 1f;
    [SerializeField] private float cameraMoveDuration = 2f;

    [Header("Idle Breathing")]
    [SerializeField] private float breatheVerticalAmplitude = 0.1f;
    [SerializeField] private float breatheHorizontalAmplitude = 0.05f;
    [SerializeField] private float breatheSpeed = 1f;

    private Vector3 initialPosition;
    private bool isMoving = false;

    private void Start()
    {
        if (cameraTransform != null)
            initialPosition = cameraTransform.position;
    }

    private void Update()
    {
        if (!isMoving && cameraTransform != null)
        {
            // Apply idle breathing motion
            float yOffset = Mathf.Sin(Time.time * Mathf.PI * 2f * breatheSpeed) * breatheVerticalAmplitude;
            float xOffset = Mathf.Cos(Time.time * Mathf.PI * 2f * breatheSpeed) * breatheHorizontalAmplitude;
            cameraTransform.position = initialPosition + new Vector3(xOffset, yOffset, 0f);
        }
    }

    public void PlayStartSequence()
    {
        StartCoroutine(StartSequenceRoutine());
    }

    private IEnumerator StartSequenceRoutine()
    {
        if (cameraTransform == null || cameraTarget == null)
            yield break;

        // Stop breathing immediately
        isMoving = true;

        // Capture start position ignoring breathing offsets
        Vector3 startPos = cameraTransform.position;
        Vector3 endPos = cameraTarget.position;

        if (doorAnimator != null)
            doorAnimator.SetTrigger("Open");

        yield return new WaitForSeconds(doorOpenDelay);

        // 🎵 Handle whoosh sound timing
        if (audioSource != null && whooshClip != null)
        {
            if (whooshStartOffset <= 0f)
            {
                // Negative or zero offset: play before/during movement
                audioSource.PlayOneShot(whooshClip);
                if (whooshStartOffset < 0f)
                {
                    // Wait extra before starting movement
                    yield return new WaitForSeconds(-whooshStartOffset);
                }
            }
            else
            {
                // Positive offset: play after movement has already started
                StartCoroutine(PlayWhooshDelayed(whooshStartOffset));
            }
        }

        float elapsed = 0f;
        while (elapsed < cameraMoveDuration)
        {
            float t = elapsed / cameraMoveDuration;

            // Ease-in-out interpolation (smoothstep)
            float easedT = t * t * (3f - 2f * t);

            cameraTransform.position = Vector3.Lerp(startPos, endPos, easedT);

            elapsed += Time.deltaTime;
            yield return null;
        }

        cameraTransform.position = endPos;

        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator PlayWhooshDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (audioSource != null && whooshClip != null)
            audioSource.PlayOneShot(whooshClip);
    }
}
