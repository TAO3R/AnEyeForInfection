using TMPro;
using UnityEngine;

namespace Tutorial
{
    public class DropperObserver : MonoBehaviour, IEventObserver
    {
        private bool _hasPickedUpDropper;
        [SerializeField] private TextMeshPro dropperTMP;
        [SerializeField] private string dropperBeforeText, dropperAfterText;
        
        #region Mono
        
        private void OnEnable()
        {
            if (TutorialManager.Instance != null && !TutorialManager.Instance.CheckObserver(this))
            {
                TutorialManager.Instance.AddObserver(this);
            }
        }

        private void OnDisable()
        {
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.RemoveObserver(this);
            }
        }
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (TutorialManager.Instance != null && !TutorialManager.Instance.CheckObserver(this))
            {
                TutorialManager.Instance.AddObserver(this);
            }
            
            _hasPickedUpDropper = false;
            dropperTMP.text = dropperBeforeText;
        }
        
        #endregion

        public void CheckEvent()
        {
            if (_hasPickedUpDropper) return;
            
            // The first time the dropper is picked up
            _hasPickedUpDropper = true;
            dropperTMP.text = dropperAfterText;
        }
    }
}
