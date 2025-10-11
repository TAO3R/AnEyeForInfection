using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneReset : MonoBehaviour
{
    [SerializeField] private Image newspaper;
    public float fadeDuration = 3f;

    private void Start()
    {
        StartCoroutine(FadeOutNewspaper(newspaper, fadeDuration));
    }

    void Update()
    {
        // Press R to reload the current scene
        // if (Input.GetKeyDown(KeyCode.R))
        // {
        //     Debug.Log("Resetting scene...");
        //     SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        // }

        // Press Escape to quit the game
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Quitting game...");
            Application.Quit();

#if UNITY_EDITOR
            // If running in the editor, stop play mode
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }

    private IEnumerator FadeOutNewspaper(Image image, float duration)
    {
        yield return new WaitForSecondsRealtime(6f);
        
        Color originalColor = image.color;
        float startAlpha = originalColor.a;
        
        float timeElapsed = 0f;
        
        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / duration;
            float newAlpha = Mathf.Lerp(startAlpha, 0f, t);

            image.color = new Color(
                originalColor.r,
                originalColor.g,
                originalColor.b,
                newAlpha);

            yield return null;
        }

        image.color = new Color(originalColor.r,
            originalColor.g,
            originalColor.b,
            0f);
    }
    
}   // End of class
