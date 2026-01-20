using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class FlagEndingVideoManager : MonoBehaviour
{
    private VideoPlayer flagVideoPlayer;
    [SerializeField] private string startSceneName;

    private void Awake()
    {
        flagVideoPlayer = GetComponent<VideoPlayer>();
    }

    private void OnEnable()
    {
        flagVideoPlayer.started += OnFlagVideoStart;
        flagVideoPlayer.loopPointReached += OnFlagVideoFinish;
    }

    private void OnDisable()
    {
        flagVideoPlayer.started -= OnFlagVideoStart;
        flagVideoPlayer.loopPointReached -= OnFlagVideoFinish;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnFlagVideoStart(VideoPlayer vp)
    {
        
    }

    private void OnFlagVideoFinish(VideoPlayer vp)
    {
        SceneManager.LoadScene(startSceneName);
    }
    
}   // End of class
