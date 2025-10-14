using System;
using System.Collections;
using System.Collections.Generic;
using Tutorial;
using UnityEngine;

public class Speculum : MonoBehaviour
{
    public enum SpeculumState
    {
        OnTray,
        OnAir,
        OnEye
    }
    
    [Header("Animation")]
    [SerializeField] private GameObject speculumGo;
    [SerializeField] private AnimationClip speculumMoveClip;    // 0 -> on tray, clip.length -> on eye
    private Vector2 _lerpInterval;
    private float _currentOnClipTime;
    private Coroutine _speculumResetCoroutine;  // Called when the physical speculum is placed on tray but the one in game is on eye
    [SerializeField] private float speculumResetSpeed;
    [SerializeField] private Eyeball eyeballScript;
    [SerializeField] private bool isResetting;

    [Header("Skinned Mesh Renderers")]
    [SerializeField] private SkinnedMeshRenderer speculumRenderer;
    [SerializeField] private SkinnedMeshRenderer eyeRenderer;

    [Tooltip("0 - Pick up, 1 - Put down")] [SerializeField]
    private List<AudioClip> speculumSounds;

    [Header("Speculum")]
    
    [Tooltip("How fast the speculum tries to match player input when on air")] [SerializeField]
    private float catchUpSpeed;
    
    [SerializeField] private SpeculumState currentSpeculumState;
    [SerializeField] private bool movingTowardsEye;
    
    // private Coroutine _currentEyeCoroutine;
    // private Coroutine _currentSpeculumCoroutine;

    // private bool zoomedIn = false;
    
    
    
    #region Mono
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize
        _lerpInterval.x = 0;
        _lerpInterval.y = speculumMoveClip.length;
        _currentOnClipTime = 0f;
        isResetting = false;
        
        currentSpeculumState = SpeculumState.OnTray;
        movingTowardsEye = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentSpeculumState == SpeculumState.OnTray)
        {
            // On tray
            
        }
        else if (currentSpeculumState == SpeculumState.OnEye)
        {
            // On eye
            if (!isResetting)
                CatchUpClipInput();
        }
        else
        {
            // On air
            LerpOnAirSpeculum();
        }
    }
    
    #endregion

    
    
    #region Input Action Callbacks
    
    /// <summary>
    /// Called when the speculum is picked up from the tray, on-tray button released
    /// </summary>
    public void OnPickedUp()
    {
        // Tutorial
        TutorialManager.Instance?.NotifyObserver();
        
        // Speculum state
        if (_speculumResetCoroutine != null)
        {
            StopCoroutine(ResetSpeculum());
            _speculumResetCoroutine = null;
            isResetting = false;
        }
        
        movingTowardsEye = true;
        currentSpeculumState = SpeculumState.OnAir;
        eyeballScript.canBlinkOrTwitch = false;
        
        // Sound
        SoundManager.Instance.CallSoundPrefabFunction(speculumSounds[0], speculumGo);
    }
    
    /// <summary>
    /// Called when the speculum is placed back on the tray, on-tray button pressed
    /// </summary>
    public void OnPutDown()
    {
        // Speculum state
        movingTowardsEye = false;
        
        if (currentSpeculumState == SpeculumState.OnEye)
        {
            // Reset speculum before becoming on air
            _speculumResetCoroutine = StartCoroutine(ResetSpeculum());
        }
        
        // Sound
        SoundManager.Instance.CallSoundPrefabFunction(speculumSounds[1], speculumGo);
    }
    
    /// <summary>
    /// Called when the speculum starts being pressed
    /// </summary>
    public void OnPullUpStarted()
    {
        if (currentSpeculumState != SpeculumState.OnEye) { return; }

        eyeballScript.currentEyeballState = EyeballState.Agitated;

        // Pull open sound
        SoundManager.Instance.CallSoundPrefabFunction(speculumSounds[2], speculumGo);


        // float currentValue = pullUpAction.action.ReadValue<float>();
        //
        // Debug.Log("currentvalue is :" + currentValue);
        // eyeRenderer.SetBlendShapeWeight(0, currentValue * 100);
        // speculumRenderer.SetBlendShapeWeight(0, currentValue * 100);

        // CameraZoom();
    }
    
    /// <summary>
    /// Called when the speculum stops being pressed
    /// </summary>
    public void OnPullUpEnded()
    {
        // if (!_speculumPickedUp) return;

        eyeballScript.currentEyeballState = EyeballState.Idling;
    }
    
    #endregion
    
    
    
    #region Methods
    
    private void LerpOnAirSpeculum()
    {
        if (currentSpeculumState != SpeculumState.OnAir)
        {
            return;
        }

        if (movingTowardsEye)
        {
            _currentOnClipTime += Time.deltaTime;
            if (_currentOnClipTime >= _lerpInterval.y)
            {
                // Just being on eye
                _currentOnClipTime = _lerpInterval.y;
                currentSpeculumState = SpeculumState.OnEye;
            }
        }
        else
        {
            _currentOnClipTime -= Time.deltaTime;
            if (_currentOnClipTime <= _lerpInterval.x)
            {
                // Just being on tray
                _currentOnClipTime = _lerpInterval.x;
                currentSpeculumState = SpeculumState.OnTray;
                eyeballScript.canBlinkOrTwitch = true;
            }
        }
        
        speculumMoveClip.SampleAnimation(speculumGo, _currentOnClipTime);
    }

    
    private void CatchUpClipInput()
    {
        float currentWeight = speculumRenderer.GetBlendShapeWeight(0);                  // 0 ~ 100
        float triggerInput = InputManager.Instance.PullUpAction.action.ReadValue<float>();      // 0 ~ 1
        float targetWeight = triggerInput * 100;                                                // 0 ~ 100

        float difference = Mathf.Abs(targetWeight - currentWeight);
        float speed = difference * catchUpSpeed;
        
        speculumRenderer.SetBlendShapeWeight(
            0,
            Mathf.MoveTowards(
                currentWeight,
                targetWeight, 
                speed * Time.deltaTime));
        
        // Make the eye follow the speculum
        SyncEyeOpenWithSpeculum();
    }
    
    /// <summary>
    /// Helper function to reset the speculum on eye and the eye as well before becoming on air
    /// </summary>
    /// <returns>
    /// yield return null
    /// </returns>
    private IEnumerator ResetSpeculum()
    {
        Debug.Log("Speculum Reset started!");
        isResetting = true;
        while (speculumRenderer.GetBlendShapeWeight(0) > 0.5f)
        {
            // Debug.Log("speculum BS weight: " + speculumRenderer.GetBlendShapeWeight(0));
            // Speculum renderer weight
            var currentWeight = speculumRenderer.GetBlendShapeWeight(0);
            // Reset speculum
            speculumRenderer.SetBlendShapeWeight(
                0,
                Mathf.MoveTowards(
                    currentWeight,
                    0,
                    speculumResetSpeed * Time.deltaTime));
            
            // Reset eye in a way that it follows the speculum
            SyncEyeOpenWithSpeculum();

            yield return null;
        }

        isResetting = false;
        currentSpeculumState = SpeculumState.OnAir;
        eyeballScript.currentEyeballState = EyeballState.Idling;
        _speculumResetCoroutine = null;
        Debug.Log("Speculum Reset finished!");
    }

    private void SyncEyeOpenWithSpeculum()
    {
        eyeballScript.bsWeights[(int)Eyeball.BlendShapes.Open] = speculumRenderer.GetBlendShapeWeight(0);
    }
    
    #endregion


    // private void CameraZoom()
    // {
    //     Vector3 zoomPos = new Vector3(0, 0, -8);
    //     Vector3 originalPos = new Vector3(0, 0, -10);
    //     if (!zoomedIn)
    //     {
    //
    //         float cameraZ = Camera.main.transform.position.z;
    //         cameraZ = Mathf.Lerp(originalPos.z, zoomPos.z, 1);
    //         zoomedIn = true;
    //     }
    //     else
    //     {
    //         Camera.main.transform.position = new Vector3(0, 0, -10);
    //         zoomedIn = false;
    //     }
    // }

    // private void StartTransition(float eyeTarget, float specTarget)
    //{
    //    float currentSpecWeight = speculumRenderer.GetBlendShapeWeight(0);
    //    float currentEyeWeight = eyeRenderer.GetBlendShapeWeight(0);

    //    if (currentEyeCoroutine != null)
    //    {
    //        StopCoroutine(currentEyeCoroutine);
    //    }

    //    if (currentSpecCoroutine != null)
    //    {
    //        StopCoroutine(currentSpecCoroutine);
    //    }

    //    currentSpecCoroutine = StartCoroutine(ChangeBlendShapeWeight(currentSpecWeight, specTarget, speculumRenderer));
    //    currentEyeCoroutine = StartCoroutine(ChangeBlendShapeWeight(currentEyeWeight, eyeTarget, eyeRenderer));
    //}

    //private IEnumerator ChangeBlendShapeWeight(float start, float target, SkinnedMeshRenderer renderer)
    //{
    //    float duration = 2f;
    //    float elapsedTime = 0f;


    //    while (elapsedTime < duration)
    //    {
    //        elapsedTime += Time.deltaTime;
    //        float t = elapsedTime / duration;

    //        float currentWeight = Mathf.Lerp(start, target, t);
    //        renderer.SetBlendShapeWeight(0, currentWeight);

    //        yield return null;
    //    }

    //    renderer.SetBlendShapeWeight(0, target);

    //}
}

