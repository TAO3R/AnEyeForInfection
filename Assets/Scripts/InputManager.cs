using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    // Singleton
    public static InputManager Instance;

    [Header("Eye Dropper Input Actions")]
    [SerializeField] private EyeDropper dropperScript;
    [SerializeField] private InputActionReference prepareAction; // pickup/place button
    [SerializeField] private InputActionReference dripAction;    // drip button

    [Header("Speculum Input Actions")]
    [SerializeField] private Speculum speculumScript;
    [SerializeField] private InputActionReference speculumPickup;
    [SerializeField] private InputActionReference pullUpAction;
    
    // Exposed for the speculum to read input value from the pressure-sensitive clip
    public InputActionReference PullUpAction => pullUpAction;

    [Header("Stamp Input Actions")]
    [SerializeField] private Judgement stampScript;
    [SerializeField] private InputActionReference judgementAction;
    [SerializeField] private InputActionReference acceptedAction;
    [SerializeField] private InputActionReference infectedAction;
    
    
    #region Mono

    private void Awake()
    {
        // Singleton
        Instance = this;
    }

    private void OnEnable()
    {
        // Eye dropper
        prepareAction.action.started += OnDropperPutDown;          // button pressed -> put on tray
        prepareAction.action.canceled += OnDropperPickedUp;        // button released -> pick up

        dripAction.action.started += OnDropperDrip;                // separate drip button

        prepareAction.action.Enable();
        dripAction.action.Enable();
        
        // Speculum
        speculumPickup.action.started += OnSpeculumPutDown;
        speculumPickup.action.canceled += OnSpeculumPickedUp;
        
        pullUpAction.action.started += OnSpeculumPullUpStarted;
        pullUpAction.action.canceled += OnSpeculumPullUpEnded;

        speculumPickup.action.Enable();
        pullUpAction.action.Enable();
        
        // Stamp
        // On-tray button
        judgementAction.action.started += OnStampPutDown;
        judgementAction.action.canceled += OnStampPickedUp;

        // Accepted/Infected buttons
        acceptedAction.action.performed += OnStampingAccepted;
        infectedAction.action.performed += OnStampingInfected;

        judgementAction.action.Enable();
        acceptedAction.action.Enable();
        infectedAction.action.Enable();
    }

    private void OnDisable()
    {
        // Eye dropper
        // On-tray button
        prepareAction.action.started -= OnDropperPutDown;
        prepareAction.action.canceled -= OnDropperPickedUp;
        
        // Binary clip inside the dropper
        dripAction.action.started -= OnDropperDrip;

        prepareAction.action.Disable();
        dripAction.action.Disable();
        
        // Speculum
        // On-tray button
        speculumPickup.action.started -= OnSpeculumPutDown;
        speculumPickup.action.canceled -= OnSpeculumPickedUp;
        
        // Pressure-sensitive clip
        pullUpAction.action.started -= OnSpeculumPullUpStarted;
        pullUpAction.action.canceled -= OnSpeculumPullUpEnded;
        
        speculumPickup.action.Disable();
        pullUpAction.action.Disable();
        
        // Stamp
        // On-tray button
        judgementAction.action.started -= OnStampPutDown;
        judgementAction.action.canceled -= OnStampPickedUp;

        // Accepted/Infected buttons
        acceptedAction.action.performed -= OnStampingAccepted;
        infectedAction.action.performed -= OnStampingInfected;

        judgementAction.action.Disable();
        acceptedAction.action.Disable();
        infectedAction.action.Disable();
    }

    private void Update()
    {
        // Debug.Log($"Current speculum button value: {speculumPickup.action.ReadValue<float>()}");
    }

    #endregion
    
    
    #region Input Action Callbacks
    
    
    // Pick up the dropper when button is released and it is on the tray
    private void OnDropperPickedUp(InputAction.CallbackContext ctx)
    {
        Debug.Log("[InputManager]: Try pick up the dropper.");
        if (LevelManager.Instance.CurrentState != LevelStates.Judging) return;
        
        dropperScript.OnPickedUp();
    }

    // Place the dropper back on the tray when button is pressed, and it is picked up
    private void OnDropperPutDown(InputAction.CallbackContext ctx)
    {
        if (LevelManager.Instance.CurrentState != LevelStates.Judging) return;
        
        dropperScript.OnPutDown();
    }

    // Drop a drip while the dropper being picked up
    private void OnDropperDrip(InputAction.CallbackContext ctx)
    {
        if (LevelManager.Instance.CurrentState != LevelStates.Judging) return;
        
        dropperScript.OnDrip();
    }
    
    // Pick up the speculum
    private void OnSpeculumPickedUp(InputAction.CallbackContext ctx)
    {
        Debug.Log("[InputManager]: Try pick up the speculum.");
        if (LevelManager.Instance.CurrentState != LevelStates.Judging) return;
        
        speculumScript.OnPickedUp();
    }
    
    // Put down the speculum
    private void OnSpeculumPutDown(InputAction.CallbackContext ctx)
    {
        if (LevelManager.Instance.CurrentState != LevelStates.Judging) return;
        
        speculumScript.OnPutDown();
    }
    
    // Pull the speculum open
    private void OnSpeculumPullUpStarted(InputAction.CallbackContext ctx)
    {
        if (LevelManager.Instance.CurrentState != LevelStates.Judging) return;
        
        speculumScript.OnPullUpStarted();
    }
    
    // Stop pull the speculum open
    private void OnSpeculumPullUpEnded(InputAction.CallbackContext ctx)
    {
        if (LevelManager.Instance.CurrentState != LevelStates.Judging) return;
        
        speculumScript.OnPullUpEnded();
    }
    
    // Pick up the stamp
    private void OnStampPickedUp(InputAction.CallbackContext ctx)
    {
        Debug.Log("[InputManager]: Try pick up the stamp.");
        if (LevelManager.Instance.CurrentState != LevelStates.Judging) return;
        
        stampScript.OnJudgementReleased();
    }
    
    // Put down the stamp
    private void OnStampPutDown(InputAction.CallbackContext ctx)
    {
        if (LevelManager.Instance.CurrentState != LevelStates.Judging) return;
            
        stampScript.OnJudgementPressed();
    }
    
    // Stamping accepted button
    private void OnStampingAccepted(InputAction.CallbackContext ctx)
    {
        if (LevelManager.Instance.CurrentState != LevelStates.Judging) return;
        
        stampScript.OnAccepted();
    }
    
    // Stamping infected button
    private void OnStampingInfected(InputAction.CallbackContext ctx)
    {
        if (LevelManager.Instance.CurrentState != LevelStates.Judging) return;

        stampScript.OnInfected();
    }

    #endregion
    
}   // End of class
