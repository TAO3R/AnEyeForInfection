using System.Collections;
using UnityEngine;
using TMPro;
using NUnit.Framework;

public class DialogueSystem : MonoBehaviour
{
    public static DialogueSystem Instance;

    [Tooltip("Assign in inspector")] [SerializeField] 
    private float typewriterDelay = 0.05f;

    [Tooltip("Assign in inspector")] [SerializeField]
    private TextMeshPro textbox;

    [Tooltip("Assign in inspector")] [SerializeField]
    private TextMeshPro textboxLower;

    [Tooltip("Assign in inspector")] [SerializeField]
    private Judgement judgement;

    private bool isWriting = false;

    // Audio Variables
    [Tooltip("Assign in inspector")] [SerializeField] 
    private AudioClip gunSFX;

    private AudioClip voice;
    private AudioSource audioSource;

    private Coroutine currentCoroutine;
    public Coroutine CurrentCoroutine => currentCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;

        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        
    }

    // Public method to call write text function
    public void CallWriteText(string textToWrite)
    {
        StartWriteText(textToWrite);
    }

    // Calls coroutine if there is not one running
    private void StartWriteText(string textToWrite)
    {
        // Stop current coroutine 
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        currentCoroutine = StartCoroutine(WriteText(textToWrite));
    }


    // Writes text character by character
    private IEnumerator WriteText(string textToWrite)
    {
        isWriting = true;   

        textboxLower.text = "";
        textbox.text = "";

        // Play the current patient's voice while text is being written
        // voice = ;
        Debug.Log("current voice: " + LevelManager.Instance.CurrentPatient.Voice);
        audioSource.clip = LevelManager.Instance.CurrentPatient.Voice;
        audioSource.Play();

        for (var i = 0; i < textToWrite.Length; i++)
        {
            char charToWrite = textToWrite[i];
            textbox.text += charToWrite;

            yield return new WaitForSecondsRealtime(typewriterDelay);
        }

        // Stop patient voice after text is finished writing
        audioSource.Stop();

        // If this is not the infected text, keep on screen
        if (!judgement.judgedInfected)
        {
            // Close textbox after 2.5 seconds
            yield return new WaitForSecondsRealtime(2.5f);

        }
        // Reset textbox at the end
        textbox.text = "";
        isWriting = false;

        // After the dialogue is done typing, play the sequence and start transition
        //LevelManager.Instance.PatientTransition();
    }

    // Helper Methods for Judgement class
    public void StopAudio()
    {
        audioSource.Stop();
    }

    public void ResetText()
    {
        textbox.text = "";
    }


}
