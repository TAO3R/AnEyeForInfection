using UnityEngine;
using UnityEngine.InputSystem;

public class MenuStamp : MonoBehaviour
{
    [SerializeField] private InputActionReference stampAction;
    [SerializeField] private InputActionReference startAction;
    [SerializeField] private InputActionReference quitAction;

    private bool isStampHeld = false;  // true = lifted and ready
    private bool hasStamped = false;    // blocks multiple stamps per lift

    private void OnEnable()
    {
        stampAction.action.started += OnStampDown;
        stampAction.action.canceled += OnStampUp;
        startAction.action.performed += OnStartStamp;
        quitAction.action.performed += OnQuitStamp;

        stampAction.action.Enable();
        startAction.action.Enable();
        quitAction.action.Enable();
    }

    private void OnDisable()
    {
        stampAction.action.started -= OnStampDown;
        stampAction.action.canceled -= OnStampUp;
        startAction.action.performed -= OnStartStamp;
        quitAction.action.performed -= OnQuitStamp;

        stampAction.action.Disable();
        startAction.action.Disable();
        quitAction.action.Disable();
    }

    private void OnStampDown(InputAction.CallbackContext ctx)
    {
        isStampHeld = false;
        // Don't reset hasStamped here; it resets when lifted
        Debug.Log("Stamp resting — locked");
    }

    private void OnStampUp(InputAction.CallbackContext ctx)
    {
        isStampHeld = true;
        hasStamped = false; // reset per lift
        Debug.Log("Stamp lifted — ready");
    }

    private void OnStartStamp(InputAction.CallbackContext ctx)
    {
        if (!isStampHeld || hasStamped) return;

        hasStamped = true;  // block further stamping this lift
        isStampHeld = false;
        Debug.Log("Stamped: START");

        CameraController cam = FindObjectOfType<CameraController>();
        if (cam != null)
            cam.PlayStartSequence();
    }

    private void OnQuitStamp(InputAction.CallbackContext ctx)
    {
        if (!isStampHeld || hasStamped) return;

        hasStamped = true;  // block further stamping this lift
        isStampHeld = false;
        Debug.Log("Stamped: QUIT");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
