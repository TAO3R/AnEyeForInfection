using UnityEngine;
using UnityEngine.UIElements;

public enum Bloodshot
{
    NotAssigned,
    NoBloodshot,
    Type1,
    Type2,
    Type3,
    Type4,
    Type5
}

public enum EyeColor
{
    NotAssigned,
    Brown,
    Black,
    Green,
    Blue,
    White
}

public enum SkinColor
{
    NotAssigned,
    Color1,
    Color2,
    Color3,
    Color4,
    Color5
}

[CreateAssetMenu(fileName = "PatientObject", menuName = "Scriptable Objects/PatientObject")]
public class PatientObject : ScriptableObject
{
    #region Variabels
    
    [Header("Patient")]
    
    [SerializeField] private bool isInfected;
    [SerializeField] private Material patientIdPhoto;

    [SerializeField] private SkinColor skinColorType;

    // Assigned during runtime
    [System.NonSerialized] public Transform idCard;
    
    [Tooltip("The number of people that gets killed if this patient is let in, assign in the inspector")]
    [SerializeField] private int peopleKilled;

    [Tooltip("The line texts patient will speak when enter the scene, judged for infected, and judged for accepted")] [SerializeField]
    private string entryText, infectedText, acceptedText;

    [Tooltip("Audio clip of the voice of the patient")] [SerializeField]
    private AudioClip voice;
    
    
    
    [Header("Eyeball")]
    
    [Tooltip("The type of bloodshot this eye has")]
    [SerializeField] private Bloodshot bloodshotType;
    
    [Tooltip("Eye color of this eye")]
    [SerializeField] private EyeColor eyeColorType;

    [Tooltip("Whether the eye will track tools when they are picked up")]
    [SerializeField] private bool willTrackTool;
    
    
    
    [Header("Dilate")]
   
    [Tooltip("Whether this eye will dilate")]
    [SerializeField] private bool willDilate;



    [Header("Blink")]
    
    [Tooltip("The lower and upper bounds of cooldowns between two blinks")]
    [SerializeField] private Vector2 blinkCd;
    
    [Tooltip("Used to modify the distribution of blink cd. A curve of y = x will have the range of blink cd being equally sampled")]
    [SerializeField] private AnimationCurve blinkCdSampler;
    
    
    
    [Header("Twitch")]
    
    [Tooltip("The lower and upper bounds of cooldowns between two twitches")]
    [SerializeField] private Vector2 twitchCd;
    
    [Tooltip("Used to modify the distribution of twitch cd. A curve of y = x will have the range of twitch cd being equally sampled")]
    [SerializeField] private AnimationCurve twitchCdSampler;
    
    [Tooltip("How will the eye twitch")]
    [SerializeField] private EyeTwitchDegree twitchDegree;
    
    #endregion
    
    
    
    #region Getters
    
    // Patient
    public bool IsInfected => isInfected;
    public Material PatientIdPhoto => patientIdPhoto;
    public SkinColor SkinColorType => skinColorType == SkinColor.NotAssigned ? SkinColor.Color1 : skinColorType;
    public int PeopleKilled => peopleKilled;
    public string EntryText => entryText;
    public string InfectedText => infectedText;
    public string AcceptedText => acceptedText;
    public AudioClip Voice => voice;    
    
    // Eyeball
    public Bloodshot BloodshotType => bloodshotType == Bloodshot.NotAssigned ? Bloodshot.NoBloodshot : bloodshotType;
    public EyeColor ColorType => eyeColorType == EyeColor.NotAssigned ? EyeColor.Black : eyeColorType;
    public bool WillTrackTool => willTrackTool;
    
    // Dilate
    // Only won't dilate if the patient is infected and set as cannot dilate
    public bool WillDilate => (!isInfected || willDilate);
    
    // Blink
    public Vector2 BlinkCd => blinkCd == Vector2.zero ? 
        (isInfected ? new Vector2(5, 8) : new Vector2(7, 10)) :
        blinkCd;
    public AnimationCurve BlinkCdSampler => blinkCdSampler.keys.Length > 0 ?
        blinkCdSampler :
        AnimationCurve.Linear(0, 0, 1, 1);
    
    // Twitch
    public Vector2 TwitchCd => twitchCd == Vector2.zero ?
        new Vector2(4, 7) :
        twitchCd;

    public AnimationCurve TwitchCdSampler =>
        twitchCdSampler.keys.Length > 0 ? twitchCdSampler : AnimationCurve.Linear(0, 0, 1, 1);
    public EyeTwitchDegree TwitchDegree => twitchDegree;
    
    
    
    // Won't have bloodshot if the patient is not infected, default to type 1 if is infected and not assigned a type
    // public Bloodshot BloodshotType => patient.IsInfected ?
    //                                   (bloodshotType == Bloodshot.NotAssigned ? Bloodshot.Type1 : bloodshotType) : 
    //                                   Bloodshot.NoBloodshot;
    
    #endregion

}   // End of class
