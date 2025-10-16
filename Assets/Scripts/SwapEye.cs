using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public class SwapEye : MonoBehaviour
{
    [Header("Input Action")]
    // [SerializeField] private InputActionReference swapEyeAction;

    [SerializeField] private Transform eyeModel;

    [SerializeField] private PlayableDirector sequenceDirector;


    private void OnEnable()
    {
        // swapEyeAction.action.performed += OnEyeSwap;
        // swapEyeAction.action.Enable();
    }

    private void OnDisable()
    {
        // swapEyeAction.action.performed -= OnEyeSwap;
        // swapEyeAction.action.Disable();
    }

    private void OnEyeSwap(InputAction.CallbackContext ctx)
    {
        Debug.Log("swapping eyes");

        // swap bool to other eye; left eye will always be shown by default
        // LevelManager.Instance.currentEyeIsLeft = !LevelManager.Instance.currentEyeIsLeft;
        // LevelManager.Instance.SetCurrentEyeball();

        // Light out
        sequenceDirector.Play();
        StartCoroutine(DelayChanges());
    }

    private void FlipEye()
    {
        Vector3 currentScale = eyeModel.localScale;
        currentScale.x *= -1;
        eyeModel.localScale = currentScale;
    }

    private IEnumerator DelayChanges()
    {
        yield return new WaitForSeconds(1.5f);
        FlipEye();
        LevelManager.Instance.UpdateEyeball();
    }
}
