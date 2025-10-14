using System;
using UnityEngine;

public class Drip : MonoBehaviour
{
    private static readonly int Dilate = Animator.StringToHash("Dilate");
    public float speed;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(0f, 0f, speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Eyeball"))
        {
            Debug.Log("[Drip] : Triggering eye dilating animation.");
            
            if (LevelManager.Instance.currentEyeball.WillDilate)
            {
                LevelManager.Instance.pupilAnim.SetBool(Dilate, true);
            }
        }
        
        Destroy(this.gameObject);
    }
}
