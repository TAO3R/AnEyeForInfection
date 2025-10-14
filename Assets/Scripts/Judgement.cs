using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Judgement : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator judgementAnimator;

    private bool isJudgementHeld = false; // true = stamp lifted / ready to use
    private bool hasJudged = false;       // true = a judgement has already been made this lift

    private GameObject currentIDCard;
    private GameObject acceptedSprite;
    private GameObject infectedSprite;

    [Tooltip("0 - pick up, 1 - innocent, 2 - infected")] [SerializeField]
    private List<AudioClip> sounds;

    // used by dialogue system class to tell when infected button is pressed
    public bool judgedInfected;

    [SerializeField] private EndOfDayPopup endOfDay;

    private int patientsKilled = 0;
    public int PatientsKilled => patientsKilled;

    private int infectedAccepted = 0;
    public int InfectedAccepted => infectedAccepted;

    [SerializeField] private GameObject spotlightGo;

    [SerializeField] private Animator pupilAnim;
    
    

    /// <summary>
    /// When the stamp is placed back to the table
    /// </summary>
    public void OnJudgementPressed()
    {
        isJudgementHeld = false; // stamp resting → cannot judge
        hasJudged = false;       // reset for next lift
        Debug.Log("Judgement stamp resting (locked)");
        
        // Initiate ID Card movement
        LevelManager.Instance.IDCardZoomOut();

        // play sound effect
        SoundManager.Instance.CallSoundPrefabFunction(sounds[0], this.gameObject);
        
        // if (judgementAnimator != null)
        //     judgementAnimator.SetTrigger("PutDown");
    }
    
    /// <summary>
    /// When the stamp is picked up
    /// </summary>
    public void OnJudgementReleased()
    {
        isJudgementHeld = true; // stamp lifted → can judge
        hasJudged = false;      // reset judgement for this lift
        Debug.Log("Judgement stamp lifted (ready)");
        
        // Initiate ID Card movement
        LevelManager.Instance.IDCardZoomIn();

        // Switch to judgement music
        MusicManager.Instance?.PlayJudgementMusic();

        // Get current patient ID card from LevelManager
        currentIDCard = LevelManager.Instance?.CurrentPatient?.idCard?.gameObject;

        if (currentIDCard != null)
        {
            acceptedSprite = currentIDCard.transform.Find("ID/AcceptedIcon")?.gameObject;
            infectedSprite = currentIDCard.transform.Find("ID/InfectedIcon")?.gameObject;

            if (acceptedSprite != null) acceptedSprite.SetActive(false);
            if (infectedSprite != null) infectedSprite.SetActive(false);
        }

        // if (judgementAnimator != null)
        // {
        //     Debug.Log("Picking up the stamp");
        //     judgementAnimator.SetTrigger("PickedUp");
        // }
    }
    
    /// <summary>
    /// When the accepted button is hit
    /// </summary>
    public void OnAccepted()
    {
        if (!isJudgementHeld || hasJudged || LevelManager.Instance.StampIsMoving)
            return;
        
        if (judgementAnimator != null)
            judgementAnimator.SetTrigger("Stamp");
        
        judgedInfected = false;
    }
    
    /// <summary>
    /// When the infected button is hit
    /// </summary>
    public void OnInfected()
    {
        if (!isJudgementHeld || hasJudged || LevelManager.Instance.StampIsMoving)
            return;
        
        if (judgementAnimator != null)
            judgementAnimator.SetTrigger("Stamp");
        
        judgedInfected = true;
        
        //Debug.Log("before, value is " + judgedInfected);
        //judgedInfected = false;
        //Debug.Log("after, value is " + judgedInfected);
    }

    public void StampHelper()
    {
        // Activate correct sprite and initiate card move
        if (judgedInfected)
        {
            patientsKilled++;
            Debug.Log("Judgement: Infected executed");
        
            // Check if you judged the patient correctly
            if (!LevelManager.Instance.CurrentPatient.IsInfected)
            {
                endOfDay.pplTurnedAway++;
            }
        
            // Play infected sound effect
            SoundManager.Instance.CallSoundPrefabFunction(sounds[2], this.gameObject);
        
            // play blood splatter (could also handle in dialogue system script)
        
            // show blood splatter on tray
        
            // Play dialogue
            DialogueSystem.Instance.CallWriteText(LevelManager.Instance.CurrentPatient.InfectedText);
        
            if (infectedSprite != null)
                infectedSprite.SetActive(true);
        }
        else
        {
            Debug.Log("Judgement: Accepted executed");
        
            // Check if you judged the patient correctly
            if (LevelManager.Instance.CurrentPatient.IsInfected)
            {
                // Population decreases if wrong
                endOfDay.population -= LevelManager.Instance.CurrentPatient.PeopleKilled;
                infectedAccepted++;
            }
        
            // Play sound effect for accepting patient
            SoundManager.Instance.CallSoundPrefabFunction(sounds[1], this.gameObject);
        
            // Play dialogue
            DialogueSystem.Instance.CallWriteText(LevelManager.Instance.CurrentPatient.AcceptedText);
        
            if (acceptedSprite != null)
                acceptedSprite.SetActive(true);
        }
        
        
        pupilAnim.SetBool("Dilate", false);
        hasJudged = true;
        isJudgementHeld = false;
        
        // Return to background music
        MusicManager.Instance?.PlayBackgroundMusic();
        
        LevelManager.Instance.StartIDSlide();

        //StartCoroutine(WaitToTransition());
        LevelManager.Instance.PatientTransition();
        LevelManager.Instance.spotlightGO.SetActive(true);
    }

    private IEnumerator WaitToTransition()
    {
        yield return new WaitForSecondsRealtime(3f);
    }

    }   // End of class
