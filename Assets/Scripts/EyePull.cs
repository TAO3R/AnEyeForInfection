using UnityEngine;

public class EyePull : MonoBehaviour
{
    public Animator eyeAnim;

    // private bool speculum = false;

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.P))
        {
            // speculum = true;

            if (Input.GetKeyDown(KeyCode.W))
            {
                // pull up animation
            }
            if (Input.GetKeyUp(KeyCode.W))
            {
                // transition back to normal
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                // pull down animation
            }

            if (Input.GetKeyUp(KeyCode.S))
            {
                // transition back to normal
            }
        }

        
        
    }
}
