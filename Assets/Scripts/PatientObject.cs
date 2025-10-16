using UnityEngine;
using UnityEngine.UIElements;


[CreateAssetMenu(fileName = "PatientObject", menuName = "Scriptable Objects/PatientObject")]
public class PatientObject : ScriptableObject
{
    #region Variabels
    
    [Header("Patient")]
    [Tooltip("Whether the patient is infected")]
    [SerializeField] private bool isInfected;
    
    // [Tooltip("Must assign in the inspector")]
    // [SerializeField] private EyeObject leftEye, rightEye;

    [Tooltip("Assign in the inspector")]
    [SerializeField] private Material patientIdPhoto;

    [Tooltip("Assigned at runtime")]
    [System.NonSerialized]
    public Transform idCard;
    
    [Tooltip("The number of people that gets killed if this patient is let in, assign in the inspector")]
    [SerializeField] private int peopleKilled;

    [Tooltip("The line texts patient will speak when enter the scene, judged for infected, and judged for accepted")] [SerializeField]
    private string entryText, infectedText, acceptedText;

    [Tooltip("Audio clip of the voice of the patient")] [SerializeField]
    private AudioClip voice;
    
    [Header("Eyeball")]
    [Tooltip("The type of bloodshot this eye has")]
    [SerializeField] private Bloodshot bloodshotType;
    
    [Tooltip("Whether this eye will dilate")]
    [SerializeField] private bool willDilate;
    
    [Tooltip("How will the eye twitch")]
    [SerializeField] private EyeTwitchDegree twitchDegree;
    
    [Tooltip("Eye color of this eye")]
    [SerializeField] private EyeColor colorType;
   
    
    
    #endregion
    
    #region Getters
    
    public bool IsInfected => isInfected;
    public EyeTwitchDegree TwitchDegree => twitchDegree;
    // public EyeObject LeftEye => leftEye;
    // public EyeObject RightEye => rightEye;
    // public bool BothEyeDilate => leftEye.WillDilate && rightEye.WillDilate;
    public Material PatientIdPhoto => patientIdPhoto;
    public int PeopleKilled => peopleKilled;
    public string EntryText => entryText;
    public string InfectedText => infectedText;
    public string AcceptedText => acceptedText;
    
    // Only won't dilate if the patient is infected and set as cannot dilate
    public bool WillDilate => (!isInfected || willDilate);
    
    // Won't have bloodshot if the patient is not infected, default to type 1 if is infected and not assigned a type
    // public Bloodshot BloodshotType => patient.IsInfected ?
    //                                   (bloodshotType == Bloodshot.NotAssigned ? Bloodshot.Type1 : bloodshotType) : 
    //                                   Bloodshot.NoBloodshot;
    public Bloodshot BloodshotType => bloodshotType;
    
    public EyeColor ColorType => colorType == EyeColor.NotAssigned ? EyeColor.Black : colorType;

    public AudioClip Voice => voice;    

    #endregion

}   // End of class
