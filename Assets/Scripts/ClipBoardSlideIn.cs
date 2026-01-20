using UnityEngine;

public class ClipboardSlideIn : MonoBehaviour
{
    [Header("Panel Settings")]
    [SerializeField] RectTransform panel;      // the clipboard panel
    [SerializeField] float slideDuration = 0.6f;   // how long the slide takes
    [SerializeField] AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    // curve so it looks smooth

    [Header("Sound Stuff")]
    // [SerializeField] AudioSource audioSource;  // where the sound plays from
    [SerializeField] AudioClip slideSound;     // sound when it starts sliding

    Vector2 startPos;  // where the panel starts (off screen)
    Vector2 finalPos;  // the position I set in the editor

    void Awake()
    {
        // if I forget to drag it in inspector, just grab it
        if (panel == null)
            panel = GetComponent<RectTransform>();

        // save the final position (editor position)
        finalPos = panel.anchoredPosition;

        // start the panel off screen to the right
        startPos = finalPos + new Vector2(1200f, 0f);
        // 1200 is big enough to push it outside screen

        // move it offscreen before animation
        panel.anchoredPosition = startPos;
    }

    void OnEnable()
    {
        // every time it's enabled, it slides in
        StartSlideIn();
    }

    public void StartSlideIn()
    {
        StopAllCoroutines(); // if something else was running
        StartCoroutine(SlideInRoutine());
    }

    public System.Collections.IEnumerator SlideInRoutine()
    {
        // play sound once at the start
        if (slideSound != null)
            SoundManager.Instance.CallSoundPrefabFunction(slideSound, this.gameObject);

        float t = 0f;

        while (t < slideDuration)
        {
            t += Time.deltaTime;

            float normalized = Mathf.Clamp01(t / slideDuration); // 0 → 1
            float eased = slideCurve.Evaluate(normalized);       // smooth curve

            // slide from offscreen to final
            panel.anchoredPosition = Vector2.Lerp(startPos, finalPos, eased);

            yield return null;
        }

        // snap just in case
        panel.anchoredPosition = finalPos;
    }
}
 