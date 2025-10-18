using UnityEngine;
using TMPro;
using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndOfDayPopup : MonoBehaviour
{
    public static EndOfDayPopup Instance;

    [Header("UI References")]
    [SerializeField] private CanvasGroup popupCanvasGroup;
    [SerializeField] private TMP_Text dayHeaderText;   // <- NEW
    [SerializeField] private TMP_Text statsText;
    
    [Tooltip("Background image component for the end-of-day stat screen")]
    [SerializeField] private Image panelImage;
    
    [Tooltip("Actual sprites of the background image")]
    [SerializeField] private Sprite correctBackground, mistakeBackground;

    public bool hasFalseNegative;

    //[SerializeField] private CanvasGroup newspaperCanvasGroup;

    //[SerializeField] private Image newspaper;

    [SerializeField] private int dayNumber;

    public int population = 404;
    public int pplTurnedAway = 0;
    //public int dayNumber = 0;

    [Header("Settings")]
    [SerializeField] private float showDuration = 5f;
    [SerializeField] private float fadeDuration = 1f;

    private Coroutine popupRoutine;

    [SerializeField]
    private string nextScene;

    [SerializeField] private Judgement judgement;

    

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    private void Start()
    {
        popupCanvasGroup.alpha = 0f;
        popupCanvasGroup.gameObject.SetActive(false);

        //newspaperCanvasGroup.alpha = 0f;
        //newspaperCanvasGroup.gameObject.SetActive(false);
        
        // Have not judged an innocent as infected at the start
        hasFalseNegative = false;
    }

    /// <summary>
    /// Shows the popup with the given day number and stats.
    /// </summary>
    public void ShowPopup()
    {
        // Delay Pop Up by 3 seconds 
        StartCoroutine(WaitToPopUp());

        
    }

    private IEnumerator PopupSequence()
    {
        popupCanvasGroup.gameObject.SetActive(true);

        // Fade in stats screen
        yield return FadeCanvasGroup(popupCanvasGroup,0f, 1f, 0.5f);

        // Wait visible
        yield return new WaitForSecondsRealtime(showDuration);

        //// Fade out stats screen
        //yield return FadeCanvasGroup(popupCanvasGroup, 1f, 0f, fadeDuration);

        SceneManager.LoadScene(nextScene);


        //// Call same process for newspaper canvas group
        //newspaperCanvasGroup.gameObject.SetActive(true);

        //yield return FadeCanvasGroup(newspaperCanvasGroup, 0f, 1f, 0.5f);

        //yield return new WaitForSecondsRealtime(showDuration);

        //yield return FadeCanvasGroup(newspaperCanvasGroup, 1f, 0f, 0.5f);

        //newspaperCanvasGroup.gameObject.SetActive(false);
        //newspaperCanvasGroup = null;
        //// Get next newspaper

        // Change scene depending on day
        SceneManager.LoadScene(nextScene);

    }

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float from, float to, float duration)
    {
        float elapsed = 0f;
        canvasGroup.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = to;
    }

    private IEnumerator WaitToPopUp()
    {
        yield return new WaitForSecondsRealtime(3f);

        dayHeaderText.text = $"End of Day {dayNumber}";
        statsText.text = "Population: " + population + "\n" 
            + "Infected People Accepted: " + judgement.InfectedAccepted + "\n" +
            "Innocent People Rejected: " + pplTurnedAway;
        
        // Background image
        panelImage.sprite = hasFalseNegative ? mistakeBackground : correctBackground;

        if (popupRoutine != null)
            StopCoroutine(popupRoutine);

        popupRoutine = StartCoroutine(PopupSequence());
    }

}
