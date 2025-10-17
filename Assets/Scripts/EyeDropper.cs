using System;
using System.Collections;
using UnityEditor.EditorTools;
using UnityEngine;

public enum EyeDropperStates
{
    OnTray,
    PickedUp,
    Moving,
    Dripping
}


public class EyeDropper : MonoBehaviour
{
    #region Animator Hashed String
    
    private static readonly int Rise = Animator.StringToHash("Rise");
    private static readonly int Drip = Animator.StringToHash("Drip");
    private static readonly int Reset = Animator.StringToHash("Reset");
    
    #endregion

    [Header("Animators")]
    public Animator dropperAnim;
    public Animator pupilAnim;
    
    [Header("Dropper Settings")]
    [SerializeField] private EyeDropperStates eyeDropperCurrentState;
    public EyeDropperStates EyeDropperCurrentState => eyeDropperCurrentState;
    [SerializeField] private float dripInterval;
    private float dripCooldown;
    

    [SerializeField] private GameObject drip;
    [SerializeField] private Transform dropperTip;
    
    [Header("Procedural Anim")]
    [SerializeField]
    private AnimationClip dropperPickUpClip;
    private GameObject dropperGo;

    [Header("Eyeball")] [SerializeField] private Eyeball eyeballScript;
    [SerializeField] private Speculum speculumScript;
    

    private void Start()
    {
        // State
        eyeDropperCurrentState = EyeDropperStates.OnTray;
        
        // Settings
        dripCooldown = dripInterval;
        
        // Reference
        dropperGo = this.gameObject;
    }

    private void Update()
    {
        if (dripCooldown > 0)
        {
            dripCooldown -= Time.deltaTime;
        }
    }

    #region Input Action Callbacks
    
    /// <summary>
    /// Called when the dropper-on-tray button is released
    /// </summary>
    public void OnPickedUp()
    {
        if (eyeDropperCurrentState == EyeDropperStates.OnTray)
        {
            // Animation
            dropperAnim.SetTrigger(Rise);
            
            // Eyeball
            eyeballScript.SetEyeballState(EyeballState.Tracking);
            eyeballScript.SetEyeballTrackingTargetTrans(dropperTip);
        }
    }
    
    /// <summary>
    /// Called when the dropper-on-tray button is pressed
    /// </summary>
    public void OnPutDown()
    {
        if (eyeDropperCurrentState == EyeDropperStates.PickedUp)
        {
            // Animation
            dropperAnim.SetTrigger(Reset);
            
            // State
            eyeDropperCurrentState = EyeDropperStates.OnTray;
            
            // Eyeball
            eyeballScript.SetEyeballState(EyeballState.Tracking);
            eyeballScript.SetEyeballTrackingTargetTrans(dropperTip);
        }
    }
    
    /// <summary>
    /// Called when the drip button is pressed
    /// </summary>
    public void OnDrip()
    {
        if (eyeDropperCurrentState == EyeDropperStates.PickedUp)
        {
            if (dripCooldown > 0)
            {
                return;
            }
            
            // Reset cooldown
            dripCooldown = dripInterval;
            
            // Animation
            dropperAnim.SetTrigger(Drip);
            
            // State
            eyeDropperCurrentState = EyeDropperStates.Dripping;
            
            // Instantiate a drip
            GameObject tempDrip = Instantiate(drip, dropperTip.position, Quaternion.identity);
            tempDrip.SetActive(true);
            
            // Eyeball slight
            
        }
    }
    
    #endregion
    
    
    
    #region Animation Events

    public void SetEyeDropperMoving()
    {
        eyeDropperCurrentState = EyeDropperStates.Moving;
    }

    public void SetEyeDropperPickedUp()
    {
        eyeDropperCurrentState = EyeDropperStates.PickedUp;
        
        // Eyeball
        if (InputManager.Instance.PullUpAction.action.ReadValue<float>() > 0
            &&
            speculumScript.CurrentSpeculumState == Speculum.SpeculumState.OnEye)
        {
            eyeballScript.SetEyeballState(EyeballState.Agitated);
        }
        else
        {
            eyeballScript.SetEyeballState(EyeballState.Idling);
        }
        
        eyeballScript.SetEyeballTrackingTargetTrans(null);
    }
    
    public void SetEyeDropperOnTray()
    {
        eyeDropperCurrentState = EyeDropperStates.OnTray;
        
        // Eyeball
        if (InputManager.Instance.PullUpAction.action.ReadValue<float>() > 0
            &&
            speculumScript.CurrentSpeculumState == Speculum.SpeculumState.OnEye)
        {
            eyeballScript.SetEyeballState(EyeballState.Agitated);
        }
        else
        {
            eyeballScript.SetEyeballState(EyeballState.Idling);
        }
        eyeballScript.SetEyeballTrackingTargetTrans(null);
    }
    
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
    
}    // End of class
