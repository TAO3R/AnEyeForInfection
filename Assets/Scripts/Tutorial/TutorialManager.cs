using System.Collections.Generic;
using UnityEngine;

namespace Tutorial
{
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance;
        private List<IEventObserver> _eventObservers;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                // If a duplicate exists, destroy it
                Destroy(gameObject);
            }
            
            _eventObservers = new List<IEventObserver>();
        }

        #region  Observer Helepr Methods

        public void AddObserver(IEventObserver observer)
        {
            Debug.Log("Adding observer: " + observer);
            _eventObservers.Add(observer);
        }
        
        public void RemoveObserver(IEventObserver observer)
        {
            _eventObservers.Remove(observer);
        }

        public bool CheckObserver(IEventObserver observer)
        {
            return _eventObservers.Contains(observer);
        }

        public void NotifyObserver()
        {
            foreach (var observer in _eventObservers)
            {
                Debug.Log("notified one");
                observer.CheckEvent();
            }
        }

        #endregion
        
    }   // End of class
}
