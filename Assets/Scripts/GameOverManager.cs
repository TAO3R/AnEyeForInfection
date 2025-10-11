using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel; // assign your UI panel in the inspector

    private bool isGameOver = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Make sure the UI is hidden at start
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log("GAME OVER: Infected patient accepted!");

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);   // show UI panel
        Time.timeScale = 0f;                 // halt gameplay
    }

    public void ResetGame()
    {
        Time.timeScale = 1f;
        isGameOver = false;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);  // hide UI panel again
    }
}
