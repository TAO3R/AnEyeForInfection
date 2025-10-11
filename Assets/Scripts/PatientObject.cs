using UnityEngine;
using UnityEngine.UIElements;


[CreateAssetMenu(fileName = "PatientObject", menuName = "Scriptable Objects/PatientObject")]
public class PatientObject : ScriptableObject
{
    #region Variabels
    
    [Tooltip("Whether the patient is infected")]
    [SerializeField] private bool isInfected;
    
    [Tooltip("How will the eye twitch")]
    [SerializeField] private EyeTwitchDegree twitchDegree;
    
    [Tooltip("must assign in the inspector")]
    [SerializeField] private EyeObject leftEye, rightEye;

    [Tooltip("Assign in the inspector")]
    [SerializeField] private Material patientAvatar;

    [Tooltip("Assigned at runtime")]
    [System.NonSerialized]
    public Transform idCard;
    
    [Tooltip("The number of people that gets killed if this patient is let in, assign in the inspector")]
    [SerializeField] private int peopleKilled;

    [Tooltip("The line texts patient will speak when enter the scene, judged for infected, and judged for accepted")] [SerializeField]
    private string entryText, infectedText, acceptedText;

    [Tooltip("Audio clip of the voice of the patient")] [SerializeField]
    private AudioClip voice;
   
    
    
    #endregion
    
    #region Getters
    
    public bool IsInfected => isInfected;
    public EyeTwitchDegree TwitchDegree => twitchDegree;
    public EyeObject LeftEye => leftEye;
    public EyeObject RightEye => rightEye;
    // public bool BothEyeDilate => leftEye.WillDilate && rightEye.WillDilate;
    public Material PatientAvatar => patientAvatar;
    public int PeopleKilled => peopleKilled;
    public string EntryText => entryText;
    public string InfectedText => infectedText;
    public string AcceptedText => acceptedText;

    public AudioClip Voice => voice;    

    #endregion

}   // End of class
