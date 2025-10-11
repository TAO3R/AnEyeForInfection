using UnityEngine;

public class TestAnimation : MonoBehaviour
{
    public Animator animator;

    private void Update()
    {
        if (Input.GetKey(KeyCode.O))
        {
            animator.enabled = true;
        }
    }
}
