using NUnit.Framework;
using UnityEngine;


public class EyeDropper : MonoBehaviour
{
    #region Animation Events

    public void SetEyeDroppingMoving()
    {
        InputManager.Instance.SetDroppingMoving();
    }

    public void SetEyeDropperPickedUp()
    {
        InputManager.Instance.SetDropperPickedUp();
    }
    
    public void SetEyeDropperOnTray()
    {
        InputManager.Instance.SetDropperOnTray();
    }
        
    #endregion
    
}
