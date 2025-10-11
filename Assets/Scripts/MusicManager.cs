using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource backgroundMusic;
    [SerializeField] private AudioSource judgementMusic;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Start with background music
        if (backgroundMusic != null) backgroundMusic.Play();
        if (judgementMusic != null) judgementMusic.Stop();
    }

    /// <summary>
    /// Call this when the judgement stamp is lifted.
    /// </summary>
    public void PlayJudgementMusic()
    {
        if (judgementMusic != null && !judgementMusic.isPlaying)
            judgementMusic.Play();

        if (backgroundMusic != null && backgroundMusic.isPlaying)
            backgroundMusic.Stop();
    }

    /// <summary>
    /// Call this when judgement ends or stamp is put down.
    /// </summary>
    public void PlayBackgroundMusic()
    {
        if (backgroundMusic != null && !backgroundMusic.isPlaying)
            backgroundMusic.Play();

        if (judgementMusic != null && judgementMusic.isPlaying)
            judgementMusic.Stop();
    }
}
