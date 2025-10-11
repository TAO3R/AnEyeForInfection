using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleSceneTransition : MonoBehaviour
{
    [Header("Transition Settings")]
    [SerializeField] private string nextSceneName = "NextScene"; // Name of the scene to load
    [SerializeField] private float delay = 2f; // Delay before transitioning

    private void Start()
    {
        // Start the delayed transition
        Invoke(nameof(LoadNextScene), delay);
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
