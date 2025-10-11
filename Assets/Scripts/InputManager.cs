using System.Collections;
using Tutorial;
using UnityEngine;
using UnityEngine.InputSystem;

public enum EyeDropperStates
{
    OnTray,
    PickedUp,
    Moving,
    Dripping
}

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    #region EyeDropper
    private static readonly int Rise = Animator.StringToHash("Rise");
    private static readonly int Drip = Animator.StringToHash("Drip");
    private static readonly int Reset = Animator.StringToHash("Reset");

    [Header("Input Actions")]
    [SerializeField] private InputActionReference prepareAction; // pickup/place button
    [SerializeField] private InputActionReference dripAction;    // drip button
    #endregion

    [Header("Animators")]
    public Animator eyeDropperAnim;
    public Animator pupilAnim;

    [Header("Dropper Settings")]
    [SerializeField] private EyeDropperStates eyeDropperCurrentState;
    public EyeDropperStates EyeDropperCurrentState => eyeDropperCurrentState;
    public GameObject drip;
    public Transform dropperTip;

    [Header("Procedural Anim")] [SerializeField]
    private AnimationClip dropperPickUpClip;
    [SerializeField]
    private GameObject dropper;

    #region Mono

    private void Awake()
    {
        Instance = this;
        eyeDropperCurrentState = EyeDropperStates.OnTray;
    }

    private void OnEnable()
    {
        // Pickup when button released, place when pressed
        prepareAction.action.started += OnPlaceDown;   // button pressed -> place on tray
        prepareAction.action.canceled += OnPickup;    // button released -> pick up

        dripAction.action.started += OnDrip;          // separate drip button

        prepareAction.action.Enable();
        dripAction.action.Enable();
    }

    private void OnDisable()
    {
        prepareAction.action.started -= OnPlaceDown;
        prepareAction.action.canceled -= OnPickup;

        dripAction.action.started -= OnDrip;

        prepareAction.action.Disable();
        dripAction.action.Disable();
    }

    #endregion

    #region Input Action Callbacks

    // Pick up the dropper when button is released and it is on the tray
    private void OnPickup(InputAction.CallbackContext ctx)
    {
        if (eyeDropperCurrentState == EyeDropperStates.OnTray)
        {
            TutorialManager.Instance?.NotifyObserver();
            
            Temp(); // trigger pickup animation
            
            // Invoke a coroutine that uses AnimationClip.SampleAnimation()
            // StartCoroutine(PlayAnimClip(dropper, dropperPickUpClip));
        }
    }

    // Place the dropper back on the tray when button is pressed and it is picked up
    private void OnPlaceDown(InputAction.CallbackContext ctx)
    {
        if (eyeDropperCurrentState == EyeDropperStates.PickedUp)
        {
            eyeDropperAnim.SetTrigger(Reset);
            eyeDropperCurrentState = EyeDropperStates.OnTray;
        }
    }

    // Drop a drip while being picked up
    private void OnDrip(InputAction.CallbackContext ctx)
    {
        if (eyeDropperCurrentState == EyeDropperStates.PickedUp)
        {
            eyeDropperCurrentState = EyeDropperStates.Dripping;
            eyeDropperAnim.SetTrigger("Drip");
            GameObject tempDrip = Instantiate(drip, dropperTip.position, Quaternion.identity);
            tempDrip.SetActive(true);
        }
    }

    #endregion

    #region Animation Events

    public void SetDroppingMoving() => eyeDropperCurrentState = EyeDropperStates.Moving;
    public void SetDropperPickedUp() => eyeDropperCurrentState = EyeDropperStates.PickedUp;
    public void SetDropperOnTray() => eyeDropperCurrentState = EyeDropperStates.OnTray;

    // Trigger rise animation
    public void Temp() => eyeDropperAnim.SetTrigger(Rise);

    #endregion

    private IEnumerator PlayAnimClip(GameObject go, AnimationClip clip)
    {
        Debug.Log("Start picking up dropper procedurally");
        float timeElapsed = 0f, duration = clip.length;
        Debug.Log("Duration: " + duration);
        while (timeElapsed < duration)
        {
            clip.SampleAnimation(go, timeElapsed);
            
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        
        clip.SampleAnimation(go, duration);
        Debug.Log("Finished picking up dropper procedurally");
    }
    
} // End of class
