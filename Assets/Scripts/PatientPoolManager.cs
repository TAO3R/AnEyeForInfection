using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// This script should be at least attached to the day 1 scene
public class PatientPoolManager : MonoBehaviour
{
    // Singleton
    public static PatientPoolManager Instance;
    
    // Patient list
    [SerializeField] private string startSceneName;

    [SerializeField] private List<PatientPool> patientPools;
    public List<PatientPool> PatientPools => patientPools;
    public bool IsUsingGroup1 { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == startSceneName)
        {
            IsUsingGroup1 = (UnityEngine.Random.Range(0, 2) == 0);
        }
    }
    
    
    
    
}   // End of class
