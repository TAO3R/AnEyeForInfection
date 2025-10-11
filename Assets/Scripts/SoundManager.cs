using System.Collections;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Tooltip("Assign in inspector")] [SerializeField]
    private GameObject soundPrefab;

    [Tooltip("Assign in inspector")] [SerializeField]
    private Judgement judgement;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }


    // Generates the sound prefab, then destroys it after the audio is played
    private void GenerateSoundPrefab(AudioClip clip, GameObject obj)
    { 
        // Position of the gameobject to instantiate the prefab at
        Vector3 pos = obj.transform.position;
        GameObject instance = Instantiate(soundPrefab, pos, Quaternion.identity);

        // Play the audio clip
        AudioSource audioSource = instance.GetComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.Play();

        // Length to wait before destroying the prefab
        float clipLength = clip.length;
        StartCoroutine(DestroyPrefab(instance, clipLength));
        
    }

    // Public method to call GenerateSoundPrefab function
    public void CallSoundPrefabFunction(AudioClip clip, GameObject obj)
    {
        GenerateSoundPrefab(clip, obj);
    }

    // Delays destroying the prefab until after it's done playing
    private IEnumerator DestroyPrefab(GameObject instance, float length)
    {
        yield return new WaitForSecondsRealtime(length);
        Destroy(instance);

    }

}
