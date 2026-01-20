using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Random = UnityEngine.Random;

public enum LevelStates
{
    Entering,
    Judging,
    Transitioning,
    Exiting
}

[System.Serializable]
public class PatientPool
{
    public List<PatientObject> pool;
}

public class LevelManager : MonoBehaviour
{
    // Singleton
    public static LevelManager Instance { get; private set; }

    private LevelStates currentState;
    public LevelStates CurrentState => currentState;
    
    // Patients
    // [SerializeField] private List<PatientPool> patientPools;
    [SerializeField] private int firstPatientIndex;
    [SerializeField] private int patientNumber;
    private List<PatientObject> patientList;
    
    // Eyeball
    [SerializeField] private Eyeball eyeballScript;
    [SerializeField] private Animator eyeballAnim;
    public Animator pupilAnim;
    
    // Tools
    [SerializeField] private Speculum speculumScript;
    [SerializeField] private EyeDropper dropperScript;
    
    // Voices
    [SerializeField] private bool toolPickUpFlag, stampPickUpFlag, idleFlag;
    public bool ToolPickupFlag => toolPickUpFlag;
    public bool StampPickUpFlag => stampPickUpFlag;
    [SerializeField] private float idleCounter;
    
    // Cutscene
    [Tooltip("Assign in the inspector")]
    [SerializeField]
    private PlayableDirector sequenceDirector;
    
    // UI
    [SerializeField]
    private EndOfDayPopup endOfDay;
    
    // Tray Materials
    [SerializeField] private List<Material> materials;
    [SerializeField] private MeshRenderer trayRenderer;

    [Tooltip("0 - safe, 1 - infected")]
    [SerializeField] private List<TimelineAsset> cutscenes;
    
    [SerializeField] private Judgement judgement;
    
    [SerializeField] private GameObject lightsGo;
    [SerializeField] private GameObject eyeGo;
    
    public GameObject spotlightGo;
    [SerializeField] private bool spotLightOffWhenZoomed = true;
    
    
    //private float textDelay;
    
    
    #region ID Card & Stamp Variables
    
    [Header("ID Card & Stamp")]
    
    [Tooltip("Assign in the inspector")] [SerializeField]
    private Transform idCardRotationCenter;
    
    [Tooltip("Assign in the inspector")] [SerializeField]
    private GameObject idCardPrefab;
    
    [Tooltip("Assign in the inspector")] [SerializeField]
    private float maxRotationZ, rotationZRange;
    
    [Tooltip("Assign in the inspector")] [SerializeField]
    private float idCardThickness;
    
    [Tooltip("Assign in the inspector")] [SerializeField]
    private Transform idZoomInTrans;
    
    [Tooltip("Assign in the inspector")] [SerializeField]
    private float zoomDuration;

    [Tooltip("Assign in the inspector")] [SerializeField]
    private Transform stampTrans;

    [Tooltip("Assign in the inspector")] [SerializeField]
    private Transform stampOnTrayTrans, stampOnAirTrans;

    [Tooltip("Assign in the inspector")] [SerializeField]
    private Animator stampAnim;

    [Tooltip("Assign in the inspector")] [SerializeField]
    private Transform idDestroyTrans;

    [Tooltip("Assign in the inspector")] [SerializeField]
    private float waitToChangePatient;

    [Tooltip("Assign in the inspector")] [SerializeField]
    private AudioClip patientSwapSFX;
    
    #endregion
    
    
    #region Debug Variables

    [Header("Debugging")]
    
    private int currentPatientIndex;
    
    [SerializeField]
    private PatientObject currentPatient;
    public PatientObject CurrentPatient => currentPatient;
    
    // Keep track of the time remaining for the current patient to blink & twitch
    [SerializeField]
    private float blinkCd, twitchCd;

    [SerializeField] private Vector3 currentIDCardOnTrayTempLocalPos;
    [SerializeField] private Quaternion currentIDCardOnTrayTempLocalRot;
    [SerializeField] private bool stampIsMoving;
    public bool StampIsMoving => stampIsMoving;
    [SerializeField] private bool isZoomedIn;
    private Coroutine _currentIDZoomCoroutine;
    private Coroutine _currentStampZoomCoroutine;
    
    public TMP_Text patientInfoDebugText;
    public TMP_Text countdownDebugText;

    [Header("Cutscene")]
    [SerializeField] private float trayMaterialChangeDelay;
    
    #endregion
    
    
    #region Mono
    
    private void Awake()
    {
        // Singleton
        Instance = this;
        
        // Populate patient list
        // Random.InitState((int)System.DateTime.Now.Ticks);
        patientList = new List<PatientObject>();
        for (int i = 0; i < patientNumber; i++)
        {
            patientList.Add(PatientPoolManager.Instance.IsUsingGroup1 ?
                PatientPoolManager.Instance.PatientPools[0].pool[firstPatientIndex + i]
                :
                PatientPoolManager.Instance.PatientPools[1].pool[firstPatientIndex + i]);
        }
        
        // State
        currentState = LevelStates.Entering;
    }

    private void OnEnable()
    {
        sequenceDirector.played += OnPlayablePlayed;
        sequenceDirector.stopped += OnPlayableStopped;
    }

    private void OnDisable()
    {
        sequenceDirector.played -= OnPlayablePlayed;
        sequenceDirector.stopped -= OnPlayableStopped;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("LevelManager.Instance: " + LevelManager.Instance);
        Debug.Log("LevelManager.Instance.CurrentPatient: " + LevelManager.Instance.CurrentPatient);
        
        // Seed the random number generator
        Random.InitState((int)System.DateTime.Now.Ticks);
        
        // Place ID cards of this level
        SetUpIDCards();
        
        // Get the first patient in the list
        currentPatientIndex = -1;
        GetPatient();
        
        // Cooldowns
        GetNewBlinkCd();
        
        if (currentPatient.TwitchDegree != EyeTwitchDegree.None)
        {
            GetNewTwitchCd();
        }
        
        // Initialize ID Card & stamp
        currentIDCardOnTrayTempLocalPos = Vector3.zero;
        currentIDCardOnTrayTempLocalRot = Quaternion.identity;
        stampIsMoving = false;
        isZoomedIn = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!currentPatient) { return; }
        
        // Cooldowns
        if (currentPatient.WillBlink)
        {
            blinkCd -= Time.deltaTime;
        }

        if (currentPatient.TwitchDegree != EyeTwitchDegree.None)
        {
            twitchCd -= Time.deltaTime;
        }

        if (currentPatient.WillBlink && blinkCd < 0)
        {
            // Trigger blink
            // InputManager.Instance.eyeballAnim.SetTrigger(currentPatient.IsInfected ? "Twitch" : "Blink");
            eyeballScript.InitiateBlink();

            // Update cooldown
            blinkCd = GetNewBlinkCd();
        }

        if (currentPatient.TwitchDegree != EyeTwitchDegree.None && twitchCd < 0)
        {
            // Trigger twitch
            eyeballScript.InitiateTwitch(currentPatient.TwitchDegree);
            
            // Update cooldown
            twitchCd = GetNewTwitchCd();
        }

        countdownDebugText.text = $"Blink cooldown: {blinkCd}\n" +
                                  $"Twitch cooldown: {twitchCd}";

        //Debug.Log("ENTRY TEXT IS: " + CurrentPatient.EntryText);

        if (!idleFlag && currentState == LevelStates.Judging)
        {
            idleCounter -= Time.deltaTime;

            if (idleCounter <= 0f)
            {
                idleFlag = true;
                
                if (currentPatient.IdleText != "")
                {
                    // Trigger text box display and voice
                    DialogueSystem.Instance.CallWriteText(currentPatient.IdleText, DialogueInterruptLevel.InteractiveDialogue);
                    
                }
            }
        }

    }
    
    #endregion
    
    
    #region Methods
    
    // Get the next patient in the list
    private void GetPatient()
    {
        currentPatientIndex++;
        
        if (currentPatientIndex == patientList.Count)
        {
            // Implement end game logic
            // endOfDay.dayNumber++;
            // StartCoroutine(WaitToShowBlood());
            endOfDay.ShowPopup();
            lightsGo.SetActive(false);
            
            Debug.LogError("The patient list is empty!");
            return;
        }

        // Get the first in the list and remove it from the list
        currentPatient = patientList[currentPatientIndex];
        
        // Reset flags
        judgement.judgedInfected = false;
        ClearToolPickUpFlag();
        ClearStampPickUpFlag();
        ClearIdleFlag();
        
        if (currentPatient.IdleDuration <= 0f)
        {
            idleFlag = true;
        }
        else
        {
            idleCounter = currentPatient.IdleDuration;
            Debug.Log($"$[Idle Text]: idle counter set to {idleCounter}");
        }
        
        // Update visuals
        UpdateEyeball();

        // Update text box
        // StartCoroutine(WaitToShowBlood());
        StartCoroutine(WaitToWriteText());
        
        spotlightGo.SetActive(true);
        
        // Debug
        Debug.Log("Current eye twitch degree: " + currentPatient.TwitchDegree);

        patientInfoDebugText.text = $"Current patient info:\n" + 
                                    $"\tIsInfected: {currentPatient.IsInfected}\n" +
                                    $"\tSkinColor: {currentPatient.SkinColorType}\n" +
                                    $"\tInnocentPoints: {currentPatient.InnocentPoints}\n" +
                                    $"\tBloodshotType: {currentPatient.BloodshotType}\n" +
                                    $"\tEyeColorType: {currentPatient.ColorType}\n" +
                                    $"\tWillSaccade: {currentPatient.WillSaccade}\n" +
                                    $"\tWillAgitate: {currentPatient.WillAgitate}\n" +
                                    $"\tWillTrackTool: {currentPatient.WillTrackTool}\n" +
                                    $"\tLookAtCameraWhileIdling: {currentPatient.LookAtCameraWhileIdling}\n" +
                                    $"\tWillDilate: {currentPatient.WillDilate}\n" +
                                    $"\tWillBlink: {currentPatient.WillBlink}\n" +
                                    $"\tTwitchDegree: {currentPatient.TwitchDegree}";
    }

    // Update Eyeball visuals to the current eyeball
    public void UpdateEyeball()
    {
        Debug.Log("[LevelManager]: updating eyeball");
        
        eyeballScript.UpdateEyeballAppearance(currentPatient);

        pupilAnim.SetBool("Dilate", false);
    }

    /// Get a new blink cooldown for the current patient
    private float GetNewBlinkCd()
    {
        return Mathf.Lerp(
                currentPatient.BlinkCd.x,
                currentPatient.BlinkCd.y,
                Mathf.InverseLerp(
                    currentPatient.BlinkCdSampler.keys.First().value,
                    currentPatient.BlinkCdSampler.keys.Last().value,
                    currentPatient.BlinkCdSampler.Evaluate(
                        Random.Range(
                            currentPatient.BlinkCdSampler.keys.First().time, 
                            currentPatient.BlinkCdSampler.keys.Last().time))));
        
        return Random.Range(currentPatient.BlinkCd.x, currentPatient.BlinkCd.y);
    }
    
    /// Get a new twitch cooldown for the current patient, if it will twitch
    private float GetNewTwitchCd()
    {
        return Mathf.Lerp(
            currentPatient.TwitchCd.x,
            currentPatient.TwitchCd.y,
            Mathf.InverseLerp(
                currentPatient.TwitchCdSampler.keys.First().value,
                currentPatient.TwitchCdSampler.keys.Last().value,
                currentPatient.TwitchCdSampler.Evaluate(
                    Random.Range(
                        currentPatient.TwitchCdSampler.keys.First().time, 
                        currentPatient.TwitchCdSampler.keys.Last().time))));

        return Random.Range(currentPatient.TwitchCd.x, currentPatient.TwitchCd.y);
    }

    public void SetToolPickUpFlag()
    {
        toolPickUpFlag = true;
    }

    public void ClearToolPickUpFlag()
    {
        toolPickUpFlag = false;
    }

    public void SetStampPickUpFlag()
    {
        stampPickUpFlag = true;
    }

    public void ClearStampPickUpFlag()
    {
        stampPickUpFlag = false;
    }

    public void SetIdleFlag()
    {
        idleFlag = true;
    }

    public void ClearIdleFlag()
    {
        idleFlag = false;
    }
    
    // Place ID cards in the scene at the start of the level
    private void SetUpIDCards()
    {
        // Reverse the patient list so that the first patient is placed at the top
        patientList.Reverse();
        
        int index;
        float rotationGap = 0;
        Debug.Log("Patient number: " + patientList.Count + ", rotationZRange: " + rotationZRange);
        if (patientList.Count - 1 > 0)
        {
            rotationGap = rotationZRange / (patientList.Count - 1);
        }
        
        for (index = 0; index < patientList.Count; index++)
        {
            // Position & Rotation 
            GameObject tempID = Instantiate(idCardPrefab, idCardRotationCenter);
            tempID.transform.localPosition = new Vector3(0f, 0f, index * -0.05f);
            tempID.transform.localRotation = Quaternion.Euler(0f, 0f, maxRotationZ - index * rotationGap);
            
            // Assign reference
            patientList[index].idCard = tempID.transform;

            // Patient Info
            // throw new NotImplementedException("Need to set up patient info on each ID cards.");
            
            tempID.GetComponentInChildren<Renderer>().material = patientList[index].PatientIdPhoto;
        }
        
        patientList.Reverse();
    }

    // private void ToggleZoom()
    // {
    //     
    // }
    
    /// <summary>
    /// Play ID card zoom in and picking up stamp animation
    /// </summary>
    public void IDCardZoomIn()
    {
        // ID Card's on tray position is stored the first time the player zooms in
        if (currentIDCardOnTrayTempLocalPos == Vector3.zero && currentIDCardOnTrayTempLocalRot == Quaternion.identity)
        {
            currentIDCardOnTrayTempLocalPos = currentPatient.idCard.localPosition;
            currentIDCardOnTrayTempLocalRot = currentPatient.idCard.localRotation;
        }
        
        StartIDZoom(idZoomInTrans.localPosition, idZoomInTrans.localRotation, zoomDuration);
        StartStampZoom(stampOnAirTrans, zoomDuration);
        isZoomedIn = true;
        stampAnim.SetBool("PickedUp", true);
        stampAnim.SetBool("PutDown", false);
        
        // Spot light off
        if (spotLightOffWhenZoomed)
            spotlightGo.SetActive(false);
    }
    
    /// <summary>
    /// Play ID card zoom out and putting down stamp animation
    /// </summary>
    public void IDCardZoomOut()
    {
        if (currentIDCardOnTrayTempLocalPos == Vector3.zero && currentIDCardOnTrayTempLocalRot == Quaternion.identity)
        {
            currentIDCardOnTrayTempLocalPos = currentPatient.idCard.localPosition;
            currentIDCardOnTrayTempLocalRot = currentPatient.idCard.localRotation;
            return;
        }
        
        StartIDZoom(currentIDCardOnTrayTempLocalPos, currentIDCardOnTrayTempLocalRot, zoomDuration);
        StartStampZoom(stampOnTrayTrans, zoomDuration);
        isZoomedIn = false;
        stampAnim.SetBool("PickedUp", false);
        stampAnim.SetBool("PutDown", true);
    }
    
    /// <summary>
    /// Helper function that sets coroutine reference and initiates the coroutine
    /// </summary>
    /// <param name="targetPos">
    /// Target local position in Vector3
    /// </param>
    /// <param name="targetRot">
    /// Target local rotation in Quaternion
    /// </param>
    /// <param name="duration">
    /// Total time (in seconds) of the zoom process
    /// </param>
    private void StartIDZoom(Vector3 targetPos, Quaternion targetRot, float duration)
    {
        if (_currentIDZoomCoroutine != null)
        {
            StopCoroutine(_currentIDZoomCoroutine);
        }
        
        // ID Card
        _currentIDZoomCoroutine = StartCoroutine(LerpToTransform(
            currentPatient.idCard,
            currentPatient.idCard,
            targetPos,
            targetRot,
            duration));
    }
    
    /// <summary>
    /// Helper function that sets coroutine reference and initiates the coroutine
    /// </summary>
    /// <param name="targetTrans">
    /// Target Transform in scene whose local position and rotation will be used for interpolation
    /// </param>
    /// <param name="duration">
    /// Total time (in seconds) of the zoom process
    /// </param>
    private void StartStampZoom(Transform targetTrans, float duration)
    {
        if (_currentStampZoomCoroutine != null)
        {
            StopCoroutine(_currentStampZoomCoroutine);      
        }
        
        // Stamp
        _currentStampZoomCoroutine = StartCoroutine(LerpToTransform(
            stampTrans,
            stampTrans,
            targetTrans.localPosition,
            targetTrans.localRotation,
            duration));
    }
    
    /// <summary>
    /// Helper function to lerp game objects
    /// </summary>
    /// <param name="movingTrans">
    /// The Transform component of the game object that will lerp
    /// </param>
    /// <param name="initialTrans">
    /// Initial Transform information (local position and rotation, or scale)
    /// </param>
    /// <param name="targetPos">
    /// Target local position in Vector3
    /// </param>
    /// <param name="targetRot">
    /// Target local rotation in Quaternion
    /// </param>
    /// <param name="duration">
    /// Total time (in seconds) the process is going to take from start to finish
    /// </param>
    /// <returns>
    /// Returns null while the process is not finished
    /// </returns>
    private IEnumerator LerpToTransform(
        Transform movingTrans,
        Transform initialTrans,
        Vector3 targetPos,
        Quaternion targetRot,
        float duration)
    {
        stampIsMoving = true;
        // Transform idTrans = currentPatient.idCard;
        
        // Store the initial position and rotation
        Vector3 startPos = initialTrans.localPosition;
        Quaternion startRot = initialTrans.localRotation;

        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            var t = timeElapsed / duration;
            
            // Apply quadratic easing (smooth acceleration and deceleration)
            // t * t is a simple quadratic (ease-in), 
            // 1 - (1 - t) * (1 - t) is a quadratic (ease-out)
            // This formula combines both for ease-in-out.
            float smoothedT = t * t * (3f - 2f * t); // Cubic Hermite (Smoothstep) - very smooth

            movingTrans.localPosition = Vector3.Lerp(startPos, targetPos, smoothedT);
            movingTrans.localRotation = Quaternion.Slerp(startRot, targetRot, smoothedT);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        movingTrans.localPosition = targetPos;
        movingTrans.localRotation = targetRot;
        stampIsMoving = false;
        _currentIDZoomCoroutine = null;

        if (!isZoomedIn)
        {
            spotlightGo.SetActive(true);
        }
    }

    private IEnumerator LerpToDestroyID(
        Transform movingTrans,
        Transform initialTrans,
        Vector3 targetPos,
        Quaternion targetRot,
        float duration)
    {
        yield return StartCoroutine(LerpToTransform(
            movingTrans,
            initialTrans,
            targetPos,
            targetRot,
            duration));
        
        // Destroy the current card
        Destroy(currentPatient.idCard.gameObject);

        // Play the sequence
        //sequenceDirector.Play();

        // Play swap sound effect
        //SoundManager.Instance.CallSoundPrefabFunction(patientSwapSFX, this.gameObject);

        // Play timeline and invoke wait time coroutine that calls get next patient
        //yield return new WaitForSecondsRealtime(waitToChangePatient);
        //GetPatient();

    }


    public void PatientTransition()
    {
        if (judgement.judgedInfected)
        {
            //textDelay = 4f;
            sequenceDirector.playableAsset = cutscenes[1];
            Debug.Log("playableAsset is: " +  sequenceDirector.playableAsset);
        }
        else
        {
            sequenceDirector.playableAsset = cutscenes[0];
        }

        Debug.Log("playing everything");
        sequenceDirector.Play();

        StartCoroutine(WaitToShowBlood());
    }

    private IEnumerator WaitToShowBlood()
    {
        Debug.Log("calling waittoshowblood");
        yield return new WaitForSecondsRealtime(trayMaterialChangeDelay);

        trayRenderer.material = materials[judgement.PatientsKilled];
        
        GetPatient();
    }

    private IEnumerator WaitToWriteText()
    {
        yield return new WaitForSecondsRealtime(4f);
        DialogueSystem.Instance.CallWriteText(currentPatient.EntryText, DialogueInterruptLevel.IntroOutroDialogue);
    }

    public void StartIDSlide()
    {
        StartStampZoom(stampOnTrayTrans, zoomDuration);
        StartCoroutine(LerpToDestroyID(
            currentPatient.idCard,
            currentPatient.idCard,
            idDestroyTrans.localPosition,
            idDestroyTrans.localRotation,
            zoomDuration));
    }

    private void OnPlayablePlayed(PlayableDirector director)
    {
        // State
        currentState = LevelStates.Transitioning;
        
        // Reset tools
        speculumScript.OnPutDown();
        dropperScript.OnPutDown();
        
        // Eyeball
        eyeballAnim.enabled = true;
    }

    private void OnPlayableStopped(PlayableDirector director)
    {
        // State
        currentState = currentPatientIndex >= patientList.Count ? LevelStates.Exiting : LevelStates.Judging;
        
        // Eyeball
        eyeballAnim.enabled = false;
    }

    public void SetLevelState(LevelStates state)
    {
        currentState = state;
    }
    
    #endregion
    
}   // End of class
