using UnityEngine;
using UnityEngine.InputSystem;

public class JournalToggle : MonoBehaviour
{
    [Header("Input Action")]
    [SerializeField] private InputActionReference toggleJournalAction;

    [Header("Journal State")]
    [SerializeField] private bool isJournalOpen = false;

    [Header("Optional Animator")]
    [SerializeField] private Animator journalAnimator;

    private void OnEnable()
    {
        toggleJournalAction.action.performed += OnToggleJournal;
        toggleJournalAction.action.Enable();
    }

    private void OnDisable()
    {
        toggleJournalAction.action.performed -= OnToggleJournal;
        toggleJournalAction.action.Disable();
    }

    private void OnToggleJournal(InputAction.CallbackContext ctx)
    {
        isJournalOpen = !isJournalOpen;

        if (isJournalOpen)
        {
            Debug.Log("Journal opened");
            if (journalAnimator != null)
                journalAnimator.SetTrigger("OpenJournal");
        }
        else
        {
            Debug.Log("Journal closed");
            if (journalAnimator != null)
                journalAnimator.SetTrigger("CloseJournal");
        }
    }
}
