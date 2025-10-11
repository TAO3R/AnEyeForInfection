using System;
using TMPro;
using UnityEngine;

namespace Tutorial
{
    public class SpeculumObserver : MonoBehaviour, IEventObserver
    {
        private bool _hasPickedUpSpeculum;
        [SerializeField] private TextMeshPro speculumTMP;
        [SerializeField] private string speculumBeforeText, speculumAfterText;
        
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
            
            _hasPickedUpSpeculum = false;
            speculumTMP.text = speculumBeforeText;
        }
        
        #endregion

        public void CheckEvent()
        {
            if (_hasPickedUpSpeculum) return;
            
            // The first time the speculum is picked up
            _hasPickedUpSpeculum = true;
            speculumTMP.text = speculumAfterText;
        }
    }
}
