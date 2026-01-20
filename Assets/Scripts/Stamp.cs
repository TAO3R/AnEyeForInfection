using UnityEngine;

public class Stamp : MonoBehaviour
{
    [SerializeField] private Judgement judgementScript;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnStampOnCard()
    {
        judgementScript.StampHelper();
    }

    public void OnStampFinished()
    {
        LevelManager.Instance.StartIDSlide();

        //StartCoroutine(WaitToTransition());
        LevelManager.Instance.PatientTransition();
        // LevelManager.Instance.spotlightGo.SetActive(true);
    }
}
