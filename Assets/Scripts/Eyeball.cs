using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public enum EyeballState
{
    Idling,         // Small micro jitter + occasional corrective saccade
    StartTracking,  // Start looking at a target
    Tracking,       // Looking at a target
    EndTracking,    // Stop looking at a target
    Agitated        // More frequent/larger saccade
}

public enum EyeTwitchDegree
{
    None,
    Small,
    Medium,
    Large
}

public class Eyeball : MonoBehaviour
{   
    /// <summary>
    /// Used as more descriptive int indices to access and modify BlendShape parameters
    /// </summary>
    public enum BlendShapes
    {
        Blink,
        Twitch,
        Open,
        Up,
        Down,
        Left,
        Right
    }
    
    #region Breath
    
    [Header("Breath")]
    [Tooltip("Translation range (in meters) along Y-axis")] [SerializeField]
    private float amplitude;
    
    [Tooltip("Rotation range (in degrees) along X-axis")] [SerializeField]
    private float rotationAmplitude;

    [Tooltip("Number of breath per second")] [SerializeField]
    private float baseFrequency;

    [Tooltip("Allowed offset (in meters) in Y-axis")] [SerializeField]
    private float noiseAmplitude;

    [Tooltip("How often are noise occuring")] [SerializeField]
    private float noiseFrequency;
    
    [SerializeField] private float minPauseInterval;
    [SerializeField] private float maxPauseInterval;
    [SerializeField] private float minPauseDuration;
    [SerializeField] private float maxPauseDuration;
    
    private Vector3 _rootBasePos;
    private Quaternion _rootBaseRot;
    private float _breathTime;
    private bool _breathPaused;
    private float _nextPauseTime, _pauseEndTime;

    #endregion
    
    
    
    #region Rotation
    
    [Header("Rotate")]
    
    // Idling
    [SerializeField] private float microRange;                              // tiny jitter (degrees)
    [SerializeField] private float correctionRange;                         // occasional correction
    [SerializeField] private float saccadeSpeed;                            // fast snap speed
    [SerializeField] private Vector2 microInterval;
    [SerializeField] private Vector2 correctionInterval;
    
    // Tracking
    [SerializeField] private Transform eyeballTrackTarget;
    
    // Agitated
    [SerializeField] private float agitatedMicroRange;                      // tiny jitter (degrees)
    [SerializeField] private float agitatedCorrectionRange;                 // occasional correction
    [SerializeField] private float agitatedSaccadeSpeed;                    // fast snap speed
    [SerializeField] private Vector2 agitatedMicroInterval;
    [SerializeField] private Vector2 agitatedCorrectionInterval;
    private Quaternion _eyeballDynamicBaseRot;
    [SerializeField] private Vector2 baseRotResetInterval;                  // Reset base rotation to (0, 0)
    private float _nextBaseResetTime;
    
    // Shared
    [SerializeField] private EyeballState currentEyeballState;
    [SerializeField] private Transform eyeballTrans;
    private Quaternion _eyeballBaseRot;
    private Quaternion _eyeballTargetRot;
    private float _nextMicroTime;
    private float _nextCorrectionTime;
    
    #endregion
    
    
    
    #region BlendShape
    
    [Header("BlendShape")]
    
    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;

    private int _bsCount;

    [Tooltip("Add elements in the inspector so that the array length matches number of bs parameters")]
    public float[] bsWeights;

    [SerializeField] private EyeTwitchDegree twitchDegree;

    [SerializeField] private AnimationClip blinkClip, smallTwitchClip, mediumTwitchClip, largeTwitchClip;

    private Coroutine _blinkCoroutine;
    private Coroutine _twitchCoroutine;

    [SerializeField] private GameObject eyeNeutral;

    private bool canBlinkOrTwitch;
    
    
    #endregion
    
    
    
    #region Mono
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Seed random number generator
        Random.InitState((int)System.DateTime.Now.Ticks);
        
        // Breath
        _rootBasePos = transform.localPosition;
        _rootBaseRot = transform.localRotation;
        _breathTime = 0f;
        _breathPaused = false;
        ScheduleNextPause();
        
        // Rotation
        currentEyeballState = EyeballState.Idling;
        _eyeballBaseRot = eyeballTrans.localRotation;   // Expected to be (0, 0)
        _eyeballTargetRot = _eyeballBaseRot;
        ScheduleMicro();
        ScheduleCorrection();
        eyeballTrackTarget = null;
        
        // BlendShape
        Mesh mesh = skinnedMeshRenderer.sharedMesh;
        _bsCount = mesh.blendShapeCount;
        
        for (int i = 0; i < _bsCount; i++)
        {
            bsWeights[i] = 0f;
        }   // Initialization

        _blinkCoroutine = null;
        _twitchCoroutine = null;

        canBlinkOrTwitch = true;
    }

    // Update is called once per frame
    void Update()
    {
        // Breath
        UpdateBreath();
        
        // Rotation
        UpdateRotation();
        
        // BlendShape
        MapBSToRotation();
        SetBSWeights();
    }
    
    #endregion

    
    
    #region Breath Methods
    
    /// <summary>
    /// Used within Update()
    /// </summary>
    private void UpdateBreath()
    {
        // Time passed since enable
        float time = Time.time;

        if (_breathPaused)
        {
            if (time >= _pauseEndTime && _pauseEndTime > 0) // The last pause, which was not manual, has ended
            {
                ResumeBreathing();
            }
            else
            {
                return; // Hold breath
            }
        }
        else if (time >= _nextPauseTime)    // Time to pause
        {
            _breathPaused = true;
            _pauseEndTime = time + Random.Range(minPauseDuration, maxPauseDuration);
            return;
        }
        
        // Advances breath clock
        _breathTime += Time.deltaTime;
        
        // Y-axis translation
        float breath = Mathf.Sin(_breathTime * baseFrequency * Mathf.PI * 2f) * amplitude;
        float micro = Mathf.Sin(_breathTime * noiseFrequency * Mathf.PI * 2f) * noiseAmplitude;
        transform.localPosition = _rootBasePos + new Vector3(0f, breath + micro, 0f);
        
        // X-axis rotation
        float rotationOffset = Mathf.Sin(_breathTime * baseFrequency * Mathf.PI * 2f) * rotationAmplitude;
        transform.localRotation = _rootBaseRot * Quaternion.Euler(rotationOffset, 0f, 0f);
    }
    
    /// <summary>
    /// Helper function to manually pause breath
    /// </summary>
    public void PauseBreathing()
    {
        _breathPaused = true;
        _pauseEndTime = 0f;
    }
    
    /// <summary>
    /// Helper function to manually resume breath and get the next timestamp to automatically pause breath
    /// </summary>
    public void ResumeBreathing()
    {
        _breathPaused = false;
        ScheduleNextPause();
    }
    
    /// <summary>
    /// Helper function to get the next timestamp to pause breath
    /// </summary>
    public void ScheduleNextPause()
    {
        _nextPauseTime = Time.time + Random.Range(minPauseInterval, maxPauseInterval);
    }
    
    #endregion
    
    
    
    #region Eyeball Rotation Methods
    
    /// <summary>
    /// Based on the current eyeball state, update eyeball rotation, called within Update()
    /// </summary>
    private void UpdateRotation()
    {
        switch (currentEyeballState)
        {
            case EyeballState.Idling:
                HandleEyeballIdling();
                break;
            case EyeballState.Tracking:
                HandleEyeballTracking();
                break;
            case EyeballState.Agitated:
                HandleEyeballAgitated();
                break;
        }

        float currentSpeed = GetSaccadeSpeed(currentEyeballState);
        
        eyeballTrans.localRotation = Quaternion.RotateTowards(
            eyeballTrans.localRotation,
            _eyeballTargetRot,
            currentSpeed * Time.deltaTime);
    }
    
    private void HandleEyeballIdling()
    {
        // Micro jitter
        if (Time.time >= _nextMicroTime)
        {
            // Off-base
            float x = Random.Range(-microRange, microRange);
            float y = Random.Range(-microRange, microRange);
            _eyeballTargetRot = _eyeballBaseRot * Quaternion.Euler(x, y, 0);
            
            ScheduleMicro();
        }
        
        // Corrective saccade
        if (Time.time >= _nextCorrectionTime)
        {
            // Off-base
            float x = Random.Range(-correctionRange, correctionRange);
            float y = Random.Range(-correctionRange, correctionRange);
            _eyeballTargetRot = _eyeballBaseRot * Quaternion.Euler(x, y, 0);
            
            // Immediately return to base
            Invoke(nameof(ReturnToBase), 1f);
            
            ScheduleCorrection();
        }
    }
    
    private void HandleEyeballTracking()
    {
        // Make the eyeball look at the direction of the object, with clamped value
        if (!eyeballTrackTarget)
        {
            HandleEyeballIdling();
        }
        else
        {
            
        }
    }

    private void HandleEyeballAgitated()
    {
        // Initialization
        if (_eyeballDynamicBaseRot == Quaternion.identity)
        {
            _eyeballDynamicBaseRot = _eyeballBaseRot;
        }
        
        // Micro jitter
        if (Time.time > _nextMicroTime)
        {
            float x = Random.Range(-agitatedMicroRange, agitatedMicroRange);
            float y = Random.Range(-agitatedMicroRange, agitatedMicroRange);
            _eyeballTargetRot = _eyeballDynamicBaseRot * Quaternion.Euler(x, y, 0);
            
            ScheduleAgitatedMicro();
        }
        
        // Corrective saccade
        if (Time.time > _nextCorrectionTime)
        {
            float x = Random.Range(-agitatedCorrectionRange, agitatedCorrectionRange);
            float y = Random.Range(-agitatedCorrectionRange, agitatedCorrectionRange);
            _eyeballDynamicBaseRot = _eyeballBaseRot * Quaternion.Euler(x, y, 0);

            _eyeballTargetRot = _eyeballDynamicBaseRot;
            ScheduleAgitatedCorrection();
        }
        
        if (_nextBaseResetTime <= 0)
        {
            ScheduleNextRotBaseReset();
        }
        
        // Occasional reset to the fixed base
        if (Time.time > _nextBaseResetTime)
        {
            _eyeballDynamicBaseRot = _eyeballBaseRot;
            _eyeballTargetRot = _eyeballBaseRot;
            _nextBaseResetTime = 0f;
        }
    }

    private void ReturnToBase()
    {
        _eyeballTargetRot = _eyeballBaseRot;
    }

    private void ScheduleMicro()
    {
        _nextMicroTime = Time.time + Random.Range(microInterval.x, microInterval.y);
    }

    private void ScheduleCorrection()
    {
        _nextCorrectionTime = Time.time + Random.Range(correctionInterval.x, correctionInterval.y);
    }

    private void ScheduleAgitatedMicro()
    {
        _nextMicroTime = Time.time + Random.Range(agitatedMicroInterval.x, agitatedMicroInterval.y);
    }

    private void ScheduleAgitatedCorrection()
    {
        _nextCorrectionTime = Time.time + Random.Range(agitatedCorrectionInterval.x, agitatedCorrectionInterval.y);
    }

    private void ScheduleNextRotBaseReset()
    {
        _nextBaseResetTime = Time.time + Random.Range(baseRotResetInterval.x, baseRotResetInterval.y);
    }
    
    /// <summary>
    /// Helper function to change eyeball state
    /// </summary>
    /// <param name="newState">
    /// The state to change eyeball into
    /// </param>
    public void SetEyeballState(EyeballState newState)
    {
        Debug.Log("Changing eyeball state to: " + newState);
        currentEyeballState = newState;
    }

    public void SetEyeballCanBlinkOrTwitch(bool can)
    {
        canBlinkOrTwitch = can;
    }

    public void SetEyeballTrackingTargetTrans(Transform targetTrans)
    {
        eyeballTrackTarget = targetTrans;
    }

    private float GetSaccadeSpeed(EyeballState state)
    {
        return state == EyeballState.Agitated ? agitatedSaccadeSpeed : saccadeSpeed;
    }
    
    /// <summary>
    /// Maps 4 direction BS parameters to current eyeball rotation
    /// </summary>
    private void MapBSToRotation()
    {
         // Up (0, 100) <=> x(0, -13)
         // Down (0, 100) <=> x(0, 13)
         // Left(0, 100) <=> y(0, 21)
         // Right(0, 100) <=> y(0, -25)

         float x = NormalizeEulerAngle(eyeballTrans.localEulerAngles.x);
         float y = NormalizeEulerAngle(eyeballTrans.localEulerAngles.y);
         
         // Up Down Left Right => bsWeights[3-6]
         bsWeights[(int)BlendShapes.Up] = MapClamped(x, 0f, -13f, 0f, 100f);
         bsWeights[(int)BlendShapes.Down] = MapClamped(x, 0f, 13f, 0f, 100f);
         bsWeights[(int)BlendShapes.Left] = MapClamped(y, 0f, 21f, 0f, 100f);
         bsWeights[(int)BlendShapes.Right] = MapClamped(y, 0f, -25f, 0f, 100f);
    }

    private float NormalizeEulerAngle(float angle)
    {
        if (angle > 180f) { angle -= 360f; }
        return angle;
    }
    
    /// <summary>
    /// Maps a value from one range to another (linearly).
    /// Clamps the result to [toMin, toMax].
    /// </summary>
    public static float MapClamped(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        float t = Mathf.InverseLerp(fromMin, fromMax, value); // normalized & clamped
        return Mathf.Lerp(toMin, toMax, t);
    }
    
    #endregion

    
    
    #region BlendShape
    
    /// <summary>
    /// Helper function to feed BlendShape parameters stored in this script to renderer
    /// </summary>
    private void SetBSWeights()
    {
        for (int i = 0; i < _bsCount; i++)
        {
            skinnedMeshRenderer.SetBlendShapeWeight(i, bsWeights[i]);
        }
    }

    public void InitiateBlink()
    {
        // If already blinking or twitch, ignore this invoke
        if (_blinkCoroutine != null || _twitchCoroutine != null || !canBlinkOrTwitch)
        {
            return;
        }
        
        _blinkCoroutine = StartCoroutine(StartEyeballAnim(eyeNeutral, blinkClip));
    }

    public void InitiateTwitch(EyeTwitchDegree degree)
    {
        // If already blinking or twitch, ignore this invoke
        if (_blinkCoroutine != null || _twitchCoroutine != null || !canBlinkOrTwitch)
        {
            return;
        }
        
        AnimationClip thisClip;
        switch (degree)
        {
            case EyeTwitchDegree.Small:
                thisClip = smallTwitchClip;
                break;
            case EyeTwitchDegree.Medium:
                thisClip = mediumTwitchClip;
                break;
            case EyeTwitchDegree.Large:
                thisClip = largeTwitchClip;
                break;
            default:
                Debug.Log("Somehow invoking a twitch on a patient that cannot twitch, returning.");
                return;
        }
        
        _twitchCoroutine = StartCoroutine(StartEyeballAnim(eyeNeutral, thisClip));
    }

    public void StopBlink()
    {
        if (_blinkCoroutine != null)
        {
            StopCoroutine(_blinkCoroutine);
        }
    }

    public void StopTwitch()
    {
        if (_twitchCoroutine != null)
        {
            StopCoroutine(_twitchCoroutine);
        }
    }

    private IEnumerator StartEyeballAnim(GameObject go, AnimationClip clip)
    {
        float timeElapsed = 0f;

        while (timeElapsed <= clip.length)
        {
            clip.SampleAnimation(go, timeElapsed);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        
        clip.SampleAnimation(go, clip.length);
        
        // Twitch and blink cannot happen at the same time
        _blinkCoroutine = null;
        _twitchCoroutine = null;
    }
    
    #endregion
    
}   // End of class
