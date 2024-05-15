using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public abstract class DrivingControlLayout : MonoBehaviour
    {

        [SerializeField] private string actionMapName;

        public virtual void SetActive()
        {
            gameObject.SetActive(true);
            
            InputManager.Instance.CarInput.SetActionMapName(actionMapName);
        }

        public void Disable()
        {
            gameObject.SetActive(false);    
        }
        
    }
}
