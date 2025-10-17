using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "EyeObject", menuName = "Scriptable Objects/EyeObject")]
public class EyeObject : ScriptableObject
{
    #region Variables
    
    [Tooltip("Must assign in the inspector")]
    [SerializeField] private PatientObject patient;
    
    [Tooltip("Whether this eye will dilate")]
    [SerializeField] private bool willDilate;
    
    [Tooltip("The type of bloodshot this eye has")]
    [SerializeField] private Bloodshot bloodshotType;
    
    [Tooltip("Eye color of this eye")]
    [SerializeField] private EyeColor colorType;
    
    #endregion
    
    #region Getters

    public PatientObject Patient => patient;
    
    // Only won't dilate if the patient is infected and set as cannot dilate
    public bool WillDilate => (!patient.IsInfected || willDilate);
    
    // Won't have bloodshot if the patient is not infected, default to type 1 if is infected and not assigned a type
    // public Bloodshot BloodshotType => patient.IsInfected ?
    //                                   (bloodshotType == Bloodshot.NotAssigned ? Bloodshot.Type1 : bloodshotType) : 
    //                                   Bloodshot.NoBloodshot;
    public Bloodshot BloodshotType => bloodshotType;
    
    public EyeColor ColorType => colorType == EyeColor.NotAssigned ? EyeColor.Black : colorType;
    
    #endregion

}   // End of class
